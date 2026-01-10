using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

public partial class MsuSongViewModel : ViewModelBase
{
    public string SongDetails { get; set; }
    public string Path { get; set; }
    public string? Url { get; set; }
    public bool DisplayUrl => !string.IsNullOrWhiteSpace(Url);
    public bool? IsCopyrightSafeCombined => IsCopyrightSafeOverrideValue ?? IsCopyrightSafeOriginalValue;
    public bool IsCopyrightSafeValueOverridden => IsCopyrightSafeOverrideValue != null && IsCopyrightSafeOverrideValue != IsCopyrightSafeOriginalValue;
    public bool? IsCopyrightSafeOriginalValue { get; set; }
    
    [Reactive, ReactiveLinkedProperties(nameof(CheckedIconKind), nameof(UndoOpacity), nameof(IsCopyrightSafeValueOverridden), nameof(CopyrightIconBrush))]
    public partial bool? IsCopyrightSafeOverrideValue { get; set; }

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

    public MsuSongViewModel(Track track)
    {
        SongDetails = $"• {track.GetDisplayText(TrackDisplayFormat.Horizontal)}";
        Path = track.Path;
        Url = track.Url;
        IsCopyrightSafeOriginalValue = track.IsCopyrightSafe;
        IsCopyrightSafeOverrideValue = track.IsCopyrightSafeOverride;
    }
}