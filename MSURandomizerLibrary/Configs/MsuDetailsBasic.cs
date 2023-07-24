using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

public class MsuDetailsBasic : MsuDetails
{
    [YamlMember(Order = 500)]
    public object? Tracks { get; set; }
}