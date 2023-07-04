using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MsuRandomizerLibrary.Configs;
using MsuRandomizerLibrary.Services;

namespace MsuRandomizerLibrary;

public sealed class MsuListViewModel : INotifyPropertyChanged
{
    public MsuListViewModel()
    {
    }
    
    public MsuListViewModel(IMsuLookupService msuLookupService)
    {
        msuLookupService.OnMsuLookupComplete += MsuLookupServiceOnOnMsuLookupComplete;
    }

    public IReadOnlyCollection<Msu> AvailableMsus =>
        _allMsus.Where(x => _msuFilter == MsuFilter.All || (_msuFilter == MsuFilter.Compatible && x.MsuType?.IsCompatibleWith(_msuType) == true) || x?.MsuTypeName == _msuType.Name).ToList();

    private IReadOnlyCollection<Msu> _allMsus = new List<Msu>();

    public IReadOnlyCollection<Msu> AllMsus
    {
        get => _allMsus;
        private set
        {
            SetField(ref _allMsus, value);
            OnPropertyChanged(nameof(AvailableMsus));
        }
    }

    private SelectionMode _selectionMode;

    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => SetField(ref _selectionMode, value);
    }

    private MsuType _msuType;

    public MsuType MsuType
    {
        get => _msuType;
        set
        {
            SetField(ref _msuType, value);
            OnPropertyChanged(nameof(AvailableMsus));
        }
    }

    private MsuFilter _msuFilter;

    public MsuFilter MsuFilter
    {
        get => _msuFilter;
        set
        {
            SetField(ref _msuFilter, value);
            OnPropertyChanged(nameof(AvailableMsus));
        }
    }

    private ICollection<string> _defaultMsus = new List<string>();
    
    public ICollection<string> SelectedMsuPaths
    {
        get => _defaultMsus;
        set => _defaultMsus = value;
    }

    private void MsuLookupServiceOnOnMsuLookupComplete(object? sender, MsuLookupEventArgs e)
    {
        AllMsus = e.Msus;
        MsuListUpdated?.Invoke(this, new MsuLookupEventArgs(e.Msus));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<MsuLookupEventArgs>? MsuListUpdated;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}