using System.ComponentModel;

namespace MSURandomizerLibrary;

public enum AltOptions
{
    [Description("Randomize")]
    Randomize,
    
    [Description("Disable Alt Tracks")]
    Disable,
    
    [Description("Prefer Alt Tracks")]
    PreferAlt
}