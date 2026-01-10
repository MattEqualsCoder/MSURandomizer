using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class MsuSongViewModel(Track track) : ViewModelBase
{
    public string SongDetails { get; set; } = $"• {track.GetDisplayText(TrackDisplayFormat.Horizontal)}";
    public string Path { get; set; } = track.Path;
    public string? Url { get; set; } = track.Url;
    public bool DisplayUrl => !string.IsNullOrWhiteSpace(Url);
    public bool? IsCopyrightSafeCombined => IsCopyrightSafeOverrideValue ?? IsCopyrightSafeOriginalValue;
    public bool IsCopyrightSafeValueOverridden => IsCopyrightSafeOverrideValue != null && IsCopyrightSafeOverrideValue != IsCopyrightSafeOriginalValue;
    public bool? IsCopyrightSafeOriginalValue { get; set; } = track.IsCopyrightSafe;
    
    [Reactive, ReactiveLinkedProperties(nameof(CheckedIconKind), nameof(UndoOpacity), nameof(IsCopyrightSafeValueOverridden), nameof(CopyrightIconBrush))]
    public bool? IsCopyrightSafeOverrideValue { get; set; } = track.IsCopyrightSafeOverride;

    public MaterialIconKind CheckedIconKind => IsCopyrightSafeCombined switch
    {
        true => MaterialIconKind.CheckboxOutline,
        false => MaterialIconKind.CancelBoxOutline,
        _ => MaterialIconKind.QuestionBoxOutline
    };
    
    public IBrush CopyrightIconBrush => IsCopyrightSafeCombined switch
    {
        true => Brushes.LimeGreen,
        false => Brushes.IndianRed,
        _ => Brushes.Goldenrod
    };

    public float UndoOpacity => IsCopyrightSafeValueOverridden ? 1.0f : 0.3f;
}