using System;
using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;
using SnesConnectorLibrary;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(SnesConnectorSettings))]
public class SnesConnectorSelectionViewModel : ViewModelBase
{
    [Reactive] public SnesConnectorType ConnectorType { get; set; }
    
    [Reactive] public string ConnectionStatus { get; set; } = "Disconnected";
    
    [Reactive] public bool CanAccept { get; set; }
    
    [Reactive] public List<Msu>? Msus { get; set; }

    public Func<Enum, bool> FilterConnectorTypes => @enum =>
        (SnesConnectorType)@enum is SnesConnectorType.None or SnesConnectorType.Sni or SnesConnectorType.Usb2Snes;
}