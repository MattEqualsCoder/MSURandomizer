using SnesConnectorLibrary;
using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Class with all options for MSU generation
/// </summary>
public class MsuUserOptions
{
    /// <summary>
    /// Default directory to look for MSUs
    /// </summary>
    public string? DefaultMsuPath { get; set; }
    
    /// <summary>
    /// The output MSU name if not applying to a rom
    /// </summary>
    public string Name { get; set; } = "RandomizedMSU";
    
    /// <summary>
    /// The name of the MSU type to export as
    /// </summary>
    public string? OutputMsuType { get; set; }
    
    /// <summary>
    /// The filter to apply to MSU types
    /// </summary>
    public MsuFilter Filter { get; set; } = MsuFilter.Compatible;
    
    /// <summary>
    /// If duplicate songs should be avoided
    /// </summary>
    public bool AvoidDuplicates { get; set; }
    
    /// <summary>
    /// If only copyright safe tracks should be used
    /// </summary>
    public MsuCopyrightSafety MsuCopyrightSafety { get; set; }
    
    /// <summary>
    /// If the folder should be opened after generating an MSU
    /// </summary>
    public bool OpenFolderOnCreate { get; set; }
    
    /// <summary>
    /// The selected MSUs
    /// </summary>
    public ICollection<string>? SelectedMsus { get; set; }
    
    /// <summary>
    /// The rom to create the MSU for
    /// </summary>
    public string? OutputRomPath { get; set; }
    
    /// <summary>
    /// The folder to generate the MSU under
    /// </summary>
    public string? OutputFolderPath { get; set; }
    
    /// <summary>
    /// If the latest release should be checked on launch (MSU Randomizer app only)
    /// </summary>
    public bool PromptOnUpdate { get; set; } = true;
    
    /// <summary>
    /// If pre-releases should also be checked against (MSU Randomizer app only)
    /// </summary>
    public bool PromptOnPreRelease { get; set; }
    
    /// <summary>
    /// Specific directories to load for specific MSU types 
    /// </summary>
    public Dictionary<string, string> MsuTypeNamePaths { get; set;  } = new();
    
    /// <summary>
    /// User settings for different MSUs
    /// </summary>
    public ICollection<MsuSettings> MsuSettings { get; set; } = new List<MsuSettings>();

    /// <summary>
    /// Display format for the current playing track info
    /// </summary>
    public TrackDisplayFormat TrackDisplayFormat { get; set; } = TrackDisplayFormat.Vertical;
    
    /// <summary>
    /// Path to write the currently playing song to
    /// </summary>
    public string? MsuCurrentSongOutputFilePath { get; set; }
    
    /// <summary>
    /// How the MSU should be generated
    /// </summary>
    public MsuRandomizationStyle RandomizationStyle { get; set; }
    
    /// <summary>
    /// How multiple MSUs should be shuffled
    /// </summary>
    public MsuShuffleStyle MsuShuffleStyle { get; set; }

    /// <summary>
    /// Settings for connecting to the SNES
    /// </summary>
    public SnesConnectorSettings SnesConnectorSettings { get; set; } = new();

    /// <summary>
    /// If the monitor window should be opened 
    /// </summary>
    public bool OpenMonitorWindow { get; set; } = true;
    
    /// <summary>
    /// The version of the installed Lua scripts
    /// </summary>
    public int? LuaScriptVersion { get; set; }

    /// <summary>
    /// The scale factor for the UI
    /// </summary>
    public double UiScaling { get; set; } = 1;
    
    /// <summary>
    /// If the user passed in the application args
    /// </summary>
    public bool PassedRomArgument { get; set; }
    
    /// <summary>
    /// Application to launch the rom after generating an MSU
    /// </summary>
    public string? LaunchApplication { get; set; }
    
    /// <summary>
    /// Arguments for launching the rom
    /// </summary>
    public string? LaunchArguments { get; set; }
    
    /// <summary>
    /// If the rom should be launched after generating an MSU
    /// </summary>
    public bool LaunchRom { get; set; }
    
    /// <summary>
    /// Directory roms should be copied to before randomizing
    /// </summary>
    public string? CopyRomDirectory { get; set; }

    /// <summary>
    /// Dictionary of all MSU directories and the MSU type associated with them
    /// </summary>
    public Dictionary<string, string> MsuDirectories { get; set; } = new();
    
    /// <summary>
    /// Specific directories to load for specific MSU types
    /// </summary>
    [YamlIgnore] public Dictionary<MsuType, string> MsuTypePaths { get; set; } = new();
    
    /// <summary>
    /// Gets the MSU Settings for a specific MSU path
    /// </summary>
    /// <param name="path">The path to the MSU file</param>
    /// <returns>The MSU settings for that MSU</returns>
    public MsuSettings GetMsuSettings(string path)
    {
        return MsuSettings.FirstOrDefault(x => x.MsuPath == path) ?? new MsuSettings(path);
    }

    /// <summary>
    /// Returns if  at least one MSU directory has been set
    /// </summary>
    /// <returns></returns>
    public bool HasMsuFolder()
    {
        return MsuDirectories.Count > 0;
    }
}
