using System.IO;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for initializing required services from the app settings
/// </summary>
public interface IMsuRandomizerInitializationService
{
    /// <summary>
    /// Initializes required MSU randomizer services
    /// </summary>
    /// <param name="randomizerSettingsPath">File path to the MSU randomizer settings file</param>
    public void Initialize(string randomizerSettingsPath);
    
    /// <summary>
    /// Initializes required MSU randomizer services
    /// </summary>
    /// <param name="randomizerSettingsPath">Stream to the MSU randomizer settings file</param>
    public void Initialize(Stream randomizerSettingsStream);
}