namespace MSURandomizerLibrary.MSUTypes;

public class MSUTrackRangedModifier
{
    public int MinimumTrackNumber { get; set; }
    public int MaximumTrackNumber { get; set; }
    public int Modifier { get; set; }
    public bool OnlyAddIfMissing { get; set; }
    public bool SkipOriginalIfConversionExists { get; set; }
}