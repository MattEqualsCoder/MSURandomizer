using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaControls;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;
using SnesConnectorLibrary;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public class SettingsWindowViewModel : ViewModelBase
{
    [Reactive] public bool PromptOnUpdate { get; set; }

    [Reactive] public string DefaultMsuPath { get; set; } = "";

    [Reactive] public string MsuCurrentSongOutputFilePath { get; set; } = "";

    [Reactive] public string DefaultDirectory { get; set; } = "";

    [Reactive] public double UiScaling { get; set; } = 1;

    [Reactive] public SnesConnectorSettingsViewModel SnesConnectorSettings { get; set; } = new();

    [Reactive] public Dictionary<string, string> MsuTypeNamePaths { get; set; } = new();

    [Reactive] public List<MsuTypePath> MsuTypeNamePathsList { get; set; } = new();
    
    [Reactive] public string? CopyRomDirectory { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(LaunchArgumentsEnabled))]
    public string? LaunchApplication { get; set; }

    public bool LaunchArgumentsEnabled => !string.IsNullOrEmpty(LaunchApplication);
    
    [Reactive] public bool DisplayNoMsuDirectoriesMessage { get; set; }

    public ObservableCollection<MsuDirectory> MsuDirectoryList { get; set; } = [];
    
    [Reactive] public string? LaunchArguments { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(TrackDisplayExample))]
    public TrackDisplayFormat TrackDisplayFormat { get; set; }
    
    public string TrackDisplayExample =>
        "Ex" + TrackDisplayFormat.GetDescription()[
            TrackDisplayFormat.GetDescription().IndexOf(':', StringComparison.Ordinal)..];
    
    public Func<string, string> TrackDisplayComboBoxText =>
        s => s[..s.IndexOf(':', StringComparison.Ordinal)];
}

public class MsuTypePath : ViewModelBase
{
    [Reactive] public MsuType? MsuType { get; set; }
    
    [Reactive] public string Path { get; set; } = "";

    [Reactive] public string DefaultDirectory { get; set; } = "";
    
    public string MsuTypeName => MsuType?.DisplayName ?? "A Link to the Past";
}

public class MsuDirectory : ViewModelBase
{
    public MsuDirectory(string path, string msuTypeName = "", List<string>? msuTypes = null)
    {
        Path = path;
        MsuTypeName = msuTypeName;
        MsuTypes = msuTypes ?? [];
    }
    
    [Reactive] public string Path { get; set; }
    [Reactive] public string MsuTypeName { get; set; }
    [Reactive] public List<string> MsuTypes { get; set; }
    public string AbbreviatedPath => Path.Length > 40 ? string.Concat("...", Path.AsSpan(Path.Length - 38)) : Path;
}

[MapsTo(typeof(SnesConnectorSettings))]
public class SnesConnectorSettingsViewModel : ViewModelBase
{
    [Reactive] public string Usb2SnesAddress { get; set; } = "";
    
    [Reactive] public string SniAddress { get; set; } = "";
    
    [Reactive] public string LuaAddress { get; set; } = "";
}