using System.Collections.Generic;

namespace MSURandomizerLibrary.Configs;

public class MsuDetailsGeneric : MsuDetails
{
    public ICollection<MsuDetailsTrack>? Tracks { get; set; }
}