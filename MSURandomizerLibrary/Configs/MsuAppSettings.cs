namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Settings for the MSU Randomizer
/// </summary>
public class MsuAppSettings
{
    /// <summary>
    /// Overrides for the default names for MSU types
    /// </summary>
    public Dictionary<string, string>? MsuTypeNameOverrides { get; set; }
    
    /// <summary>
    /// The MSU type names that match SMZ3
    /// </summary>
    public List<string>? Smz3MsuTypes { get; set; }
    
    /// <summary>
    /// MSU types that have Super Metroid tracks starting at track 1
    /// </summary>
    public List<string>? MetroidFirstMsuTypes { get; set; }
    
    /// <summary>
    /// MSU types that have A Link to the Past tracks starting at track 1
    /// </summary>
    public List<string>? ZeldaFirstMsuTypes { get; set; }
    
    /// <summary>
    /// Path to load/save the user options YAML file from/to
    /// </summary>
    public string? UserOptionsFilePath { get; set; }
    
    /// <summary>
    /// Path to either the parent JSON config or a single JSON file with all of the MSU types
    /// </summary>
    public string? MsuTypeFilePath { get; set; }
    
    /// <summary>
    /// How frequently the continuous shuffle feature should operate
    /// </summary>
    public int? ContinuousReshuffleSeconds { get; set; }
    
    /// <summary>
    /// If the button to select a random MSU should be shown
    /// </summary>
    public bool? MsuWindowDisplayRandomButton { get; set; }
    
    /// <summary>
    /// If the button to shuffle the songs of the selected MSUs should be shown
    /// </summary>
    public bool? MsuWindowDisplayShuffleButton { get; set; }
    
    /// <summary>
    /// If the button to continuously shuffle the selected MSUs should be shown
    /// </summary>
    public bool? MsuWindowDisplayContinuousButton { get; set; }
    
    /// <summary>
    /// If the button to update the options should be shown
    /// </summary>
    public bool? MsuWindowDisplayOptionsButton { get; set; }
    
    /// <summary>
    /// If the button to save the selected MSUs and close the MSU list window should be shown
    /// </summary>
    public bool? MsuWindowDisplaySelectButton { get; set; }
    
    /// <summary>
    /// The name of an MSU type that should be forced to be used as output
    /// </summary>
    public string? ForcedMsuType { get; set; }
    
    /// <summary>
    /// Where the MSU lookup cache file will be stored
    /// </summary>
    public string? MsuCachePath { get; set; }
    
    /// <summary>
    /// The Windows title of the main MSU Window
    /// </summary>
    public string? MsuWindowTitle { get; set; }

    /// <summary>
    /// Disable opening the MSU Monitor window
    /// </summary>
    public bool? DisableMsuMonitorWindow { get; set; }

    /// <summary>
    /// If the user can select the option to launch a rom after generating one
    /// </summary>
    public bool CanLaunchRoms { get; set; } = true;
    
    /// <summary>
    /// If the hardware connection mode is disabled or not
    /// </summary>
    public bool DisableHardwareMode { get; set; }
    
    /// <summary>
    /// If the messenger that sends generation and track notifications via grpc
    /// </summary>
    public bool DisableMessageSender { get; set; }

    /// <summary>
    /// Default directory for misc save data
    /// </summary>
    public string SaveDataDirectory { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSURandomizer");

    /// <summary>
    /// Default directory for writing the currently playing song to
    /// </summary>
    public string? DefaultMsuCurrentSongOutputFilePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSURandomizer", "current_song.txt");
    
    /// <summary>
    /// Default directory for lua scripts to be added
    /// </summary>
    public string DefaultLuaDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSURandomizer", "Lua");
    
    /// <summary>
    /// Retrieves the MSU Type name if found, using any overrides if specified in the app settings
    /// </summary>
    /// <param name="msuName"></param>
    /// <returns></returns>
    public string GetMsuTypeName(string msuName)
    {
        return MsuTypeNameOverrides == null
            ? msuName
            : MsuTypeNameOverrides.GetValueOrDefault(msuName, msuName);
    }

    /// <summary>
    /// Retrieves any MSU type names related to SMZ3
    /// </summary>
    public IEnumerable<string> ZeldaSuperMetroidSmz3MsuTypes =>
        MetroidFirstMsuTypes != null && ZeldaFirstMsuTypes != null
            ? MetroidFirstMsuTypes.Concat(ZeldaFirstMsuTypes)
            : MetroidFirstMsuTypes ?? (ZeldaFirstMsuTypes ?? []);

}