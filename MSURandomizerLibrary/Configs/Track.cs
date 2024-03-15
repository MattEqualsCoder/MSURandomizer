using System.Text.Json.Serialization;

namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Represents a single song under an MSU
/// </summary>
public class Track
{
    /// <summary>
    /// Constructor
    /// </summary>
    public Track()
    {
        
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="trackName">The name of the track in the MSU type</param>
    /// <param name="number">The number of the track in the MSU type</param>
    /// <param name="songName">The name of the song</param>
    /// <param name="path">The path to the pcm file</param>
    /// <param name="artist">The name of the artist(s)</param>
    /// <param name="album">The name of the album</param>
    /// <param name="url">Url to find the song</param>
    /// <param name="isAlt">If this is an alt track or not</param>
    /// <param name="isBaseFile">If this is the base file and not an alt</param>
    public Track(string trackName, int number, string songName, string path, string? artist = null,
        string? album = null, string? url = null, bool isAlt = false, bool isBaseFile = false)
    {
        TrackName = trackName;
        Number = number;
        OriginalTrackNumber = number;
        OriginalTrackName = trackName;
        SongName = songName;
        Path = path;
        Artist = artist;
        Album = album;
        Url = url;
        IsAlt = isAlt;
        OriginalPath = path;
        IsBaseFile = isBaseFile;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="other">Track to copy details from</param>
    /// <param name="number">Override for the track number in case of copying fallbacks</param>
    /// <param name="path">The path to the pcm file</param>
    /// <param name="trackName">Override for the track number in case of copying fallbacks</param>
    public Track(Track other, int? number = null, string? path = null, string? trackName = null)
    {
        TrackName = trackName ?? other.TrackName;
        Number = number ?? other.Number;
        OriginalTrackNumber = other.OriginalTrackNumber;
        OriginalTrackName = other.TrackName;
        SongName = other.SongName;
        Path = path ?? other.Path;
        Artist = other.Artist;
        Album = other.Album;
        IsAlt = other.IsAlt;
        IsBaseFile = other.IsBaseFile;
        Url = other.Url;
        MsuName = other.MsuName;
        MsuCreator = other.MsuCreator;
        OriginalPath = other.OriginalPath;
        OriginalMsu = other.OriginalMsu ?? other.Msu;
    }

    /// <summary>
    /// The name of the track in the MSU type
    /// </summary>
    public string TrackName { get; set; } = "";
    
    /// <summary>
    /// The number of the track in the MSU type
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// The path to the original MSU the track is from
    /// </summary>
    public string? MsuPath { get; set; }
    
    /// <summary>
    /// The name of the original MSU the track is from
    /// </summary>
    public string? MsuName { get; set; }
    
    /// <summary>
    /// The creator of the original MSU the track is from
    /// </summary>
    public string? MsuCreator { get; set; }
    
    /// <summary>
    /// The path to the pcm file
    /// </summary>
    public string Path { get; set; } = "";
    
    /// <summary>
    /// The name of the song
    /// </summary>
    public string SongName { get; set; } = "";
    
    /// <summary>
    /// The name of the artist(s) that created the song
    /// </summary>
    public string? Artist { get; set; }
    
    /// <summary>
    /// The name of the album the song is from
    /// </summary>
    public string? Album { get; set; } 
    
    /// <summary>
    /// Url to view/purchase/download the song from
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Original path to the pcm file
    /// </summary>
    public string? OriginalPath { get; set; }
    
    /// <summary>
    /// If this is an alt track or not
    /// </summary>
    public bool IsAlt { get; set; }
    
    /// <summary>
    /// If this is copied from a fallback track
    /// </summary>
    public bool IsCopied { get; set; }
    
    /// <summary>
    /// If this is the current base pcm file, regardless of if it is an alt track or not
    /// </summary>
    public bool IsBaseFile { get; set; }
    
    /// <summary>
    /// The number the track was shuffled as
    /// </summary>
    public int OriginalTrackNumber { get; set; }

    /// <summary>
    /// The name the track was shuffled as
    /// </summary>
    public string OriginalTrackName { get; set; } = "";
    
    /// <summary>
    /// The MSU this track is currently part of
    /// </summary>
    [JsonIgnore]
    public Msu? Msu { get; set; }
    
    /// <summary>
    /// The original MSU this track is from
    /// </summary>
    [JsonIgnore]
    public Msu? OriginalMsu { get; set; }
    
    /// <summary>
    /// The name of the artist(s) that created this song. If no artist is specified for the track, it pulls from the MSU
    /// </summary>
    [JsonIgnore]
    public string? DisplayArtist => Artist ?? Msu?.Artist;
    
    /// <summary>
    /// The name of the album the song is from. If no album is specified for the track, it pulls from the MSU
    /// </summary>
    [JsonIgnore]
    public string? DisplayAlbum => Album ?? Msu?.Album;
    
    /// <summary>
    /// The url to access the song. If no url is specified for the track, it pulls from the MSU
    /// </summary>
    [JsonIgnore]
    public string? DisplayUrl => Url ?? Msu?.Url;
    
    /// <summary>
    /// Gets text to represent the song
    /// </summary>
    /// <param name="includeMsu">Include the details about the MSU that the track is from</param>
    /// <returns>Text to display for the song</returns>
    public string GetDisplayText(bool includeMsu = true)
    {
        var artist = string.IsNullOrWhiteSpace(DisplayArtist) ? "" : $" by {DisplayArtist}";
        var album = string.IsNullOrWhiteSpace(DisplayAlbum) ? "" : $" from album {DisplayAlbum}";
        if (!includeMsu || string.IsNullOrEmpty(MsuName))
            return $"{SongName}{artist}{album}";
        var creator = string.IsNullOrWhiteSpace(MsuCreator) ? "" : $" by {MsuCreator}";
        return $"{SongName}{artist}{album} from MSU Pack {MsuName}{creator}";
    }

    /// <summary>
    /// Gets the text to represent the song
    /// </summary>
    /// <param name="format">The format to use for song text</param>
    /// <returns>The text to represent the song</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public string GetDisplayText(TrackDisplayFormat format)
    {
        var builder = new TrackDisplayTextBuilder(this);
        
        switch (format)
        {
            case TrackDisplayFormat.Horizontal:
            {
                if (string.IsNullOrWhiteSpace(DisplayArtist) && string.IsNullOrWhiteSpace(DisplayAlbum))
                {
                    return builder.AddSongName("{0}")
                        .AddMsuNameAndCreator("from {0}")
                        .ToString();
                }

                return builder.AddAlbum("{0} -")
                    .AddSongName("{0}")
                    .AddArtist("({0})")
                    .ToString();
            }
            case TrackDisplayFormat.Vertical:
            {
                return builder.AddMsuNameAndCreator("MSU: {0}")
                    .AddAlbum("Album: {0}")
                    .AddSongName("Song: {0}")
                    .AddArtist("Artist: {0}")
                    .ToString("\r\n");
            }
            case TrackDisplayFormat.VerticalWithOriginalTrackName:
            {
                return builder.AddMsuNameAndCreator("MSU: {0}")
                    .AddAlbum("Album: {0}")
                    .AddSongName("Song: {0}")
                    .AddArtist("Artist: {0}")
                    .AddOriginalTrackName("Originally plays for: {0}")
                    .ToString("\r\n");
            }
            case TrackDisplayFormat.HorizonalWithMsu:
            {
                return builder.AddAlbum("{0}:")
                    .AddSongName("{0}")
                    .AddArtist("- {0}")
                    .AddMsuNameAndCreator("(MSU: {0})")
                    .ToString();
            }
            case TrackDisplayFormat.SentenceStyle:
            {
                return builder.AddSongName("{0}")
                    .AddArtist("by {0}")
                    .AddAlbum("from album {0}")
                    .AddMsuNameAndCreator("from MSU Pack {0}")
                    .ToString();
            }
            case TrackDisplayFormat.SpeechStyle:
            {
                return builder.AddSongName("{0}")
                    .AddArtist("by {0}")
                    .AddAlbum("from album {0}")
                    .AddMsuName("from MSU Pack {0}")
                    .AddMsuCreator("by {0}")
                    .ToString("; ");
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    /// <summary>
    /// Returns the MSU name and creator for the track
    /// </summary>
    /// <returns>A string of the MSU name and creator</returns>
    public string? GetMsuName()
    {
        if (!string.IsNullOrEmpty(MsuName))
        {
            var creator = string.IsNullOrWhiteSpace(MsuCreator) ? "" : $" by {MsuCreator}";
            return $"{MsuName}{creator}";
        }
        else if(OriginalMsu != null)
        {
            var creator = string.IsNullOrWhiteSpace(OriginalMsu.DisplayCreator) ? "" : $" by {OriginalMsu.DisplayCreator}";
            return $"{OriginalMsu.DisplayName}{creator}";
        }
        else if (Msu != null)
        {
            var creator = string.IsNullOrWhiteSpace(Msu.DisplayCreator) ? "" : $" by {Msu.DisplayCreator}";
            return $"{Msu.DisplayName}{creator}";
        }
        else
        {
            return null;
        }
    }
    
    /// <summary>
    /// If this track matches a user's alt track options
    /// </summary>
    /// <param name="option">The user selected option</param>
    /// <returns>True if matches, false otherwise</returns>
    public bool MatchesAltOption(AltOptions option)
    {
        if (option == AltOptions.LeaveAlone)
            return IsBaseFile;
        else if (option == AltOptions.Disable)
            return !IsAlt;
        else if (option == AltOptions.PreferAlt)
            return IsAlt;
        else
            return true;
    }
}