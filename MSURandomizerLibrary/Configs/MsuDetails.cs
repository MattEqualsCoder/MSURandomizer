namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Represents a YAML file with additional details about a MSU
/// </summary>
public class MsuDetails
{
    /// <summary>
    /// The name of the MSU
    /// </summary>
    public string? PackName { get; set; }
    
    /// <summary>
    /// The creator of the MSU
    /// </summary>
    public string? PackAuthor { get; set; }
    
    /// <summary>
    /// The current version of the MSU
    /// </summary>
    public string? PackVersion { get; set; }
    
    /// <summary>
    /// The default artist of the MSU
    /// </summary>
    public string? Artist { get; set; }
    
    /// <summary>
    /// The default album of the MSU
    /// </summary>
    public string? Album { get; set; }
    
    /// <summary>
    /// The default url for the MSU
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// A specified type for the MSU
    /// </summary>
    public string? MsuType { get; set; }
    
    /// <summary>
    /// All of the details about the tracks in the MSU
    /// </summary>
    public Dictionary<string, MsuDetailsTrack>? Tracks { get; set; }

}