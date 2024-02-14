using MSURandomizerLibrary.Models;
using SnesConnectorLibrary;

namespace MSURandomizerLibrary.GameConnectors;

internal interface IGameConnector
{
    public event TrackNumberChangedEventHandler? OnTrackChanged;
    
    public List<SnesRecurringMemoryRequest> GetMemoryRequests();
}