namespace MSURandomizerLibrary.Configs;

public class MsuTypeTrack
{
    public required int Number { get; init; }
    public required string Name { get; init; }
    public int Fallback { get; init; }
    public int PairedTrack { get; init; }
    public bool IsExtended { get; init; }
    public bool NonLooping { get; init; }
    public bool IsIgnored { get; init; }
}