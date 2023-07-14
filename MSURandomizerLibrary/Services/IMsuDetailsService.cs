using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for retrieving the MSU YAML details
/// </summary>
public interface IMsuDetailsService
{
    /// <summary>
    /// Loads the MSU Details from a YAML file
    /// </summary>
    /// <param name="msuType">The MSU type that is applicable for the MSU</param>
    /// <param name="msuPath">The path to the MSU file</param>
    /// <param name="msuDirectory">The directory of the MSU file</param>
    /// <param name="msuBaseName">The base name of the MSU file with no file extension</param>
    /// <param name="yamlPath">The path to the YAML file</param>
    /// <returns>The MSU with all of the updated details from the YAML file</returns>
    public Msu? LoadMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlPath);
    
    /// <summary>
    /// Saves the MSU details to a YAML file
    /// </summary>
    /// <param name="msu">The MSU to save the details for</param>
    /// <param name="outputPath">The path to save the YAML file to</param>
    /// <returns>If the write was successful or not</returns>
    public bool SaveMsuDetails(Msu msu, string outputPath);
}