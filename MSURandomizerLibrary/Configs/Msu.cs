using System.Collections.Generic;
using System.Linq;

namespace MSURandomizerLibrary.Configs;

public class Msu
{
    public MsuType? MsuType { get; set; }
    public required string Name { get; set; }
    public string? Creator { get; set; }
    public bool HasDetails { get; set; }
    public required string FolderName { get; set; }
    public required string FileName { get; set; }
    public required string Path { get; set; }
    public required ICollection<Track> Tracks { get; set; }
    public MsuSettings Settings { get; set; } = new() { MsuPath = "" };
    public string DisplayName => string.IsNullOrWhiteSpace(Settings.Name) ? Name : Settings.Name;
    public string DisplayCreator => string.IsNullOrWhiteSpace(Settings.Creator) ? Creator ?? "" : Settings.Creator;
    public MsuType? SelectedMsuType => Settings.MsuType ?? MsuType; 
    public string FullName => DisplayName + (string.IsNullOrWhiteSpace(DisplayCreator) ? "" : $" by {DisplayCreator}");
    public string MsuTypeName => string.IsNullOrWhiteSpace(SelectedMsuType?.Name) ? "Unknown" : SelectedMsuType.Name;
    public ICollection<Track> ValidTracks => Tracks.Where(x => Settings.AllowAltTracks || !x.IsAlt).ToList();
    public int NumUniqueTracks => Tracks.Select(x => x.Path).Distinct().Count();

    public bool MatchesFilter(MsuFilter filter, MsuType type, string? path)
    {
        return (filter == MsuFilter.All ||
                (filter == MsuFilter.Compatible && SelectedMsuType?.IsCompatibleWith(type) == true) ||
                (filter == MsuFilter.Exact && SelectedMsuType?.IsExactMatchWith(type) == true)) &&
               (string.IsNullOrEmpty(path) || Path.StartsWith(path));
    }
}