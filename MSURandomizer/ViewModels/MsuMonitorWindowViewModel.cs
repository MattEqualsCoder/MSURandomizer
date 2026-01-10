using System;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using SnesConnectorLibrary;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(SnesConnectorSettings))]
public partial class MsuMonitorWindowViewModel : ViewModelBase
{
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsLuaFolderButtonVisible))]
    public partial SnesConnectorType ConnectorType { get; set; }
    
    public string Usb2SnesAddress { get; set; } = "";

    public string SniAddress { get; set; } = "";

    public string LuaAddress { get; set; } = "";
    
    public string ClientName { get; set; } = "MSURandomizer";
    
    [Reactive] public partial bool ShowConnectorDropdown { get; set; }
    
    [Reactive] public partial string ConnectionStatus { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(HasErrorMessage))]
    public partial string? ErrorMessage { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(LastUpdateTimeText))]
    public partial DateTime LastUpdateTime { get; set; }

    public bool IsLuaFolderButtonVisible => ConnectorType == SnesConnectorType.Lua;

    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    public string LastUpdateTimeText => $"Last updated {DateTime.Now.ToShortTimeString()}";
    
    public SnesConnectorType CurrentConnectorType { get; set; }
    
    public Func<Enum, bool> FilterConnectorTypes => @enum =>
        (SnesConnectorType)@enum is SnesConnectorType.None or SnesConnectorType.Sni or SnesConnectorType.Usb2Snes or SnesConnectorType.Lua;

    public MsuMonitorWindowViewModel()
    {
        ConnectionStatus = "Disconnected";
        LastUpdateTime = DateTime.Now;
    }
}