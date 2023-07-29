namespace MSURandomizerLibrary.Configs;

public class MsuDetails
{
    public string? PackName { get; set; }
    public string? PackAuthor { get; set; }
    public string? PackVersion { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? MsuType { get; set; }
    public string? Url { get; set; }
    public Dictionary<string, MsuDetailsTrack>? Tracks { get; set; }

}