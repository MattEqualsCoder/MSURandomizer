using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

public class MsuDetailsGeneric : MsuDetails
{
    [YamlMember(Order = 500)]
    public ICollection<MsuDetailsTrack>? Tracks { get; set; }
}