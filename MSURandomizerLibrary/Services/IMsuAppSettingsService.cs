using System.IO;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for loading and retrieving the App Settings for the MSU Randomizer
/// </summary>
public interface IMsuAppSettingsService
{
    /// <summary>
    /// Initializes the MSU Randomizer App Settings
    /// </summary>
    /// <param name="stream">The stream to read the settings from</param>
    /// <returns>The MSU App Settings</returns>
    public MsuAppSettings Initialize(Stream? stream);
    
    /// <summary>
    /// Initializes the MSU Randomizer App Settings
    /// </summary>
    /// <param name="path">The path of the file to read the settings from</param>
    /// <returns>The MSU App Settings</returns>
    public MsuAppSettings Initialize(string path);

    /// <summary>
    /// The loaded App Settings
    /// </summary>
    public MsuAppSettings MsuAppSettings { get; }
}