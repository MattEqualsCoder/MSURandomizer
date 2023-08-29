using System.ComponentModel;

namespace MSURandomizerLibrary;

/// <summary>
/// Options for randomizing alt songs
/// </summary>
public enum AltOptions
{
    /// <summary>
    /// Alts should be randomly selected
    /// </summary>
    [Description("Randomize")]
    Randomize,
    
    /// <summary>
    /// Always use the base tracks instead of alt tracks
    /// </summary>
    [Description("Disable Alt Tracks")]
    Disable,
    
    /// <summary>
    /// Prefer alt tracks over base tracks if available
    /// </summary>
    [Description("Prefer Alt Tracks")]
    PreferAlt,
    
    /// <summary>
    /// Don't change tracks at all
    /// </summary>
    [Description("Leave Tracks Alone")]
    LeaveAlone
}