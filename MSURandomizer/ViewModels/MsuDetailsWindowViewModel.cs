using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuSettings))]
public class MsuDetailsWindowViewModel : ViewModelBase
{
    [Reactive] public string MsuPath { get; set; } = "";
    
    [Reactive] public string AbbreviatedPath { get; set; } = "";

    [Reactive] public string? MsuTypeName { get; set; }

    [Reactive] public AltOptions AltOption { get; set; }

    [Reactive] public string? Name { get; set; }
    
    [Reactive] public string? Creator { get; set; }

    public string DefaultMsuName => Msu?.Name ?? "";
    
    public string DefaultCreator => Msu?.Creator ?? "";

    public Msu? Msu { get; set; }
    
    public List<MsuTrackViewModel> Tracks { get; set; } = [];
    
    public List<string> MsuTypeNames { get; set; } = [];
    
    public int TrackCount { get; set; }

    public string TrackCountString => $"{TrackCount} Tracks";

    public bool CanEditDetails => Msu?.HasDetails != true;

    public bool CanEditMsuType => true;

    public bool IsNotLast { get; set; } = true;
    
    [Reactive, ReactiveLinkedProperties(nameof(UndoOpacity))]
    public bool IsAnyCopyrightSafeValueOverridden { get; set; }
    
    [Reactive, ReactiveLinkedProperties(nameof(CheckedIconKind), nameof(CopyrightIconBrush))]
    public bool? AreAllCopyrightSafe { get; set; }
    
    public MaterialIconKind CheckedIconKind => AreAllCopyrightSafe switch
    {
        true => MaterialIconKind.CheckboxOutline,
        false => MaterialIconKind.CancelBoxOutline,
        _ => MaterialIconKind.QuestionBoxOutline
    };
    
    public IBrush CopyrightIconBrush => AreAllCopyrightSafe switch
    {
        true => Brushes.LimeGreen,
        false => Brushes.IndianRed,
        _ => Brushes.Goldenrod
    };

    public float UndoOpacity => IsAnyCopyrightSafeValueOverridden ? 1.0f : 0.3f;

    public void UpdateCopyrightOptions()
    {
        var allSongs = Tracks.SelectMany(x => x.Songs).ToList();
        IsAnyCopyrightSafeValueOverridden = allSongs.Any(x => x.IsCopyrightSafeValueOverridden);
        AreAllCopyrightSafe = allSongs.All(x => x.IsCopyrightSafeCombined == true);
    }
}