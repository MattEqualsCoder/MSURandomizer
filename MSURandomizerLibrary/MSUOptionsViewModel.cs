using System.ComponentModel;
using System.Windows;

namespace MSURandomizerLibrary;

internal class MSUOptionsViewModel: INotifyPropertyChanged
{
    private readonly MSURandomizerOptions _options;
    
    public MSUOptionsViewModel(MSURandomizerOptions options)
    {
        _options = options;
    }

    public string? Directory
    {
        get => _options.Directory;
        set
        {
            _options.Directory = value;
            OnPropertyChanged(nameof(Directory));
        }
    }

    public bool OpenFolderOnCreate
    {
        get => _options.OpenFolderOnCreate;
        set
        {
            _options.OpenFolderOnCreate = value;
            OnPropertyChanged(nameof(Directory));
        }
    }

    public bool UseFolderNames
    {
        get => _options.UseFolderNames;
        set => _options.UseFolderNames = value;
    }

    public Visibility ShowMsuFolder => _options.AllowMSUFolderChange ? Visibility.Visible : Visibility.Collapsed;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}