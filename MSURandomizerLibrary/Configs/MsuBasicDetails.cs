namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Class that houses the basic MSU metadata
/// </summary>
public class MsuBasicDetails
{
    /// <summary>
    /// The name of the MSU
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// The creator of the MSU
    /// </summary>
    public string? Creator { get; set; }
    
    /// <summary>
    /// The full path to the MSU
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// The msu type
    /// </summary>
    public string MsuTypeName { get; set; } = "";
}