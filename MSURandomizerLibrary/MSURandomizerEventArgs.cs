using System;

namespace MSURandomizerLibrary;

/// <summary>
/// Event args for when the MSURandomizer has either updated the options
/// or generated a new MSU
/// </summary>
public class MSURandomizerEventArgs : EventArgs
{
    public MSURandomizerEventArgs(MSURandomizerOptions options)
    {
        Options = options;
    }
    
    public MSURandomizerOptions Options { get; }
}