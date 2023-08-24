namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Represents track details of an MSU type
/// </summary>
public class MsuTypeTrack
{
    /// <summary>
    /// The number of the track for pcm files
    /// </summary>
    public required int Number { get; init; }
    
    /// <summary>
    /// The name of the track
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// A track that this track should fall back to if it does not exist
    /// </summary>
    public int Fallback { get; init; }
    
    /// <summary>
    /// If there is another track this one goes along with with creating shuffled MSUs
    /// </summary>
    public int? PairedTrack { get; set; }
    
    /// <summary>
    /// If this is a track that's part of extended MSU support
    /// </summary>
    public bool IsExtended { get; init; }
    
    /// <summary>
    /// If this is a track that does not loop
    /// </summary>
    public bool NonLooping { get; init; }
    
    /// <summary>
    /// If this track should be ignored or not as it's not used
    /// </summary>
    public bool IsIgnored { get; init; }
    
    /// <summary>
    /// The name of the track to appear in MSU details files
    /// </summary>
    public string? YamlName { get; set; }
    
    /// <summary>
    /// Override names to appear in MSU details files
    /// </summary>
    public string? YamlNameSecondary { get; set; }
}