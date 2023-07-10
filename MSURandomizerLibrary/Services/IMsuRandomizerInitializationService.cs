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
    /// <param name="msuTypeFilePathOverride">Override for the location of the MSU types</param>
    public void Initialize(string randomizerSettingsPath, string msuTypeFilePathOverride = "");

    /// <summary>
    /// Initializes required MSU randomizer services
    /// </summary>
    /// <param name="randomizerSettingsStream">Stream to the MSU randomizer settings file</param>
    /// <param name="msuTypeFilePathOverride">Override for the location of the MSU types</param>
    public void Initialize(Stream randomizerSettingsStream, string msuTypeFilePathOverride = "");
}