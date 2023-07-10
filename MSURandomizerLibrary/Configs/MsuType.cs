using System.Collections.Generic;

namespace MSURandomizerLibrary.Configs;

public class MsuType
{
    public delegate int Conversion(int OtherTrackNum);
    public required string Name { get; init; }
    public bool Selectable { get; init; } = true;
    public required HashSet<int> RequiredTrackNumbers { get; init; }
    public required HashSet<int> ValidTrackNumbers { get; init; }
    public required IEnumerable<MsuTypeTrack> Tracks { get; init; }
    public Dictionary<MsuType, Conversion> Conversions = new();
    public List<MsuType> ExactMatches = new();

    public bool IsCompatibleWith(MsuType type)
    {
        return Conversions.ContainsKey(type) || Name == type.Name;
    }
    
    public bool IsExactMatchWith(MsuType type)
    {
        return this == type || Name == type.Name || ExactMatches.Contains(type) || type.ExactMatches.Contains(this);
    }
}