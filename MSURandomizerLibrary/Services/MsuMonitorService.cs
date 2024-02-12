using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

internal class MsuMonitorService(
    ILogger<MsuMonitorService> logger,
    IMsuGameService gameService,
    IMsuSelectorService msuSelectorService,
    MsuUserOptions msuUserOptions,
    MsuAppSettings msuAppSettings)
    : IMsuMonitorService
{
    private Msu? _currentMsu;
    private Track? _currentTrack;
    private readonly CancellationTokenSource _cts = new();
    private string? _outputPath;

    public event MsuTrackChangedEventHandler? MsuTrackChanged;

    public event EventHandler? MsuShuffled;

    public async Task StartShuffle(MsuSelectorRequest request, int seconds = 60)
    {
        logger.LogInformation("Start shuffling");
        UpdateOutputPath();
        gameService.SetMsuType(request.OutputMsuType!);
        gameService.OnTrackChanged += GameServiceOnOnTrackChanged;
        _currentMsu = null;
        _currentTrack = null;
        
        do
        {
            request.CurrentTrack = _currentTrack;
            var response = msuSelectorService.CreateShuffledMsu(request);
            _currentMsu = response.Msu;
            MsuShuffled?.Invoke(this, EventArgs.Empty);
            await Task.Delay(TimeSpan.FromSeconds(seconds), _cts.Token);
        } while (!_cts.Token.IsCancellationRequested);
    }
    
    public void StartMonitor(Msu msu)
    {
        logger.LogInformation("Start monitor");
        UpdateOutputPath();
        gameService.SetMsuType(msu.MsuType!);
        gameService.OnTrackChanged += GameServiceOnOnTrackChanged;
        _currentMsu = msu;
        _currentTrack = null;
    }

    public void Stop()
    {
        logger.LogInformation("Stop monitor");
        _cts.Cancel();
        gameService.OnTrackChanged -= GameServiceOnOnTrackChanged;
        gameService.Disconnect();
        _currentMsu = null;
        _currentTrack = null;
    }

    private void UpdateOutputPath()
    {
        _outputPath = msuUserOptions.MsuCurrentSongOutputFilePath;
        if (string.IsNullOrWhiteSpace(_outputPath))
        {
            _outputPath = msuAppSettings.DefaultMsuCurrentSongOutputFilePath;
        }
    }

    private void GameServiceOnOnTrackChanged(object sender, TrackNumberChangedEventArgs e)
    {
        _currentTrack = _currentMsu?.Tracks.FirstOrDefault(x => x.Number == e.TrackNumber);
        var currentMsuTypeTrack = _currentMsu?.MsuType?.Tracks.FirstOrDefault(x => x.Number == e.TrackNumber);
        logger.LogInformation("Track changed #{Number} ({TrackName}): {Song}", e.TrackNumber, currentMsuTypeTrack?.Name, _currentTrack?.GetDisplayText(TrackDisplayFormat.HorizonalWithMsu));
        
        if (_currentTrack != null)
        {
            MsuTrackChanged?.Invoke(this, new MsuTrackChangedEventArgs(_currentTrack));
            if (string.IsNullOrEmpty(_outputPath))
            {
                return;
            }
            try
            {
                _ = File.WriteAllTextAsync(_outputPath, _currentTrack.GetDisplayText(msuUserOptions.TrackDisplayFormat));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to write current track details to {Path}", _outputPath);
            }
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
        gameService.Dispose();
        GC.SuppressFinalize(this);
    }
}