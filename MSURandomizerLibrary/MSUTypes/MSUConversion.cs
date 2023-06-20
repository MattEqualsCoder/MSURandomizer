using System;
using System.Collections.Generic;

namespace MSURandomizerLibrary.MSUTypes;

public class MSUConversion
{
    public required string MSUType { get; set; }
    public required int DefaultModifier { get; set; }
    public int MinimumTrackNumber { get; set; } = Int32.MinValue;
    public int MaximumTrackNumber { get; set; } = Int32.MaxValue;
    public List<MSUTrackRemapping>? ManualRemaps { get; set; }
    public List<MSUTrackRangedModifier>? RangedModifiers { get; set; }
}