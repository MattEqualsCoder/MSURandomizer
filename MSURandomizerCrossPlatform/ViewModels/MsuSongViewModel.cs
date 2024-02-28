using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizerCrossPlatform.ViewModels;

public class MsuSongViewModel(Track track) : ViewModelBase
{
    public string SongDetails { get; set; } = $"• {track.GetDisplayText(TrackDisplayFormat.Horizontal)}";
    public string Path { get; set; } = track.Path;
    public string? Url { get; set; } = track.Url;
    public bool DisplayUrl => !string.IsNullOrWhiteSpace(Url);
}