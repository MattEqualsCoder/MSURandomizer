namespace MSURandomizerLibrary.Models;

/// <summary>
/// Class for holding the details when the currently played track number has changed
/// </summary>
public class TrackNumberChangedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="number">The number of the track being played</param>
    public TrackNumberChangedEventArgs(byte number)
    {
        TrackNumber = number;
    }
    
    /// <summary>
    /// The number of the track being played
    /// </summary>
    public int TrackNumber { get; }
}