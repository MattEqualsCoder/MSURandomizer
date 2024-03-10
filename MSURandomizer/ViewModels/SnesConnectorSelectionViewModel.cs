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

    public Func<Enum, bool> FilterConnectorTypes => @enum =>
        (SnesConnectorType)@enum is SnesConnectorType.None or SnesConnectorType.Sni or SnesConnectorType.Usb2Snes;
}