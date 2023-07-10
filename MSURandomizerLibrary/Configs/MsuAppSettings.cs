using System.Collections.Generic;

namespace MSURandomizerLibrary.Configs;

public class MsuAppSettings
{
    public Dictionary<string, string> MsuNameOverrides { get; set; } = new Dictionary<string, string>();
    public List<string> Smz3MsuTypes { get; set; } = new List<string>();
    public string Smz3MsuTypeName { get; set; } = "";
    public string Smz3LegacyMsuTypeName { get; set; } = "";

    public string UserSettingsFilePath { get; set; } = "";
    public string MsuTypeFilePath { get; set; } = "";
    public int ContinuousReshuffleSeconds { get; set; } = 60;

    public string GetMsuName(string msuName)
    {
        return MsuNameOverrides.TryGetValue(msuName, out var overrideName) ? overrideName : msuName;
    }
}