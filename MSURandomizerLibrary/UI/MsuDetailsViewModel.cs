using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.UI;

public class MsuDetailsViewModel : INotifyPropertyChanged
{
    private string _name;
    private string _creator;
    private string _msuTypeName;
    private bool _allowAltTracks;
    private readonly string _originalName;
    private readonly string _originalCreator;
    private readonly string _originalMsuTypeName;
    private readonly bool _originalAllowAltTracks;
    private bool _hasChanges;

    public MsuDetailsViewModel()
    {
        _originalName = _name = "";
        _originalCreator = _creator = "";
        _originalMsuTypeName = _msuTypeName = "";
        Path = "";
        MsuTypeNames = new List<string>();
        Tracks = new List<MsuDetailsTrackViewModel>();
    }
    
    public MsuDetailsViewModel(Msu msu, ICollection<string> msuTypeNames)
    {
        _originalName = _name = msu.DisplayName;
        _originalCreator = _creator = msu.DisplayCreator;
        _originalMsuTypeName = _msuTypeName = msu.MsuTypeName;
        _originalAllowAltTracks = _allowAltTracks = msu.Settings.AllowAltTracks;
        CanViewEditBoxes = !msu.HasDetails;
        Path = msu.Path;
        MsuTypeNames = msuTypeNames;

        Tracks = (msu.SelectedMsuType?.ValidTrackNumbers ?? msu.Tracks.Select(x => x.Number))
            .Order()
            .Select(x => new MsuDetailsTrackViewModel(msu, x))
            .ToList();
    }
    
    
    public string Path { get; }
    
    public bool CanViewEditBoxes { get; }

    public bool CanViewText => !CanViewEditBoxes;
    public string Name
    {
        get => _name;
        set
        {
            SetField(ref _name, value);
            CheckChanges();
        }
    }

    public string Creator
    {
        get => _creator;
        set
        {
            SetField(ref _creator, value);
            CheckChanges();
        }
    }

    public string MsuTypeName
    {
        get => _msuTypeName;
        set
        {
            SetField(ref _msuTypeName, value);
            CheckChanges();
        }
    }
    
    public bool AllowAltTracks
    {
        get => _allowAltTracks;
        set
        {
            SetField(ref _allowAltTracks, value);
            CheckChanges();
        }
    }

    public bool HasChanges
    {
        get => _hasChanges;
        set => SetField(ref _hasChanges, value);
    }

    public ICollection<string> MsuTypeNames { get; }
    
    public ICollection<MsuDetailsTrackViewModel> Tracks { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void CheckChanges(string? name = null, string? creator = null, string? msuTypeName = null)
    {
        HasChanges = (name == null ? _originalName != _name : _originalName != name)
            || (creator == null ? _originalCreator != _creator : _originalCreator != creator)
            || (msuTypeName == null ? _originalMsuTypeName != _msuTypeName : _originalMsuTypeName != msuTypeName)
            || (_originalAllowAltTracks != _allowAltTracks);
    }

    public void ApplyChanges(Msu msu, MsuType? type)
    {
        msu.Settings.Name = Name != msu.Name ? Name : null;

        msu.Settings.Creator = Creator != msu.Creator ? Creator : null;

        msu.Settings.MsuTypeName = type?.Name != msu.MsuType?.Name ? type?.Name : null;
        msu.Settings.MsuType = type;

        msu.Settings.AllowAltTracks = _allowAltTracks;
    }
}