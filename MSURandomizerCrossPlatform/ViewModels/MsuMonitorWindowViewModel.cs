using System;
using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using SnesConnectorLibrary;

namespace MSURandomizerCrossPlatform.ViewModels;

[MapsTo(typeof(SnesConnectorSettings))]
public class MsuMonitorWindowViewModel : ViewModelBase
{
    [Reactive] public SnesConnectorType ConnectorType { get; set; }
    
    [Reactive] public string ConnectionStatus { get; set; } = "Disconnected";

    [Reactive]
    [ReactiveLinkedProperties(nameof(HasErrorMessage))]
    public string? ErrorMessage { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(LastUpdateTimeText))]
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;

    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    public string LastUpdateTimeText => $"Last updated {DateTime.Now.ToShortTimeString()}";
}