using System.Collections.Generic;

namespace MsuRandomizerLibrary.Configs;

public class MsuDetailsGeneric : MsuDetails
{
    public ICollection<MsuDetailsTrack>? Tracks { get; set; }
}