using System.Collections.Generic;
using System.IO;
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
    /// The collection of MSU types previously loaded
    /// </summary>
    public IReadOnlyCollection<MsuType> MsuTypes { get; }
}