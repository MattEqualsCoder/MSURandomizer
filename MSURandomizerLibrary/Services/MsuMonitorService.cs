using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

internal class MsuMonitorService(
    ILogger<MsuMonitorService> logger,
    IMsuGameService gameService,
    IMsuSelectorService msuSelectorService)
    : IMsuMonitorService
{
    private Msu? _currentMsu;
    private Track? _currentTrack;
    private readonly CancellationTokenSource _cts = new();

    public event MsuTrackChangedEventHandler? MsuTrackChanged;

    public async Task StartShuffle(MsuSelectorRequest request, int seconds = 60)
    {
        logger.LogInformation("StartShuffle");
        gameService.SetMsuType(request.OutputMsuType!);
        gameService.OnTrackChanged += GameServiceOnOnTrackChanged;
        _currentMsu = null;
        _currentTrack = null;
        
        do
        {
            request.CurrentTrack = _currentTrack;
            var response = msuSelectorService.CreateShuffledMsu(request);
            _currentMsu = response.Msu;
            await Task.Delay(TimeSpan.FromSeconds(seconds), _cts.Token);
        } while (!_cts.Token.IsCancellationRequested);
    }
    
    public void StartMonitor(Msu msu)
    {
        gameService.SetMsuType(msu.MsuType!);
        gameService.OnTrackChanged += GameServiceOnOnTrackChanged;
        _currentMsu = msu;
        _currentTrack = null;
    }

    public void Stop()
    {
        _cts.Cancel();
        gameService.OnTrackChanged -= GameServiceOnOnTrackChanged;
        _currentMsu = null;
        _currentTrack = null;
    }

    private void GameServiceOnOnTrackChanged(object sender, TrackNumberChangedEventArgs e)
    {
        logger.LogInformation("Track changed {Number}", e.TrackNumber);
        _currentTrack = _currentMsu?.Tracks.FirstOrDefault(x => x.Number == e.TrackNumber);
        if (_currentTrack != null)
        {
            MsuTrackChanged?.Invoke(this, new MsuTrackChangedEventArgs(_currentTrack));
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
        gameService.Dispose();
        GC.SuppressFinalize(this);
    }
}