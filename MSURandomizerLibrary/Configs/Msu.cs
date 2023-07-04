using System.Collections.Generic;
using System.Linq;

namespace MsuRandomizerLibrary.Configs;

public class Msu
{
    public MsuType? MsuType { get; set; }
    public required string Name { get; set; }
    public string? Creator { get; set; }
    public required string FolderName { get; set; }
    public required string FileName { get; set; }
    public required string Path { get; set; }
    public required ICollection<Track> Tracks { get; set; }
    public MsuSettings Settings { get; set; } = new() { MsuPath = "" };
    public string DisplayName => string.IsNullOrWhiteSpace(Settings.Name) ? Name : Settings.Name;
    public string? DisplayCreator => string.IsNullOrWhiteSpace(Settings.Creator) ? Creator : Settings.Creator;
    public string FullName => DisplayName + (string.IsNullOrWhiteSpace(DisplayCreator) ? "" : $" by {DisplayCreator}");
    public string MsuTypeName => string.IsNullOrWhiteSpace(MsuType?.Name) ? "Unknown" : MsuType.Name;
    public ICollection<Track> ValidTracks => Tracks.Where(x => Settings.AllowAltTracks || !x.IsAlt).ToList();
}