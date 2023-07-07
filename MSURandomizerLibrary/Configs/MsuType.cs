using System.Collections.Generic;

namespace MSURandomizerLibrary.Configs;

public class MsuType
{
    public delegate int Conversion(int OtherTrackNum);
    public required string Name { get; init; }
    public required HashSet<int> RequiredTrackNumbers { get; init; }
    public required HashSet<int> ValidTrackNumbers { get; init; }
    public required IEnumerable<MsuTypeTrack> Tracks { get; init; }
    public Dictionary<MsuType, Conversion> Conversions = new();

    public bool IsCompatibleWith(MsuType type)
    {
        return Conversions.ContainsKey(type) || Name == type.Name;
    }
}