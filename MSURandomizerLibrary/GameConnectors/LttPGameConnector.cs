using MSURandomizerLibrary.Models;
using SnesConnectorLibrary;
using SNI;

namespace MSURandomizerLibrary.GameConnectors;

internal class LttPGameConnector : IGameConnector
{
    public event TrackNumberChangedEventHandler? OnTrackChanged;
    
    public List<SnesRecurringMemoryRequest> GetMemoryRequests()
    {
        return new List<SnesRecurringMemoryRequest>
        {
            new()
            {
                Address = 0x7E010B,
                Length = 1,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                SniMemoryMapping = MemoryMapping.LoRom,
                AddressFormat = AddressFormat.Snes9x,
                RespondOnChangeOnly = true,
                FrequencySeconds = 1,
                OnResponse = (data =>
                {
                    OnTrackChanged?.Invoke(this, new TrackNumberChangedEventArgs(data.ReadUInt8(0) ?? 0));
                })
            }
        };
    }
}