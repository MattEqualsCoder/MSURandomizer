using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.UI;

public class MsuDetailsSongViewModel
{
    public MsuDetailsSongViewModel(Track track)
    {
        SongDetails = track.GetDisplayText(false);
        Url = track.Url;
    }
    
    public string SongDetails { get; set; }
    public string? Url { get; set; }
    public bool HasUrl => !string.IsNullOrEmpty(Url);
}