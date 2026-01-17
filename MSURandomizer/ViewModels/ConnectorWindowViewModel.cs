using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

[MapsTo(typeof(MsuUserOptions))]
public partial class ConnectorWindowViewModel : ViewModelBase
{
    [Reactive] public partial bool CanAccept { get; set; }
    [Reactive] public partial SnesConnectorSelectionViewModel SnesConnectorSettings { get; set; } = new();
    [Reactive] public partial string ConnectionStatus { get; set; } = "Select connector";
}