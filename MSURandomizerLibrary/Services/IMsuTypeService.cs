using System.Collections.Generic;
using System.IO;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public interface IMsuTypeService
{
    /// <summary>
    /// Loads all MSU types from a directory
    /// </summary>
    /// <param name="directory">The directory to load from</param>
    public void LoadMsuTypesFromDirectory(string directory);
    
    /// <summary>
    /// Loads all MSU types from a stream for a file to parse
    /// </summary>
    /// <param name="stream">The stream to load from</param>
    public void LoadMsuTypesFromStream(Stream stream);

    /// <summary>
    /// The collection of MSU types previously loaded
    /// </summary>
    public IReadOnlyCollection<MsuType> MsuTypes { get; }
}