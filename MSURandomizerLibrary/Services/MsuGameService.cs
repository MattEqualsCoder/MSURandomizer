using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.GameConnectors;
using MSURandomizerLibrary.Models;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Lua;

namespace MSURandomizerLibrary.Services;

internal class MsuGameService(ILogger<MsuGameService> logger, ISnesConnectorService snesConnectorService, MsuAppSettings appSettings, IMsuUserOptionsService userOptions)
    : IMsuGameService
{
    private IGameConnector? _currentGame;
    private readonly List<SnesRecurringMemoryRequest> _currentRequests = [];

    private readonly Dictionary<string, Type> _connectors = new()
    {
        { "A Link to the Past", typeof(LttPGameConnector) },
        { "Super Metroid / A Link to the Past Combination Randomizer", typeof(SMZ3GameConnector) }
    };

    public event TrackNumberChangedEventHandler? OnTrackChanged;

    public string LuaScriptFolder => appSettings.DefaultLuaDirectory;
    
    public void InstallLuaScripts(bool force = false)
    {
        if (!force && userOptions.MsuUserOptions.LuaScriptVersion == LuaScriptVersion.VersionNumber)
        {
            return;
        }

        snesConnectorService.CreateLuaScriptsFolder(LuaScriptFolder);
        userOptions.MsuUserOptions.LuaScriptVersion = LuaScriptVersion.VersionNumber;
        userOptions.Save();
    }

    public void SetMsuType(MsuType msuType)
    {
        foreach (var request in _currentRequests)
        {
            snesConnectorService.RemoveRecurringRequest(request);
        }

        if (_connectors.TryGetValue(msuType.Name, out var type))
        {
            _currentGame = Activator.CreateInstance(type) as IGameConnector;
        }
        else
        {
            return;
        }

        if (_currentGame == null)
        {
            return;
        }
        
        logger.LogInformation("Setup game connector");

        _currentGame.OnTrackChanged += (sender, args) =>
        {
            OnTrackChanged?.Invoke(sender, args);
            CurrentTrack = msuType.Tracks.FirstOrDefault(x => x.Number == args.TrackNumber);
        };

        foreach (var request in _currentGame.GetMemoryRequests())
        {
            _currentRequests.Add(snesConnectorService.AddRecurringRequest(request));
            snesConnectorService.AddRecurringRequest(request);
        }
        
    }

    public void Disconnect()
    {
        foreach (var request in _currentRequests)
        {
            snesConnectorService.RemoveRecurringRequest(request);
        }
        snesConnectorService.Disconnect();
    }

    public MsuTypeTrack? CurrentTrack { get; private set; }
    
    public bool IsMsuTypeCompatible(MsuType msuType) => _connectors.ContainsKey(msuType.Name);

    public void Dispose()
    {
        foreach (var request in _currentRequests)
        {
            snesConnectorService.RemoveRecurringRequest(request);
        }
        GC.SuppressFinalize(this);
    }
}