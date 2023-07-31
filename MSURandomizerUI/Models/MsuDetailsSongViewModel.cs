using MSURandomizerLibrary.Configs;

namespace MSURandomizerUI.Models;

public class MsuDetailsSongViewModel
{
    public MsuDetailsSongViewModel(Track track)
    {
        SongDetails = track.GetDisplayText(false);
        Url = track.DisplayUrl;
        Path = track.Path;
    }
    
    public string SongDetails { get; set; }
    public string? Url { get; set; }
    public string Path { get; set; }
    public bool HasUrl => !string.IsNullOrEmpty(Url);
}