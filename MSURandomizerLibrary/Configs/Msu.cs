using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

public class Msu
{
    public Msu()
    {
    }
    
    public Msu(MsuType? type, string name, string folderName, string fileName, string path, ICollection<Track> tracks, MsuDetails? msuDetails, Msu? prevMsu)
    {
        MsuType = type;
        Name = name;
        FolderName = folderName;
        FileName = fileName;
        Path = path;
        Tracks = tracks;

        Version = msuDetails?.PackVersion ?? prevMsu?.Version;
        Creator = msuDetails?.PackAuthor ?? prevMsu?.Creator;
        Artist = msuDetails?.Artist ?? prevMsu?.Artist;
        Album = msuDetails?.Album ?? prevMsu?.Album;
        Url = msuDetails?.Url ?? prevMsu?.Url;
        HasDetails = msuDetails != null || prevMsu?.HasDetails == true;

        foreach (var track in tracks)
        {
            track.Msu = this;
        }
    }
    
    [JsonIgnore]
    public MsuType? MsuType { get; set; }

    public string Name { get; set; } = "";
    public string? Creator { get; set; }
    public string? Version { get; set; }
    public bool HasDetails { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Url { get; set; }
    public string FolderName { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Path { get; set; } = "";
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    
    [JsonIgnore]
    public MsuSettings Settings { get; set; } = new() { MsuPath = "" };
    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Settings.Name) ? Name : Settings.Name;
    [JsonIgnore]
    public string DisplayCreator => string.IsNullOrWhiteSpace(Settings.Creator) ? Creator ?? "" : Settings.Creator;
    [JsonIgnore]
    public MsuType? SelectedMsuType => Settings.MsuType ?? MsuType;
    [JsonIgnore]
    public string FullName => DisplayName + (string.IsNullOrWhiteSpace(DisplayCreator) ? "" : $" by {DisplayCreator}");
    [JsonIgnore]
    public string MsuTypeName => string.IsNullOrWhiteSpace(SelectedMsuType?.DisplayName) ? "Unknown" : SelectedMsuType.DisplayName;
    [JsonIgnore]
    public ICollection<Track> ValidTracks => Tracks.Where(x => Settings.AllowAltTracks || !x.IsAlt).ToList();
    [JsonIgnore]
    public int NumUniqueTracks => Tracks.Select(x => x.Path).Distinct().Count();

    public bool MatchesFilter(MsuFilter filter, MsuType type, string? path)
    {
        return (filter == MsuFilter.All ||
                (filter == MsuFilter.Compatible && SelectedMsuType?.IsCompatibleWith(type) == true) ||
                (filter == MsuFilter.Exact && SelectedMsuType?.IsExactMatchWith(type) == true)) &&
               (string.IsNullOrEmpty(path) || Path.StartsWith(path)) &&
               Tracks.Count >= 1;
    }
}