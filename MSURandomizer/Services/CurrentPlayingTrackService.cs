using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizer.Services;

public class CurrentPlayingTrackService(
    IMsuMonitorService msuMonitorService, 
    ISnesConnectorService snesConnectorService,
    IMsuUserOptionsService msuUserOptionsService) : ControlService, IDisposable
{
    public CurrentPlayingTrackViewModel Model = new();
    private DispatcherTimer? _dispatcherTimer;
    private CancellationTokenSource _cts = new();

    public CurrentPlayingTrackViewModel InitializeModel()
    {
        msuMonitorService.MsuTrackChanged += MsuMonitorServiceOnMsuTrackChanged;
        snesConnectorService.Disconnected += SnesConnectorServiceOnOnDisconnected;
        
        _dispatcherTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1 / 30.0),
        };

        _dispatcherTimer.Tick += (sender, args) =>
        {
            Model.Ticks++;
            var fraction = Math.Clamp(Model.Ticks / Model.MaxTicks, 0, 1);
            Model.AnimationMargin = new Thickness(Model.TargetMargin * fraction, 0, 0, 0);

            if (fraction >= 1)
            {
                _dispatcherTimer.Stop();
                _ = RestartTimer();
            }
        };
        
        return Model;
    }

    public void Shutdown()
    {
        msuMonitorService.MsuTrackChanged -= MsuMonitorServiceOnMsuTrackChanged;
        snesConnectorService.Disconnected -= SnesConnectorServiceOnOnDisconnected;
    }

    public event EventHandler? TrackChanged;

    public void StartMarquee(double innerWidth, double outerWidth)
    {
        if (innerWidth < outerWidth)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            Model.MaxTicks = (innerWidth - outerWidth) / 50 * 30;
            Model.TargetMargin = outerWidth - innerWidth;
        });
        _ = StartTimer();
    }

    private async Task StartTimer()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Model.Ticks = 0;
            Model.AnimationMargin = new Thickness(0);
        });
        var cts = _cts = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
        if (cts.IsCancellationRequested) return;
        Dispatcher.UIThread.Post(() => { _dispatcherTimer?.Start(); });
    }

    private async Task RestartTimer()
    {
        var cts = _cts;
        if (cts.IsCancellationRequested) return;
        await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
        if (cts.IsCancellationRequested) return;
        _ = StartTimer();
    }

    private void SnesConnectorServiceOnOnDisconnected(object? sender, EventArgs e)
    {
        ChangeMessage(CurrentPlayingTrackViewModel.NotConnectedMessage);
    }

    private void MsuMonitorServiceOnMsuTrackChanged(object sender, MsuTrackChangedEventArgs e)
    {
        ChangeMessage(e.Track.GetDisplayText(msuUserOptionsService.MsuUserOptions.TrackDisplayFormat));
    }

    private void ChangeMessage(string message)
    {
        _cts.Cancel();
        
        Dispatcher.UIThread.Post(() =>
        {
            _dispatcherTimer?.Stop();
            Model.AnimationMargin = new Thickness(0);
            Model.Message = message;
        });
        
        TrackChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        msuMonitorService.Dispose();
        snesConnectorService.Dispose();
        GC.SuppressFinalize(this);
    }
}