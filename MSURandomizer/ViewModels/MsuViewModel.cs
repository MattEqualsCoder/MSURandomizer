using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuViewModel))]
public class MsuViewModel : ViewModelBase
{

    public MsuViewModel()
    {
        Msu = new();
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
    }
    
    [Reactive] public Msu Msu { get; init; }

    [Reactive] 
    [ReactiveLinkedProperties(nameof(ListText))]
    public string? MsuName { get; set; }
    
    [Reactive] 
    [ReactiveLinkedProperties(nameof(ListText))]
    public string? MsuCreator { get; set; }
    
    
    [Reactive] public AltOptions AltOptions { get; set; }

    [Reactive] public string MsuPath { get; init; } = "";

    [Reactive] public string DisplayPath { get; set; } = "";

    [Reactive] public string MsuTypeName { get; init; } = "";

    [Reactive] public string MsuTrackCount { get; init; } = "";
    
    [Reactive] public bool IsHardwareMsu { get; init; }

    [Reactive] public List<MsuTrackViewModel>? Tracks { get; init; } = new();
    
    [Reactive] public bool IsMonitorWindowAvailable { get; set; }

    [Reactive] 
    [ReactiveLinkedProperties(nameof(FavoriteIconColor))]
    public bool IsFavorite { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(ShuffleFrequencyIcon), nameof(ShuffleFrequencyColor))]
    public ShuffleFrequency ShuffleFrequency { get; set; }

    [Reactive] public bool ShowShuffleFrequency { get; set; } = true;
    
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