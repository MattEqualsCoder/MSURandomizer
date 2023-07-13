using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Models;

public class MsuSelectorResponse
{
    public Msu? Msu { get; init; }
    public ICollection<Msu>? Msus { get; init; }
    public bool Successful { get; init; }
    public string? Message { get; init; }
}