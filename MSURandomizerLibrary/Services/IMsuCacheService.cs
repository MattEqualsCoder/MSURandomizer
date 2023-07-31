using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for storing and retrieving loaded MSU data from a JSON cache file
/// </summary>
public interface IMsuCacheService
{
    /// <summary>
    /// Initializes the cache service so that MSUs can be added to the cache
    /// </summary>
    /// <param name="cachePath">The path to the cache directory or a JSON file to save to</param>
    public void Initialize(string cachePath);
    
    /// <summary>
    /// Gets an MSU from the cache
    /// </summary>
    /// <param name="msuPath">The path to the MSU file</param>
    /// <param name="yamlHash">A hash of the MSU details YAML file</param>
    /// <param name="pcmFiles">The list of all found PCM files for the MSU</param>
    /// <returns>The retrieved matching MSU, if one was found</returns>
    public Msu? Get(string msuPath, string yamlHash, ICollection<string> pcmFiles);

    /// <summary>
    /// Adds an MSU to the cache
    /// </summary>
    /// <param name="msu">The MSU to add to the cache</param>
    /// <param name="yamlHash">A hash of the MSU details YAML file</param>
    /// <param name="pcmFiles">The list of all PCM files for the MSU</param>
    /// <param name="saveCache">If the cache should be immediately saved after the MSU is added to it</param>
    public void Put(Msu msu, string yamlHash, ICollection<string> pcmFiles, bool saveCache);

    /// <summary>
    /// Saves the cache to the path specified in the Initialize method
    /// </summary>
    public void Save();
}