using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

public class MsuDetailsSmz3 : MsuDetails
{
    [YamlMember(Order = 500)]
    public MsuDetailsTrackListSmz3? Tracks { get; set; }
}