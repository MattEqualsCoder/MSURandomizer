namespace MSURandomizerLibrary.Configs;

public class Track
{
    public Track(string trackName, int number, string songName, string path, string msuPath, string msuName, string? msuCreator = null, string? artist = null,
        string? album = null, string? url = null, bool isAlt = false)
    {
        TrackName = trackName;
        Number = number;
        SongName = songName;
        Path = path;
        MsuPath = msuPath;
        MsuName = msuName;
        MsuCreator = msuCreator;
        Artist = artist;
        Album = album;
        Url = url;
        IsAlt = isAlt;
        OriginalPath = path;
    }
    
    public Track(Track other, int? number = null, string? path = null, string? trackName = null)
    {
        TrackName = other.TrackName;
        Number = number ?? other.Number;
        SongName = other.SongName;
        Path = path ?? other.Path;
        Artist = other.Artist;
        Album = other.Album;
        IsAlt = other.IsAlt;
        MsuPath = other.MsuPath;
        MsuName = other.MsuName;
        MsuCreator = other.MsuCreator;
        Url = other.Url;
        OriginalPath = other.OriginalPath;
    }
    
    public string TrackName { get; set; }
    public int Number { get; set; }
    public string SongName { get; set; }
    public string MsuPath { get; set; }
    public string MsuName { get; set; }
    public string? MsuCreator { get; set; }
    public string Path { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; } 
    public string? Url { get; set; }
    public string? OriginalPath { get; set; }
    public bool IsAlt { get; set; }
    
    public string GetDisplayText()
    {
        var artist = string.IsNullOrWhiteSpace(Artist) ? "" : $" by {Artist}";
        var album = string.IsNullOrWhiteSpace(Album) ? "" : $" from album {Album}";
        var creator = string.IsNullOrWhiteSpace(MsuCreator) ? "" : $" by {MsuCreator}";
        return $"{SongName}{artist}{album} from MSU Pack {MsuName}{creator}";
    }
}