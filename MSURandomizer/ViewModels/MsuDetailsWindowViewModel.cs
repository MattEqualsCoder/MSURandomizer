using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuSettings))]
public partial class MsuDetailsWindowViewModel : ViewModelBase
{
    [Reactive] public partial string MsuPath { get; set; }
    
    [Reactive] public partial string AbbreviatedPath { get; set; }

    [Reactive] public partial string? MsuTypeName { get; set; }

    [Reactive] public partial AltOptions AltOption { get; set; }

    [Reactive] public partial string? Name { get; set; }
    
    [Reactive] public partial string? Creator { get; set; }

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
    public partial bool IsAnyCopyrightSafeValueOverridden { get; set; }
    
    [Reactive, ReactiveLinkedProperties(nameof(CheckedIconKind), nameof(CopyrightIconBrush))]
    public partial bool? AreAllCopyrightSafe { get; set; }
    
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

    public MsuDetailsWindowViewModel()
    {
        MsuPath = string.Empty;
        AbbreviatedPath = string.Empty;
    }

    public void UpdateCopyrightOptions()
    {
        var allSongs = Tracks.SelectMany(x => x.Songs).ToList();
        IsAnyCopyrightSafeValueOverridden = allSongs.Any(x => x.IsCopyrightSafeValueOverridden);
        AreAllCopyrightSafe = allSongs.All(x => x.IsCopyrightSafeCombined == true);
    }
}