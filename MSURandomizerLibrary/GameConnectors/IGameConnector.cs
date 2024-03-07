using MSURandomizerLibrary.Models;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;

namespace MSURandomizerLibrary.GameConnectors;

internal interface IGameConnector
{
    public event TrackNumberChangedEventHandler? OnTrackChanged;
    
    public List<SnesRecurringMemoryRequest> GetMemoryRequests();
}