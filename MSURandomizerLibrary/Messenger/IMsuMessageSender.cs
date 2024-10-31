using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Messenger;

/// <summary>
/// Class for sending messages about changes in the MSU and tracks
/// </summary>
public interface IMsuMessageSender
{
    /// <summary>
    /// Sends a message to the receiver that the currently playing track has changed
    /// </summary>
    /// <param name="track">Details about the track being played</param>
    public Task SendTrackChangedAsync(Track track);
    
    /// <summary>
    /// Sends a message to the receiver that the MSU has been regenerated (including re-shuffles)
    /// </summary>
    /// <param name="msu">The MSU that was generated</param>
    public Task SendMsuGenerated(Msu msu);
}