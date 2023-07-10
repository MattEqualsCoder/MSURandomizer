using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for retrieving the options which can be customized by the user
/// </summary>
public interface IMsuUserOptionsService
{
    /// <summary>
    /// Loads the user settings from the specified YAML file
    /// </summary>
    /// <param name="settingsFilePath">The path to the YAML file to load</param>
    /// <returns>The loaded MSU user options</returns>
    public MsuUserOptions Initialize(string settingsFilePath);

    /// <summary>
    /// Retrieve the previously loaded user options
    /// </summary>
    public MsuUserOptions MsuUserOptions { get; }

    /// <summary>
    /// Saves the user settings to the YAML file specified on initialization
    /// </summary>
    public void Save();
}