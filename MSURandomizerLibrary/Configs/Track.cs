using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

public class Track
{
    public Track(string trackName, int number, string songName, string path, string? artist = null,
        string? album = null, string? url = null, bool isAlt = false)
    {
        TrackName = trackName;
        Number = number;
        SongName = songName;
        Path = path;
        Artist = artist;
        Album = album;
        Url = url;
        IsAlt = isAlt;
        OriginalPath = path;
    }
    
    public Track(Track other, int? number = null, string? path = null, string? trackName = null)
    {
        TrackName = trackName ?? other.TrackName;
        Number = number ?? other.Number;
        SongName = other.SongName;
        Path = path ?? other.Path;
        Artist = other.Artist;
        Album = other.Album;
        IsAlt = other.IsAlt;
        Url = other.Url;
        OriginalPath = other.OriginalPath;
        OriginalMsu = other.OriginalMsu ?? other.Msu;
    }
    
    public string TrackName { get; set; }
    public int Number { get; set; }
    public string SongName { get; set; }
    public string? MsuPath { get; set; }
    public string? MsuName { get; set; }
    public string? MsuCreator { get; set; }
    public string Path { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; } 
    public string? Url { get; set; }
    public string? OriginalPath { get; set; }
    [YamlIgnore]
    public bool IsAlt { get; set; }
    [YamlIgnore]
    public bool IsCopied { get; set; }
    [YamlIgnore]
    public Msu? Msu { get; set; }
    [YamlIgnore]
    public Msu? OriginalMsu { get; set; }
    [YamlIgnore]
    public string? DisplayArtist => Artist ?? Msu?.Artist;
    [YamlIgnore]
    public string? DisplayAlbum => Album ?? Msu?.Album;
    [YamlIgnore]
    public string? DisplayUrl => Url ?? Msu?.Url;
    
    public string GetDisplayText(bool includeMsu = true)
    {
        var artist = string.IsNullOrWhiteSpace(DisplayArtist) ? "" : $" by {DisplayArtist}";
        var album = string.IsNullOrWhiteSpace(DisplayAlbum) ? "" : $" from album {DisplayAlbum}";
        if (!includeMsu)
            return $"{SongName}{artist}{album}";
        var creator = string.IsNullOrWhiteSpace(MsuCreator) ? "" : $" by {MsuCreator}";
        return $"{SongName}{artist}{album} from MSU Pack {MsuName}{creator}";
    }
}