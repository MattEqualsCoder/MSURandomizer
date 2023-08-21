using System.IO;

namespace MSURandomizerLibrary.Models;

public class MsuRandomizerInitializationRequest
{
    /// <summary>
    /// Stream to load the MSU Randomizer App Settings from. If unspecified, it will fall back to the MsuAppSettingsStream
    /// </summary>
    public Stream? MsuAppSettingsStream { get; set; }
    
    /// <summary>
    /// File path to load the MSU Randomizer App Settings from if MsuAppSettingsStream is not specified
    /// </summary>
    public string? MsuAppSettingsPath { get; set; }
    
    /// <summary>
    /// Stream to load with an MSU type config file. If unspecified, it will fall back to the MsuTypeConfigPath
    /// </summary>
    public Stream? MsuTypeConfigStream { get; set; }
    
    /// <summary>
    /// File path to the parent directory to load the MSU type config files from. If unspecified, it will back back to
    /// the value specified in the App Settings
    /// </summary>
    public string? MsuTypeConfigPath { get; set; }
    
    /// <summary>
    /// Where the MSU lookup cache file will be stored
    /// </summary>
    public string? MsuCachePath { get; set; }
    
    /// <summary>
    /// File path to load the user options from. If unspecified, it will fall back to the value specified in the App
    /// Settings
    /// </summary>
    public string? UserOptionsPath { get; set; }

    /// <summary>
    /// If the MSUs should be automatically looked up on initialization
    /// </summary>
    public bool LookupMsus { get; set; } = true;
}