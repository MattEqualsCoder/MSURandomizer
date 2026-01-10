using System;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using SnesConnectorLibrary;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(SnesConnectorSettings))]
public partial class SnesConnectorSelectionViewModel : ViewModelBase
{
    [Reactive] public partial SnesConnectorType ConnectorType { get; set; }

    public string Usb2SnesAddress { get; set; } = "";

    public string SniAddress { get; set; } = "";

    public string LuaAddress { get; set; } = "";
    
    public string ClientName { get; set; } = "MSURandomizer";
    
    public Func<Enum, bool> FilterConnectorTypes => @enum =>
        (SnesConnectorType)@enum is SnesConnectorType.None or SnesConnectorType.Sni or SnesConnectorType.Usb2Snes;
}