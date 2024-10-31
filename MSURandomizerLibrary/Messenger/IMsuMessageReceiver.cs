using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Messenger;

/// <summary>
/// Class for receiving messages from the MSU Randomizer app
/// </summary>
public interface IMsuMessageReceiver
{
    /// <summary>
    /// Event for when a new track is being played
    /// </summary>
    public event EventHandler<MsuTrackChangedEventArgs>? TrackChanged;
    
    /// <summary>
    /// Event for when a msu have been generated (including re-shuffles)
    /// </summary>
    public event EventHandler<MsuGeneratedEventArgs>? MsuGenerated;
    
    /// <summary>
    /// Initializes the message receiver to start receiving messages
    /// </summary>
    /// <returns></returns>
    public IMsuMessageReceiver Initialize();
}