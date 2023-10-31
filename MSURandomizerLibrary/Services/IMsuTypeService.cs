using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for loading MSU types from JSON configs
/// </summary>
public interface IMsuTypeService
{
    /// <summary>
    /// Loads all MSU types from a directory
    /// </summary>
    /// <param name="directory">The directory to load from</param>
    public void LoadMsuTypes(string directory);
    
    /// <summary>
    /// Loads all MSU types from a stream for a file to parse
    /// </summary>
    /// <param name="stream">The stream to load from</param>
    public void LoadMsuTypes(Stream stream);

    /// <summary>
    /// Loads all MSU types from internal msu_types json
    /// </summary>
    public void LoadMsuTypes();

    /// <summary>
    /// Gets a MSU type matching a name
    /// </summary>
    /// <param name="name">The name of the MSU type to lookup</param>
    /// <returns>The MSU type</returns>
    public MsuType? GetMsuType(string? name);
    
    /// <summary>
    /// Gets the name of an MSU type
    /// </summary>
    /// <param name="msuType">The MSU type to get the name of</param>
    /// <returns>The name of the MSU type</returns>
    public string GetMsuTypeName(MsuType? msuType);

    /// <summary>
    /// Returns the MSU type matching the new SMZ3 MSU patch (Zelda first)
    /// </summary>
    /// <returns>The MSU type, if it was found</returns>
    public MsuType? GetSMZ3MsuType();

    /// <summary>
    /// Returns the MSU type matching the original SMZ3 MSU patch (Metroid first)
    /// </summary>
    /// <returns>The MSU type, if it was found</returns>
    public MsuType? GetSMZ3LegacyMSUType();

    /// <summary>
    /// The collection of MSU types previously loaded
    /// </summary>
    public IReadOnlyCollection<MsuType> MsuTypes { get; }
}