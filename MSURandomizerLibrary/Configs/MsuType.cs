namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Represents an MSU type for a particular game
/// </summary>
public class MsuType
{
    /// <summary>
    /// Delegate for converting one MSU type to another
    /// </summary>
    public delegate int Conversion(int OtherTrackNum);
    
    /// <summary>
    /// The name (or game name) of the MSU type
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The name to present to the user
    /// </summary>
    public required string DisplayName { get; init; }
    
    /// <summary>
    /// If this MSU type should show up in any dropdown lists
    /// </summary>
    public bool Selectable { get; init; } = true;
    
    /// <summary>
    /// The minimum required tracks for an MSU of this type
    /// </summary>
    public required HashSet<int> RequiredTrackNumbers { get; init; }
    
    /// <summary>
    /// All possible tracks for this MSU type
    /// </summary>
    public required HashSet<int> ValidTrackNumbers { get; init; }
    
    /// <summary>
    /// List of all of the tracks for this MSU type
    /// </summary>
    public required IEnumerable<MsuTypeTrack> Tracks { get; init; }
    
    /// <summary>
    /// Any possible conversions for this MSU type to another MSU type
    /// </summary>
    public Dictionary<MsuType, Conversion> Conversions = new();
    
    /// <summary>
    /// MSU types that have a 1:1 conversion with this one
    /// </summary>
    public List<MsuType> ExactMatches = new();

    /// <summary>
    /// If this MSU type is compatible with another MSU type
    /// </summary>
    /// <param name="type">The MSU type to compare to</param>
    /// <returns>True if compatible, false otherwise</returns>
    public bool IsCompatibleWith(MsuType type)
    {
        return Conversions.ContainsKey(type) || DisplayName == type.DisplayName;
    }
    
    /// <summary>
    /// If this MSU type matches another
    /// </summary>
    /// <param name="type">The MSU type to compare against</param>
    /// <returns>True if an exact match, false otherwise</returns>
    public bool IsExactMatchWith(MsuType type)
    {
        return this == type || DisplayName == type.DisplayName || ExactMatches.Contains(type) || type.ExactMatches.Contains(this);
    }
}