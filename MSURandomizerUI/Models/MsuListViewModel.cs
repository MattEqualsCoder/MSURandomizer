using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerUI.Models;

internal sealed class MsuListViewModel : ViewModel
{
    public MsuListViewModel()
    {
    }
    
    public MsuListViewModel(IMsuLookupService msuLookupService)
    {
        if (msuLookupService.Status == MsuLoadStatus.Loaded)
        {
            _allMsus = msuLookupService.Msus;
            Errors = msuLookupService.Errors;
        }
        msuLookupService.OnMsuLookupComplete += MsuLookupServiceOnOnMsuLookupComplete;
    }

    public IReadOnlyCollection<Msu> AvailableMsus => _msuType == null ? new List<Msu>() :
        _allMsus.Where(x => _msuFilter == null || x.MatchesFilter(_msuFilter.Value, _msuType, _basePath))
                .Where(x => x.NumUniqueTracks > x.MsuType?.RequiredTrackNumbers.Count / 5)
                .ToList();

    private IReadOnlyCollection<Msu> _allMsus = new List<Msu>();

    public IReadOnlyCollection<Msu> AllMsus
    {
        get => _allMsus;
        private set
        {
            SetField(ref _allMsus, value);
            OnPropertyChanged(nameof(AvailableMsus));
            MsuListUpdated?.Invoke(this, new MsuListEventArgs(value, null));
            AvailableMsusUpdated?.Invoke(this, new MsuListEventArgs(AvailableMsus, null));
        }
    }
    
    private SelectionMode _selectionMode;

    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => SetField(ref _selectionMode, value);
    }

    private MsuType? _msuType;

    public MsuType? MsuType
    {
        get => _msuType;
        set
        {
            SetField(ref _msuType, value);
            OnPropertyChanged(nameof(AvailableMsus));
            AvailableMsusUpdated?.Invoke(this, new MsuListEventArgs(AvailableMsus, null));
        }
    }

    private MsuFilter? _msuFilter;

    public MsuFilter? MsuFilter
    {
        get => _msuFilter;
        set
        {
            SetField(ref _msuFilter, value);
            OnPropertyChanged(nameof(AvailableMsus));
            AvailableMsusUpdated?.Invoke(this, new MsuListEventArgs(AvailableMsus, null));
        }
    }

    private ICollection<string> _selectedMsuPaths = new List<string>();

    public ICollection<string> SelectedMsuPaths
    {
        get => _selectedMsuPaths;
        set
        {
            SetField(ref _selectedMsuPaths, value);
            AvailableMsusUpdated?.Invoke(this, new MsuListEventArgs(AvailableMsus, null));
        }
    }

    private string? _basePath;

    public string? BasePath
    {
        get => _basePath;
        set
        {
            SetField(ref _basePath, value);
            AvailableMsusUpdated?.Invoke(this, new MsuListEventArgs(AvailableMsus, null));
        }
    }

    private Visibility _msuMonitorWindowMenuItemVisibility = Visibility.Visible;
    public Visibility MsuMonitorWindowMenuItemVisibility
    {
        get => _msuMonitorWindowMenuItemVisibility;
        set => SetField(ref _msuMonitorWindowMenuItemVisibility, value);
    }

    private bool _msuMonitorWindowEnabled = true;
    public bool MsuMonitorWindowEnabled
    {
        get => _msuMonitorWindowEnabled;
        set => SetField(ref _msuMonitorWindowEnabled, value);
    }
    
    
    private void MsuLookupServiceOnOnMsuLookupComplete(object? sender, MsuListEventArgs e)
    {
        AllMsus = e.Msus;
        Errors = e.Errors?.ToDictionary(x => x.Key, x => x.Value);
    }

    public event EventHandler<MsuListEventArgs>? MsuListUpdated;
    
    public event EventHandler<MsuListEventArgs>? AvailableMsusUpdated;

    public IDictionary<string, string>? Errors { get; set; }
}