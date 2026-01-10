using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaControls.Extensions;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;
using SnesConnectorLibrary;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public partial class SettingsWindowViewModel : ViewModelBase
{
    [Reactive] public partial bool PromptOnUpdate { get; set; }

    [Reactive] public partial string DefaultMsuPath { get; set; }

    [Reactive] public partial string MsuCurrentSongOutputFilePath { get; set; }

    [Reactive] public partial string DefaultDirectory { get; set; }

    [Reactive] public partial double UiScaling { get; set; }

    [Reactive] public partial SnesConnectorSettingsViewModel SnesConnectorSettings { get; set; }

    [Reactive] public partial Dictionary<string, string> MsuTypeNamePaths { get; set; }

    [Reactive] public partial List<MsuTypePath> MsuTypeNamePathsList { get; set; }
    
    [Reactive] public partial string? CopyRomDirectory { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(LaunchArgumentsEnabled))]
    public partial string? LaunchApplication { get; set; }

    public bool LaunchArgumentsEnabled => !string.IsNullOrEmpty(LaunchApplication);
    
    [Reactive] public partial bool DisplayNoMsuDirectoriesMessage { get; set; }

    public ObservableCollection<MsuDirectory> MsuDirectoryList { get; set; } = [];
    
    [Reactive] public partial string? LaunchArguments { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(TrackDisplayExample))]
    public partial TrackDisplayFormat TrackDisplayFormat { get; set; }

    public bool DisplayDesktopFileButton => OperatingSystem.IsLinux();
    
    public string TrackDisplayExample =>
        "Ex" + TrackDisplayFormat.GetDescription()[
            TrackDisplayFormat.GetDescription().IndexOf(':', StringComparison.Ordinal)..];
    
    public Func<string, string> TrackDisplayComboBoxText =>
        s => s[..s.IndexOf(':', StringComparison.Ordinal)];

    public SettingsWindowViewModel()
    {
        DefaultMsuPath = "";
        MsuCurrentSongOutputFilePath = "";
        DefaultDirectory = "";
        UiScaling = 1;
        SnesConnectorSettings = new SnesConnectorSettingsViewModel();
        MsuTypeNamePaths = new Dictionary<string, string>();
        MsuTypeNamePathsList = [];
    }
}

public partial class MsuTypePath : ViewModelBase
{
    [Reactive] public partial MsuType? MsuType { get; set; }
    
    [Reactive] public partial string Path { get; set; }

    [Reactive] public partial string DefaultDirectory { get; set; }
    
    public string MsuTypeName => MsuType?.DisplayName ?? "A Link to the Past";

    public MsuTypePath()
    {
        Path = string.Empty;
        DefaultDirectory = string.Empty;
    }
}

public partial class MsuDirectory : ViewModelBase
{
    public MsuDirectory(string path, string msuTypeName = "", List<string>? msuTypes = null)
    {
        Path = path;
        MsuTypeName = msuTypeName;
        MsuTypes = msuTypes ?? [];
    }
    
    [Reactive] public partial string Path { get; set; }
    [Reactive] public partial string MsuTypeName { get; set; }
    [Reactive] public partial List<string> MsuTypes { get; set; }
    public string AbbreviatedPath => Path.Length > 40 ? string.Concat("...", Path.AsSpan(Path.Length - 38)) : Path;
}

[MapsTo(typeof(SnesConnectorSettings))]
public partial class SnesConnectorSettingsViewModel : ViewModelBase
{
    [Reactive] public partial string Usb2SnesAddress { get; set; }
    
    [Reactive] public partial string SniAddress { get; set; }
    
    [Reactive] public partial string LuaAddress { get; set; }

    public SnesConnectorSettingsViewModel()
    {
        Usb2SnesAddress = string.Empty;
        SniAddress = string.Empty;
        LuaAddress = string.Empty;
    }
}