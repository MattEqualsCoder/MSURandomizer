namespace MSURandomizerLibrary.Configs;

public class MsuSettings
{
    public MsuSettings()
    {
        MsuPath = "";
    }
    
    public MsuSettings(string path)
    {
        MsuPath = path;
    }
    
    public string MsuPath { get; set; }
    public string? MsuType { get; set; }
    public bool AllowAltTracks { get; set; } = true;
    public string? Name { get; set; }
    public string? Creator { get; set; }
}