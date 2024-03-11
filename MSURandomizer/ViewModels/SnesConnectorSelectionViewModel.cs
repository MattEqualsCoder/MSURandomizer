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

    public string Usb2SnesAddress { get; set; } = "";

    public string SniAddress { get; set; } = "";

    public string LuaAddress { get; set; } = "";
    
    public string ClientName { get; set; } = "MSURandomizer";
    
    public Func<Enum, bool> FilterConnectorTypes => @enum =>
        (SnesConnectorType)@enum is SnesConnectorType.None or SnesConnectorType.Sni or SnesConnectorType.Usb2Snes;
}