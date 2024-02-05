using System.ComponentModel;

namespace MSURandomizerLibrary;

/// <summary>
/// Enum for dictating how tracks should be shuffled
/// </summary>
public enum MsuShuffleStyle
{
    /// <summary>
    /// Standard shuffle where for a given track, only songs meant for that track are selected
    /// </summary>
    [Description("Standard shuffle - shuffle songs meant for that location")]
    StandardShuffle,
    
    /// <summary>
    /// Same as the regular standard shuffle, only for related tracks it will try to use the same MSU
    /// </summary>
    [Description("Standard shuffle - shuffle songs meant for that location, pairing related tracks such as a dungeon and its boss theme")]
    ShuffleWithPairedTracks,
    
    /// <summary>
    /// Shuffle mode where for a given track, it'll include songs intended for other tracks. For special tracks such as the  ALttP
    /// crystal get theme, it'll continue to only allow songs for that given track
    /// </summary>
    [Description("Chaos shuffle - shuffle in songs not meant for a given location, ignoring special tracks")]
    ChaosNonSpecialTracks,
    
    /// <summary>
    /// Shuffle mode where literally any song can be picked. Even for special tracks such as 
    /// </summary>
    [Description("Chaos shuffle - shuffle in songs not meant for a given location, including special tracks like the crystal get")]
    ChaosAllTracks
}