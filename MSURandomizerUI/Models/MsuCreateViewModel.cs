using System.Windows;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerUI.Models;

internal class MsuCreateViewModel : ViewModel
{
    private readonly MsuRandomizationStyle _randomizationStyle;
    private readonly MsuAppSettings _msuAppSettings = new();
    
    public MsuCreateViewModel()
    {
        
    }

    public MsuCreateViewModel(MsuUserOptions options, MsuAppSettings appSettings)
    {
        _msuAppSettings = appSettings;
        _randomizationStyle = options.RandomizationStyle;
        _msuName = options.Name;
        _openMonitorWindow = options.OpenMonitorWindow;
        _openFolderOnCreate = options.OpenFolderOnCreate;
        _avoidDuplicates = options.AvoidDuplicates;
        _msuShuffleStyle = options.MsuShuffleStyle;
    }

    public void UpdateSettings(MsuUserOptions options)
    {
        options.Name = _msuName;
        options.OpenMonitorWindow = _openMonitorWindow;
        options.OpenFolderOnCreate = _openFolderOnCreate;
        options.AvoidDuplicates = _avoidDuplicates;
        options.MsuShuffleStyle = _msuShuffleStyle;
    }

    public Visibility OpenFolderOnCreateCheckBoxVisibility => _randomizationStyle != MsuRandomizationStyle.Continuous
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility AvoidDuplicatesCheckBoxVisibility => _randomizationStyle != MsuRandomizationStyle.Single
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility OpenMonitorWindowCheckBoxVisibility => _randomizationStyle != MsuRandomizationStyle.Continuous && _msuAppSettings.DisableMsuMonitorWindow != true
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility MsuShuffleStyleVisibility => _randomizationStyle != MsuRandomizationStyle.Single
        ? Visibility.Visible
        : Visibility.Collapsed;

    private string _msuName = "";

    public string MsuName
    {
        get => _msuName;
        set => SetField(ref _msuName, value);
    }

    private bool _openFolderOnCreate;

    public bool OpenFolderOnCreate
    {
        get => _openFolderOnCreate;
        set => SetField(ref _openFolderOnCreate, value);
    }
    
    private bool _avoidDuplicates;

    public bool AvoidDuplicates
    {
        get => _avoidDuplicates;
        set => SetField(ref _avoidDuplicates, value);
    }
    
    private bool _openMonitorWindow;

    public bool OpenMonitorWindow
    {
        get => _openMonitorWindow;
        set => SetField(ref _openMonitorWindow, value);
    }

    private MsuShuffleStyle _msuShuffleStyle;

    public MsuShuffleStyle MsuShuffleStyle
    {
        get => _msuShuffleStyle;
        set => SetField(ref _msuShuffleStyle, value);
    }
}