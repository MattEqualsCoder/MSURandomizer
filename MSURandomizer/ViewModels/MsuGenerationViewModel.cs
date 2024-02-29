using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public class MsuGenerationViewModel : ViewModelBase
{
    [Reactive] public string Name { get; set; } = "";

    [Reactive] public string? OutputRomPath { get; set; }

    [Reactive] public string? OutputFolderPath { get; set; }

    [Reactive] public bool AvoidDuplicates { get; set; }

    [Reactive] public bool OpenFolderOnCreate { get; set; }

    [Reactive] public MsuShuffleStyle MsuShuffleStyle { get; set; }

    [Reactive] public bool OpenMonitorWindow { get; set; }
    
    [Reactive] public string OutputMsuType { get; set; } = "";
    
    [Reactive] public ICollection<string> SelectedMsus { get; set; } = new List<string>();
    
    [Reactive] 
    [ReactiveLinkedProperties(nameof(IsOpenFolderVisible), nameof(IsAvoidDuplicatesVisible), nameof(IsOpenMonitorVisible), nameof(IsMsuShuffleStyleVisible))]
    public MsuRandomizationStyle RandomizationStyle { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsOpenMonitorVisible))]
    public bool IsMsuMonitorDisabled { get; set; }

    public bool IsOpenFolderVisible => RandomizationStyle != MsuRandomizationStyle.Continuous;

    public bool IsAvoidDuplicatesVisible => RandomizationStyle != MsuRandomizationStyle.Single;

    public bool IsOpenMonitorVisible =>
        RandomizationStyle != MsuRandomizationStyle.Continuous && !IsMsuMonitorDisabled;

    public bool IsMsuShuffleStyleVisible => RandomizationStyle != MsuRandomizationStyle.Single;
}