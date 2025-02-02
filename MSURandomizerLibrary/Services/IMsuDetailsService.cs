﻿using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for retrieving the MSU YAML details
/// </summary>
public interface IMsuDetailsService
{
    /// <summary>
    /// Loads the basic MSU details without track information
    /// </summary>
    /// <param name="msuPath">The path of the MSU to load the MSU details for</param>
    /// <param name="yamlHash">The hashed value of the YAML file to using for caching</param>
    /// <param name="error">Any errors with loading the basic MSU details</param>
    /// <returns>The MSU details if they were able to be loaded</returns>
    public MsuDetails? GetMsuDetails(string msuPath, out string yamlHash, out string? error);

    /// <summary>
    /// Converts the MSU details object into an MSU object
    /// </summary>
    /// <param name="msuDetails">The previously loaded MSU details object</param>
    /// <param name="msuType">The MSU type that is applicable for the MSU</param>
    /// <param name="msuPath">The path to the MSU file</param>
    /// <param name="msuDirectory">The directory of the MSU file</param>
    /// <param name="msuBaseName">The base name of the MSU file with no file extension</param>
    /// <param name="error">Any errors with loading the MSU details</param>
    /// <param name="ignoreAlts">If it should skip trying to detect alt tracks</param>
    /// <param name="pcmPaths">A list of found PCM files to check for instead of checking if the PCM file exists</param>
    /// <returns>The MSU with all of the updated details from the YAML file</returns>
    public Msu? ConvertToMsu(MsuDetails msuDetails, MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, out string? error, bool ignoreAlts = false, List<string>? pcmPaths = null);

    /// <summary>
    /// Saves the MSU details to a YAML file
    /// </summary>
    /// <param name="msu">The MSU to save the details for</param>
    /// <param name="outputPath">The path to save the YAML file to</param>
    /// <param name="error">Any errors with saving the MSU details</param>
    /// <returns>If the write was successful or not</returns>
    public bool SaveMsuDetails(Msu msu, string outputPath, out string? error);
    
    /// <summary>
    /// Retrieves the MSU Details given an MSU path, matching the folder and filename
    /// </summary>
    /// <param name="msuPath">The path to the MSU file</param>
    /// <param name="msuType">The desired MSU type, if applicable</param>
    /// <returns>The MSU details if they are found</returns>
    public MsuDetails? GetMsuDetailsForPath(string msuPath, MsuType? msuType);
}