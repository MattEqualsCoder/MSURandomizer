using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public partial class MsuGenerationViewModel : ViewModelBase
{
    [Reactive] public partial string Name { get; set; }

    [Reactive] public partial string? OutputRomPath { get; set; }

    [Reactive] public partial string? OutputFolderPath { get; set; }

    [Reactive] public partial bool AvoidDuplicates { get; set; }
    
    [Reactive] public partial MsuCopyrightSafety MsuCopyrightSafety { get; set; }

    [Reactive] public partial bool OpenFolderOnCreate { get; set; }

    [Reactive] public partial MsuShuffleStyle MsuShuffleStyle { get; set; }

    [Reactive] public partial bool OpenMonitorWindow { get; set; }
    
    [Reactive] public partial string OutputMsuType { get; set; }
    
    [Reactive] public partial ICollection<string> SelectedMsus { get; set; }
    
    [Reactive] public partial bool PassedRomArgument { get; set; }
    
    [Reactive] public partial bool LaunchRom { get; set; }
    
    [Reactive] public partial bool IsLaunchRomVisible { get; set; }
    
    [Reactive] 
    [ReactiveLinkedProperties(nameof(IsOpenFolderVisible), nameof(IsAvoidDuplicatesVisible), nameof(IsOpenMonitorVisible), nameof(IsMsuShuffleStyleVisible), nameof(IsOnlyCopyrightSafeTracks))]
    public partial MsuRandomizationStyle RandomizationStyle { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsOpenMonitorVisible))]
    public partial bool IsMsuMonitorDisabled { get; set; }

    public bool IsOpenFolderVisible => RandomizationStyle != MsuRandomizationStyle.Continuous;

    public bool IsAvoidDuplicatesVisible => RandomizationStyle != MsuRandomizationStyle.Single;
    
    public bool IsOnlyCopyrightSafeTracks => RandomizationStyle != MsuRandomizationStyle.Single;

    public bool IsOpenMonitorVisible =>
        RandomizationStyle != MsuRandomizationStyle.Continuous && !IsMsuMonitorDisabled;

    public bool IsMsuShuffleStyleVisible => RandomizationStyle != MsuRandomizationStyle.Single;

    public MsuGenerationViewModel()
    {
        Name = string.Empty;
        SelectedMsus = [];
        OutputMsuType = string.Empty;
    }
}