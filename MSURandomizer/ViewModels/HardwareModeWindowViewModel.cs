using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public partial class HardwareModeWindowViewModel : ViewModelBase
{
    [Reactive] public partial bool LaunchRom { get; set; }
    [Reactive] public partial bool OpenMonitorWindow { get; set; }
    [Reactive] public partial string ConnectionStatus { get; set; }
    [Reactive] public partial bool CanAccept { get; set; }
    [Reactive] public partial List<Msu>? Msus { get; set; }
    [Reactive] public partial SnesConnectorSelectionViewModel SnesConnectorSettings { get; set; }

    public HardwareModeWindowViewModel()
    {
        ConnectionStatus = "Disconnected";
        SnesConnectorSettings = new SnesConnectorSelectionViewModel();
    }
}