using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.GameConnectors;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Interface for the service that interacts with games by reading their memory
/// </summary>
public interface IMsuGameService : IDisposable
{
    /// <summary>
    /// Event for when the currently played track has changed
    /// </summary>
    public event TrackNumberChangedEventHandler OnTrackChanged;

    /// <summary>
    /// Sets the MSU type to know what game should be monitored for changes
    /// </summary>
    /// <param name="msuType">The MSU type</param>
    public void SetMsuType(MsuType msuType);

    /// <summary>
    /// Disconnects from the current SNES connector
    /// </summary>
    public void Disconnect();
    
    /// <summary>
    /// The current MsuTypeTrack being played
    /// </summary>
    public MsuTypeTrack? CurrentTrack { get; }

    /// <summary>
    /// returns if a given MSU type is configured for the game service to read the current playing song
    /// </summary>
    /// <param name="msuType">The MSU type</param>
    /// <returns>True if the MSU type can have the current playing song read, false otherwise</returns>
    public bool IsMsuTypeCompatible(MsuType msuType);
}