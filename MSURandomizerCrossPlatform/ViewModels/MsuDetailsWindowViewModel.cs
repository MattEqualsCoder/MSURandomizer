using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizerCrossPlatform.ViewModels;

[MapsTo(typeof(MsuSettings))]
public class MsuDetailsWindowViewModel : ViewModelBase
{
    [Reactive] public string MsuPath { get; set; } = "";

    [Reactive] public string? MsuTypeName { get; set; }

    [Reactive] public AltOptions AltOption { get; set; }

    [Reactive] public string? Name { get; set; }
    
    [Reactive] public string? Creator { get; set; }

    public string DefaultMsuName => Msu?.Name ?? "";
    
    public string DefaultCreator => Msu?.Creator ?? "";

    public Msu? Msu { get; set; }
    
    public List<MsuTrackViewModel> Tracks { get; set; } = new();
    
    public List<string> MsuTypeNames { get; set; } = new();

    public bool CanEditDetails => Msu?.HasDetails != true;
}