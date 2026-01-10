using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Messenger;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

internal class MsuMonitorService(
    ILogger<MsuMonitorService> logger,
    IMsuGameService gameService,
    IMsuSelectorService msuSelectorService,
    IMsuUserOptionsService msuUserOptions,
    MsuAppSettings msuAppSettings,
    IMsuMessageSender msuMessageSender)
    : IMsuMonitorService
{
    private Msu? _currentMsu;
    private Track? _currentTrack;
    private CancellationTokenSource _cts = new();
    private string? _outputPath;
    private bool _monitorEnabled = false;

    public event MsuTrackChangedEventHandler? MsuTrackChanged;

    public event EventHandler? PreMsuShuffle;
    
    public event EventHandler? MsuShuffled;
    
    public event EventHandler? MsuMonitorStarted;
    
    public event EventHandler? MsuMonitorStopped;

    public async Task StartShuffle(MsuSelectorRequest request, int seconds = 60)
    {
        logger.LogInformation("Start shuffling");
        UpdateOutputPath();
        
        if (request.OutputMsuType != null && gameService.IsMsuTypeCompatible(request.OutputMsuType))
        {
            gameService.SetMsuType(request.OutputMsuType!);
            gameService.OnTrackChanged += GameServiceOnOnTrackChanged;
            _monitorEnabled = true;
        }
        
        _currentMsu = null;
        _currentTrack = null;
        MsuMonitorStarted?.Invoke(this, EventArgs.Empty);
        _cts = new CancellationTokenSource();

        var preDelay = TimeSpan.FromMilliseconds(100);
        var postDelay = TimeSpan.FromSeconds(seconds).Subtract(preDelay);
        
        do
        {
            PreMsuShuffle?.Invoke(this, EventArgs.Empty);
            await Task.Delay(preDelay, _cts.Token);
            request.CurrentTrack = _currentTrack;
            request.PrevMsu = _currentMsu;
            var response = msuSelectorService.CreateShuffledMsu(request);
            _currentMsu = response.Msu;
            MsuShuffled?.Invoke(this, EventArgs.Empty);
            await Task.Delay(postDelay, _cts.Token);
        } while (!_cts.Token.IsCancellationRequested);
    }
    
    public void StartMonitor(Msu msu, MsuType msuType)
    {
        logger.LogInformation("Start monitor");
        UpdateOutputPath();
        gameService.SetMsuType(msuType);
        gameService.OnTrackChanged += GameServiceOnOnTrackChanged;
        _monitorEnabled = true;
        _currentMsu = msu;
        _currentTrack = null;
        MsuMonitorStarted?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        logger.LogInformation("Stop monitor");
        _cts.Cancel();
        
        if (_monitorEnabled)
        {
            gameService.OnTrackChanged -= GameServiceOnOnTrackChanged;
            gameService.Disconnect();    
        }
        
        _currentMsu = null;
        _currentTrack = null;
        MsuMonitorStopped?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateOutputPath()
    {
        _outputPath = msuUserOptions.MsuUserOptions.MsuCurrentSongOutputFilePath;
        if (string.IsNullOrWhiteSpace(_outputPath))
        {
            _outputPath = msuAppSettings.DefaultMsuCurrentSongOutputFilePath;
        }
    }

    private void GameServiceOnOnTrackChanged(object sender, TrackNumberChangedEventArgs e)
    {
        _currentTrack = _currentMsu?.Tracks.Where(x => x.Number == e.TrackNumber).OrderBy(x => x.IsAlt).FirstOrDefault();
        var currentMsuTypeTrack = _currentMsu?.MsuType?.Tracks.FirstOrDefault(x => x.Number == e.TrackNumber);
        logger.LogInformation("Track changed #{Number} ({TrackName}): {Song}", e.TrackNumber, currentMsuTypeTrack?.Name, _currentTrack?.GetDisplayText(TrackDisplayFormat.HorizonalWithMsu));
        
        if (_currentTrack != null)
        {
            MsuTrackChanged?.Invoke(this, new MsuTrackChangedEventArgs(_currentTrack));
            _ = msuMessageSender.SendTrackChangedAsync(_currentTrack);
            
            if (string.IsNullOrEmpty(_outputPath))
            {
                return;
            }
            try
            {
                _ = File.WriteAllTextAsync(_outputPath, _currentTrack.GetDisplayText(msuUserOptions.MsuUserOptions.TrackDisplayFormat));
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