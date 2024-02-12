using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Control for displaying the current playing song
/// </summary>
public partial class MsuCurrentPlayingTrackControl : UserControl, IDisposable
{
    private readonly DoubleAnimation _marquee = new();
    private CancellationTokenSource _cts = new();
    private IMsuMonitorService? _msuMonitorService;
    private ISnesConnectorService? _snesConnectorService;
    private IMsuUserOptionsService? _msuUserOptionsService;
    private ILogger<MsuCurrentPlayingTrackControl>? _logger;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msuMonitorService"></param>
    /// <param name="snesConnectorService"></param>
    /// <param name="msuUserOptionsService"></param>
    /// <param name="logger"></param>
    public MsuCurrentPlayingTrackControl(IMsuMonitorService? msuMonitorService, ISnesConnectorService? snesConnectorService, IMsuUserOptionsService? msuUserOptionsService, ILogger<MsuCurrentPlayingTrackControl>? logger)
    {
        _msuMonitorService = msuMonitorService;
        _snesConnectorService = snesConnectorService;
        _msuUserOptionsService = msuUserOptionsService;
        _logger = logger;
        InitializeComponent();
        
        if (_snesConnectorService == null || _msuMonitorService == null)
        {
            return;
        }
        
        _marquee.Completed += MarqueeOnCompleted;
        _snesConnectorService.OnDisconnected += SnesConnectorServiceOnOnDisconnected;
        _msuMonitorService.MsuTrackChanged += MsuMonitorServiceOnMsuTrackChanged;
    }

    private void MsuMonitorServiceOnMsuTrackChanged(object sender, MsuTrackChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            SongTextBlock.Text = e.Track.GetDisplayText(_msuUserOptionsService?.MsuUserOptions.TrackDisplayFormat ?? TrackDisplayFormat.Vertical);
            _cts.Cancel();
            _ = StartMarquee();
        });
    }

    private void SnesConnectorServiceOnOnDisconnected(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            SongTextBlock.Text = "No song detected. Connect to one of the Snes Connectors.";
        });
    }

    private void MsuCurrentPlayingTrackControl_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_snesConnectorService != null)
            _snesConnectorService.OnDisconnected -= SnesConnectorServiceOnOnDisconnected;
        if (_msuMonitorService != null)
            _msuMonitorService.MsuTrackChanged -= MsuMonitorServiceOnMsuTrackChanged;
    }

    private void MarqueeOnCompleted(object? sender, EventArgs e)
    {
        if (Math.Abs(Math.Round(Canvas.GetLeft(SongTextBlock)) - Math.Round(_target)) > .1)
        {
            _logger?.LogWarning("Canvas size does not match {Value} {Target}", Canvas.GetLeft(SongTextBlock), _target);
            return;
        }
        
        var cts = _cts;
        _ = Dispatcher.Invoke(async () =>
        {
            if (cts.IsCancellationRequested) return;
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            if (cts.IsCancellationRequested) return;
            _ = StartMarquee();
        });
    }

    private double _target;
    private async Task StartMarquee()
    {
        var cts = _cts = new CancellationTokenSource();
        await Dispatcher.Invoke(async () =>
        {
            SongTextBlock.BeginAnimation(Canvas.LeftProperty, null);
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            if (cts.IsCancellationRequested) return;
            var outerWidth = OuterCanvas.ActualWidth;
            var innerWidth = SongTextBlock.ActualWidth;
            if (innerWidth < outerWidth)
                return;
            _marquee.From = 0;
            _marquee.To = outerWidth - innerWidth;
            _target = outerWidth - innerWidth;
            _marquee.Duration = new Duration(TimeSpan.FromSeconds((innerWidth - outerWidth) / 50));
            SongTextBlock.BeginAnimation(Canvas.LeftProperty, _marquee);
        });
    }

    /// <summary>
    /// Disposes the control
    /// </summary>
    public void Dispose()
    {
        _cts.Dispose();
        _msuMonitorService?.Dispose();
        _snesConnectorService?.Dispose();
        GC.SuppressFinalize(this);
    }
}