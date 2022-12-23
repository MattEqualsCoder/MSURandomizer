using System.Collections.Generic;

namespace MSURandomizerLibrary.MSUTypes;

public class MSUConversion
{
    public required string MSUType { get; set; }
    public required int DefaultModifier { get; set; }
    public List<MSUTrackRemapping>? ManualRemaps { get; set; }
}