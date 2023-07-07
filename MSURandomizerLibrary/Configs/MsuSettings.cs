namespace MSURandomizerLibrary.Configs;

public class MsuSettings
{
    public required string MsuPath { get; set; }
    public string? MsuType { get; set; }
    public bool AllowAltTracks { get; set; } = true;
    public string? Name { get; set; }
    public string? Creator { get; set; }
}