using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.GameConnectors;
using MSURandomizerLibrary.Models;
using SnesConnectorLibrary;

namespace MSURandomizerLibrary.Services;

internal class MsuGameService(ILogger<MsuGameService> logger, ISnesConnectorService snesConnectorService)
    : IMsuGameService
{
    private IGameConnector? _currentGame;
    private readonly List<SnesRecurringMemoryRequest> _currentRequests = [];

    public event TrackNumberChangedEventHandler? OnTrackChanged;

    public void SetMsuType(MsuType msuType)
    {
        foreach (var request in _currentRequests)
        {
            snesConnectorService.RemoveRecurringRequest(request);
        }
        
        if (msuType.Name == "A Link to the Past")
        {
            _currentGame = new LttPGameConnector();
        }
        else if (msuType.Name == "Super Metroid / A Link to the Past Combination Randomizer")
        {
            _currentGame = new SMZ3GameConnector();
        }

        if (_currentGame == null)
        {
            return;
        }
        
        logger.LogInformation("Setup game connector");

        _currentGame.OnTrackChanged += (sender, args) =>
        {
            OnTrackChanged?.Invoke(sender, args);
            CurrentTrack = msuType.Tracks.FirstOrDefault(x => x.Number == args.TrackNumber);
        };

        foreach (var request in _currentGame.GetMemoryRequests())
        {
            _currentRequests.Add(snesConnectorService.AddRecurringRequest(request));
            snesConnectorService.AddRecurringRequest(request);
        }
        
    }

    public MsuTypeTrack? CurrentTrack { get; private set; }

    public void Dispose()
    {
        foreach (var request in _currentRequests)
        {
            snesConnectorService.RemoveRecurringRequest(request);
        }
        GC.SuppressFinalize(this);
    }
}