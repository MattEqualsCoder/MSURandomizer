using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public class HardwareModeWindowViewModel : ViewModelBase
{
    [Reactive] public bool LaunchRom { get; set; }
    [Reactive] public bool OpenMonitorWindow { get; set; }
    [Reactive] public string ConnectionStatus { get; set; } = "Disconnected";
    [Reactive] public bool CanAccept { get; set; }
    [Reactive] public List<Msu>? Msus { get; set; }
    [Reactive] public SnesConnectorSelectionViewModel SnesConnectorSettings { get; set; } = new();
}