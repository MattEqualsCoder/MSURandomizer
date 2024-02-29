using System;
using Avalonia;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class CurrentPlayingTrackViewModel : ViewModelBase
{
    public const string NotConnectedMessage = "No song detected. Connect to one of the Snes Connectors. No song detected. Connect to one of the Snes Connectors. No song detected. Connect to one of the Snes Connectors. ";

    [Reactive] public string Message { get; set; } = NotConnectedMessage;

    [Reactive] public TimeSpan Duration { get; set; }

    [Reactive] public Thickness AnimationMargin { get; set; }

    public int Ticks = 0;
    public double MaxTicks = 0;
    public double TargetMargin = 0;
}