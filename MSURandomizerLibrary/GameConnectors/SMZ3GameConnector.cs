using MSURandomizerLibrary.Models;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SNI;

namespace MSURandomizerLibrary.GameConnectors;

internal class SMZ3GameConnector : IGameConnector
{
    public event TrackNumberChangedEventHandler? OnTrackChanged;

    private SMZ3Game CurrentGame = SMZ3Game.Neither;
    
    public List<SnesRecurringMemoryRequest> GetMemoryRequests()
    {
        return new List<SnesRecurringMemoryRequest>
        {
            // Check which game is being played
            new()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                Address = 0xA173FE,
                Length = 2,
                SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                AddressFormat = AddressFormat.Snes9x,
                RespondOnChangeOnly = true,
                OnResponse = data =>
                {
                    var value = data.ReadUInt8(0);
                    if (value == 0x00)
                    {
                        CurrentGame = SMZ3Game.Zelda;
                    }
                    else if (value == 0xFF)
                    {
                        CurrentGame = SMZ3Game.Metroid;
                    }
                    else if (value == 0x11)
                    {
                        CurrentGame = SMZ3Game.Credits;
                        OnTrackChanged?.Invoke(this, new TrackNumberChangedEventArgs(99));
                    }
                }
            },
            // Check the Zelda song
            new()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                Address = 0x7E010B,
                Length = 1,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                AddressFormat = AddressFormat.Snes9x,
                RespondOnChangeOnly = true,
                Filter = () => CurrentGame == SMZ3Game.Zelda,
                FrequencySeconds = 1,
                OnResponse = data =>
                {
                    var value = data.ReadUInt8(0);

                    if (value is <= 0 or > 200 or null)
                    {
                        return;
                    }
                    
                    OnTrackChanged?.Invoke(this, new TrackNumberChangedEventArgs(value.Value));
                }
            },
            // Check the Metroid song
            new()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                Address = 0x7E0332,
                Length = 1,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                AddressFormat = AddressFormat.Snes9x,
                RespondOnChangeOnly = true,
                Filter = () => CurrentGame is SMZ3Game.Metroid or SMZ3Game.Neither,
                FrequencySeconds = 1,
                OnResponse = data =>
                {
                    var value = data.ReadUInt8(0);

                    if (value is <= 0 or > 200 or null)
                    {
                        return;
                    }
                    
                    OnTrackChanged?.Invoke(this, new TrackNumberChangedEventArgs(value.Value));
                }
            }
        };
    }

    private enum SMZ3Game
    {
        Neither,
        Metroid,
        Zelda,
        Credits
    }
}