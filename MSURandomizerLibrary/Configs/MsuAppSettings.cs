using System.Collections.Generic;
using System.Linq;

namespace MSURandomizerLibrary.Configs;

public class MsuAppSettings
{
    public Dictionary<string, string> MsuNameOverrides { get; set; } = new Dictionary<string, string>();
    public List<string> Smz3MsuTypes { get; set; } = new List<string>();
    public List<string> MetroidFirstMsuTypes { get; set; } = new List<string>();
    public List<string> ZeldaFirstMsuTypes { get; set; } = new List<string>();
    public string UserOptionsFilePath { get; set; } = "";
    public string MsuTypeFilePath { get; set; } = "";
    public int ContinuousReshuffleSeconds { get; set; } = 60;
    public bool MsuWindowDisplayRandomButton { get; set; }
    public bool MsuWindowDisplayShuffleButton { get; set; }
    public bool MsuWindowDisplayContinuousButton { get; set; }
    public bool MsuWindowDisplayOptionsButton { get; set; }
    public bool MsuWindowDisplaySelectButton { get; set; }
    public string? ForcedMsuType { get; set; }

    public string GetMsuName(string msuName)
    {
        return MsuNameOverrides.TryGetValue(msuName, out var overrideName) ? overrideName : msuName;
    }

    public IEnumerable<string> ZeldaSuperMetroidSmz3MsuTypes => MetroidFirstMsuTypes.Concat(ZeldaFirstMsuTypes);
}