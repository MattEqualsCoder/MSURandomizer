using System;
using System.Collections.Generic;
using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary;

/// <summary>
/// Event args for when the MSURandomizer has either updated the options
/// or generated a new MSU
/// </summary>
public class MsuLookupEventArgs : EventArgs
{
    public MsuLookupEventArgs(IReadOnlyCollection<Msu> msus)
    {
        Msus = msus;
    }
    
    public IReadOnlyCollection<Msu> Msus { get; }
}