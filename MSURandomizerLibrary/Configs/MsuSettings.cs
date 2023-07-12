using YamlDotNet.Serialization;

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
    public string? MsuTypeName { get; set; }
    public bool AllowAltTracks { get; set; } = true;
    public string? Name { get; set; }
    public string? Creator { get; set; }
    
    [YamlIgnore]
    public MsuType? MsuType { get; set; }

    [YamlIgnore] 
    public bool HasSettings => !string.IsNullOrEmpty(MsuTypeName) 
                                 || !string.IsNullOrEmpty(Name) 
                                 || !string.IsNullOrEmpty(Creator) 
                                 || !AllowAltTracks;
}