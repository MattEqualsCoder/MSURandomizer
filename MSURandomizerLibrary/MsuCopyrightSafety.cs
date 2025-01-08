using System.ComponentModel;

namespace MSURandomizerLibrary;

/// <summary>
/// Shuffle for copyright strike safety
/// </summary>
public enum MsuCopyrightSafety
{
    /// <summary>
    /// Use all songs
    /// </summary>
    [Description("Use all songs")]
    All,
    
    /// <summary>
    /// Skip tracks that are known to have copyright strikes
    /// </summary>
    [Description("Skip tracks that are known to have copyright strikes")]
    IgnoreUnsafe,
    
    /// <summary>
    /// Only use songs that are known to be safe from copyright strikes"
    /// </summary>
    [Description("Only use songs that are known to be safe from copyright strikes")]
    SafeTracksOnly
}