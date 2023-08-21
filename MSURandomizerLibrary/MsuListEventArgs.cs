using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary;

/// <summary>
/// Event args for when the MSURandomizer has either updated the options
/// or generated a new MSU
/// </summary>
public class MsuListEventArgs : EventArgs
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msus">The list of MSUs that were found</param>
    /// <param name="errors">Any errors that were found for the MSUs</param>
    public MsuListEventArgs(IReadOnlyCollection<Msu> msus, IReadOnlyDictionary<string, string>? errors)
    {
        Msus = msus;
        Errors = errors;
    }
    
    /// <summary>
    /// The list of found MSUs
    /// </summary>
    public IReadOnlyCollection<Msu> Msus { get; }
    
    /// <summary>
    /// Errors that were found for specific MSUs
    /// </summary>
    public IReadOnlyDictionary<string, string>? Errors { get; }
}