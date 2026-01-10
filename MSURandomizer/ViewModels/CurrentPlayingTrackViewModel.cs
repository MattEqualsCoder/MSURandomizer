using System;
using Avalonia;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

public partial class CurrentPlayingTrackViewModel : ViewModelBase
{
    public const string NotConnectedMessage = "No song detected. Connect to one of the Snes Connectors.";

    [Reactive] public partial string Message { get; set; }

    [Reactive] public partial TimeSpan Duration { get; set; }

    [Reactive] public partial Thickness AnimationMargin { get; set; }

    public int Ticks = 0;
    public double MaxTicks = 0;
    public double TargetMargin = 0;

    public CurrentPlayingTrackViewModel()
    {
        Message = NotConnectedMessage;
    }
}