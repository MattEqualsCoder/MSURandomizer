using System;

namespace MSURandomizerLibrary;

public class MSURandomizerEventArgs : EventArgs
{
    public MSURandomizerEventArgs(MSURandomizerOptions options)
    {
        Options = options;
    }
    
    public MSURandomizerOptions Options { get; }
}