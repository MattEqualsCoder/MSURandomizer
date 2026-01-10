using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuViewModel))]
public partial class MsuViewModel : ViewModelBase
{

    public MsuViewModel()
    {
        Msu = new Msu();
        MsuPath = string.Empty;
        DisplayPath = string.Empty;
        MsuTrackCount = string.Empty;
        MsuTypeName = string.Empty;
        Tracks = [];
        ShowShuffleFrequency = true;
    }
    
    public MsuViewModel(Msu msu)
    {
        Msu = msu;
        MsuName = msu.DisplayName;
        MsuCreator = msu.DisplayCreator;
        MsuPath = msu.Path;
        DisplayPath = msu.RelativePath;
        MsuTypeName = msu.MsuType?.DisplayName ?? msu.MsuTypeName;
        MsuTrackCount = $"{msu.ValidTracks.Count} Tracks";
        IsFavorite = msu.Settings.IsFavorite;
        ShuffleFrequency = msu.Settings.ShuffleFrequency;
        IsHardwareMsu = msu.IsHardwareMsu;
        ShowShuffleFrequency = !IsHardwareMsu;
        Tracks = [];
    }
    
    [Reactive] public partial Msu Msu { get; set; }

    [Reactive] 
    [ReactiveLinkedProperties(nameof(ListText))]
    public partial string? MsuName { get; set; }
    
    [Reactive] 
    [ReactiveLinkedProperties(nameof(ListText))]
    public partial string? MsuCreator { get; set; }
    
    [Reactive] public partial AltOptions AltOptions { get; set; }

    [Reactive] public partial string MsuPath { get; set; }

    [Reactive] public partial string DisplayPath { get; set; }

    [Reactive] public partial string MsuTypeName { get; set; }

    [Reactive] public partial string MsuTrackCount { get; set; }
    
    [Reactive] public partial bool IsHardwareMsu { get; set; }

    [Reactive] public partial List<MsuTrackViewModel>? Tracks { get; set; }
    
    [Reactive] public partial bool IsMonitorWindowAvailable { get; set; }

    [Reactive] 
    [ReactiveLinkedProperties(nameof(FavoriteIconColor))]
    public partial bool IsFavorite { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(ShuffleFrequencyIcon), nameof(ShuffleFrequencyColor))]
    public partial ShuffleFrequency ShuffleFrequency { get; set; }

    [Reactive] public partial bool ShowShuffleFrequency { get; set; }
    
    public string ListText => string.IsNullOrEmpty(MsuCreator) ? MsuName ?? "" : $"{MsuName} by {MsuCreator}";
    
    public IImmutableSolidColorBrush FavoriteIconColor => IsFavorite ? Brushes.Goldenrod : Brushes.Gray;

    public MaterialIconKind ShuffleFrequencyIcon => ShuffleFrequency switch
    {
        ShuffleFrequency.Default => MaterialIconKind.CircleOutline,
        ShuffleFrequency.LessFrequent => MaterialIconKind.ChevronDownCircleOutline,
        _ => MaterialIconKind.ChevronUpCircleOutline
    };
    
    public IImmutableSolidColorBrush ShuffleFrequencyColor => ShuffleFrequency switch
    {
        ShuffleFrequency.Default => Brushes.Gray,
        ShuffleFrequency.LessFrequent => Brushes.Red,
        _ => Brushes.Green
    };

}