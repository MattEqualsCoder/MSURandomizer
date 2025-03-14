using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Interface for the service to monitor the currently played track and reshuffle the MSU
/// </summary>
public interface IMsuMonitorService : IDisposable
{
    /// <summary>
    /// Event for when the current playing track has changed
    /// </summary>
    public event MsuTrackChangedEventHandler? MsuTrackChanged;
    
    /// <summary>
    /// Event for before the MSU has been reshuffled
    /// </summary>
    public event EventHandler? PreMsuShuffle;
    
    /// <summary>
    /// Event for when the MSU has been reshuffled
    /// </summary>
    public event EventHandler? MsuShuffled;
    
    /// <summary>
    /// Event fired off when the MSU monitor has started
    /// </summary>
    public event EventHandler? MsuMonitorStarted;
    
    /// <summary>
    /// Event fired off when the MSU monitor has stopped
    /// </summary>
    public event EventHandler? MsuMonitorStopped;

    /// <summary>
    /// Starts shuffling the MSU on a certain interval
    /// </summary>
    /// <param name="request">The shuffle request to redo</param>
    /// <param name="seconds">The frequency in which to reshuffle</param>
    /// 
    public Task StartShuffle(MsuSelectorRequest request, int seconds = 60);

    /// <summary>
    /// Starts the monitor for a specific MSU to detect when the current song has changed
    /// </summary>
    /// <param name="msu">The MSU to play</param>
    /// <param name="msuType">The MSU type to use for monitoring</param>
    public void StartMonitor(Msu msu, MsuType msuType);

    /// <summary>
    /// Stops the monitor
    /// </summary>
    public void Stop();
}