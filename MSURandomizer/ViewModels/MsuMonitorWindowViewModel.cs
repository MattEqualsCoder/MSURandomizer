using System;
using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using SnesConnectorLibrary;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(SnesConnectorSettings))]
public class MsuMonitorWindowViewModel : ViewModelBase
{
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsLuaFolderButtonVisible))]
    public SnesConnectorType ConnectorType { get; set; }
    
    public string Usb2SnesAddress { get; set; } = "";

    public string SniAddress { get; set; } = "";

    public string LuaAddress { get; set; } = "";
    
    public string ClientName { get; set; } = "MSURandomizer";
    
    [Reactive] public string ConnectionStatus { get; set; } = "Disconnected";

    [Reactive]
    [ReactiveLinkedProperties(nameof(HasErrorMessage))]
    public string? ErrorMessage { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(LastUpdateTimeText))]
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;

    public bool IsLuaFolderButtonVisible => ConnectorType == SnesConnectorType.Lua;

    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    public string LastUpdateTimeText => $"Last updated {DateTime.Now.ToShortTimeString()}";
    
    public SnesConnectorType CurrentConnectorType { get; set; }
    
    public Func<Enum, bool> FilterConnectorTypes => @enum =>
        (SnesConnectorType)@enum is SnesConnectorType.None or SnesConnectorType.Sni or SnesConnectorType.Usb2Snes or SnesConnectorType.Lua;
}