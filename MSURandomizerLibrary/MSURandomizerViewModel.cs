using MSURandomizerLibrary.MSUTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MSURandomizerLibrary;

internal class MSURandomizerViewModel: INotifyPropertyChanged
{
    private ICollection<MSU> _msus = new List<MSU>();
    private ICollection<string> _msuTypes = new List<string>();

    public MSURandomizerOptions Options { get; set; } = new();

    public ICollection<MSU> MSUs
    {
        get => _msus;
        set
        {
            _msus = value;
            OnPropertyChanged(nameof(VisibleMSUs));
        }
    }
        
    public ICollection<string> MSUTypes
    {
        get => _msuTypes;
        set
        {
            _msuTypes = value;
            OnPropertyChanged(nameof(SelectedMsuType));
            OnPropertyChanged(nameof(MSUTypes));
            OnPropertyChanged(nameof(VisibleMSUs));
            OnPropertyChanged(nameof(ShowMSUType));
        }
    }
        
    public string? SelectedMsuType
    {
        get => Options.OutputType;
        set
        {
            Options.OutputType = value;
            OnPropertyChanged(nameof(SelectedMsuType));
            OnPropertyChanged(nameof(VisibleMSUs));
        }
    }
        
    public MSUFilter Filter
    {
        get => Options.Filter;
        set
        {
            Options.Filter = value;
            OnPropertyChanged(nameof(Filter));
            OnPropertyChanged(nameof(VisibleMSUs));
        }
    }

    public Visibility ShowMSUName => Options.UseFolderNames ? Visibility.Collapsed : Visibility.Visible;
    public Visibility ShowFolderName => Options.UseFolderNames ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowMSUType =>
        string.IsNullOrEmpty(Options.ForcedMsuType) ? Visibility.Visible : Visibility.Collapsed;

    public IEnumerable<MSU> VisibleMSUs => MSURandomizerService.ApplyFilter(MSUs, Options).OrderBy(x => Options.UseFolderNames ? x.FolderName : x.FileName);

    private bool _canGenerate;

    public bool CanGenerate
    {
        get => _canGenerate;
        set
        {
            _canGenerate = value;
            OnPropertyChanged(nameof(CanGenerate));
        }
    }

    public void UpdateDisplayNames()
    {
        OnPropertyChanged(nameof(ShowMSUName));
        OnPropertyChanged(nameof(ShowFolderName));
        OnPropertyChanged(nameof(VisibleMSUs));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}