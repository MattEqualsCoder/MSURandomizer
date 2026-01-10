using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Extensions;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class CurrentPlayingTrackControl : UserControl
{
    private readonly CurrentPlayingTrackService? _service;
    private readonly Canvas? _canvas;
    private readonly TextBlock? _textBlock;
    
    public CurrentPlayingTrackControl()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
        {
            DataContext = new CurrentPlayingTrackViewModel();
            return;
        }
        
        _canvas = this.Get<Canvas>(nameof(OuterCanvas));
        _textBlock = this.Get<TextBlock>(nameof(SongTextBlock));

        _service = this.GetControlService<CurrentPlayingTrackService>();
        DataContext = _service!.InitializeModel();
        _service.TrackChanged += (sender, args) =>
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                 Dispatcher.UIThread.Post(StartMarquee);
            });
        };
    }

    private void StartMarquee()
    {
        if (_canvas == null || _textBlock == null)
        {
            return;
        }
        
        var outerWidth = _canvas.Bounds.Width;
        var innerWidth = _textBlock.Bounds.Width;
        _service?.StartMarquee(innerWidth, outerWidth);
    }

    private void Control_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _service?.Shutdown();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        
    }
}