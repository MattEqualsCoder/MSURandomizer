using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Models;

/// <summary>
/// Class for the details of when a track has changed
/// </summary>
public class MsuTrackChangedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="track">The current track being played</param>
    public MsuTrackChangedEventArgs(Track track)
    {
        Track = track;
    }
    
    /// <summary>
    /// The current track being played
    /// </summary>
    public Track Track { get; set; }
}