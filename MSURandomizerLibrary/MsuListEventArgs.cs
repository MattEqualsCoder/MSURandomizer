using System;
using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary;

/// <summary>
/// Event args for when the MSURandomizer has either updated the options
/// or generated a new MSU
/// </summary>
public class MsuListEventArgs : EventArgs
{
    public MsuListEventArgs(IReadOnlyCollection<Msu> msus)
    {
        Msus = msus;
    }
    
    public IReadOnlyCollection<Msu> Msus { get; }
}