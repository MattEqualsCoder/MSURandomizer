using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for selecting MSUs
/// </summary>
public interface IMsuSelectorService
{
    /// <summary>
    /// Copies an MSU to a path
    /// </summary>
    /// <param name="msu">The MSU to save</param>
    /// <param name="msuType">The type to save the MSU as</param>
    /// <param name="outputPath">The path to save the MSU to</param>
    /// <returns>The saved MSU</returns>
    public Msu AssignMsu(Msu msu, MsuType msuType, string outputPath);

    /// <summary>
    /// Picks a random MSU based on the user options
    /// </summary>
    /// <param name="emptyFolder">If all MSU related files should be deleted from the destination first</param>
    /// <returns>The saved MSU</returns>
    public Msu PickRandomMsu(bool emptyFolder = true);
    
    /// <summary>
    /// Picks a random MSU from a list of provided MSUs
    /// </summary>
    /// <param name="msuPaths">The list of paths of the MSUs to pick from</param>
    /// <param name="msuTypeName">The name of the MSU type to save the generated MSU as</param>
    /// <param name="outputPath">The path to save the MSU to</param>
    /// <param name="emptyFolder">If all MSU related files should be deleted from the destination first</param>
    /// <param name="openFolder">If the folder should be opened in file explorer</param>
    /// <returns>The saved MSU</returns>
    public Msu PickRandomMsu(ICollection<string> msuPaths, string msuTypeName, string outputPath, bool emptyFolder = true, bool openFolder = false);

    /// <summary>
    /// Picks a random MSU from a list of provided MSUs
    /// </summary>
    /// <param name="msus">The list of MSUs to pick from</param>
    /// <param name="msuType">The type to save the generated MSU as</param>
    /// <param name="outputPath">The path to save the MSU to</param>
    /// <param name="emptyFolder">If all MSU related files should be deleted from the destination first</param>
    /// <param name="openFolder">If the folder should be opened in file explorer</param>
    /// <returns>The saved MSU</returns>
    public Msu PickRandomMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, bool emptyFolder = true, bool openFolder = false);

    /// <summary>
    /// Creates a shuffled MSU based on the user options
    /// </summary>
    /// <param name="prevMsu">The previous generated MSU for to pull locked track information from</param>
    /// <param name="emptyFolder">If all MSU related files should be deleted from the destination first</param>
    /// <returns>The saved MSU</returns>
    public Msu CreateShuffledMsu(Msu? prevMsu = null, bool emptyFolder = true);
    
    /// <summary>
    /// Creates a shuffled MSU from a list of provided MSUs
    /// </summary>
    /// <param name="msuPaths">The list of paths of the MSUs to pick from</param>
    /// <param name="msuTypeName">The name of the MSU type to save the generated MSU as</param>
    /// <param name="outputPath">The path to save the MSU to</param>
    /// <param name="prevMsu">The previous generated MSU for to pull locked track information from</param>
    /// <param name="emptyFolder">If all MSU related files should be deleted from the destination first</param>
    /// <param name="avoidDuplicates">Avoid picking the same track multiple times, if possible</param>
    /// <param name="openFolder">If the folder should be opened in file explorer</param>
    /// <returns>The saved MSU</returns>
    public Msu CreateShuffledMsu(ICollection<string> msuPaths, string msuTypeName, string outputPath, Msu? prevMsu = null, bool emptyFolder = true, bool avoidDuplicates = true, bool openFolder = false);
    
    /// <summary>
    /// Creates a shuffled MSU from a list of provided MSUs
    /// </summary>
    /// <param name="msus">The list of MSUs to pick from</param>
    /// <param name="msuType">The type to save the generated MSU as</param>
    /// <param name="outputPath">The path to save the MSU to</param>
    /// <param name="prevMsu">The previous generated MSU for to pull locked track information from</param>
    /// <param name="emptyFolder">If all MSU related files should be deleted from the destination first</param>
    /// <param name="avoidDuplicates">Avoid picking the same track multiple times, if possible</param>
    /// <param name="openFolder">If the folder should be opened in file explorer</param>
    /// <returns>The saved MSU</returns>
    public Msu CreateShuffledMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, Msu? prevMsu = null, bool emptyFolder = true, bool avoidDuplicates = true, bool openFolder = false);

    /// <summary>
    /// Converts an MSU to a given MSU type
    /// </summary>
    /// <param name="msu">The MSU to convert</param>
    /// <param name="msuType">The type to convert the MSU to</param>
    /// <returns>The converted MSU</returns>
    public Msu ConvertMsu(Msu msu, MsuType msuType);

    /// <summary>
    /// Saves an MSU to a given location
    /// </summary>
    /// <param name="msu">The MSU to save</param>
    /// <param name="outputPath">The path to save the MSU to</param>
    /// <param name="prevMsu">The previous generated MSU for to pull locked track information from</param>
    /// <returns>The saved MSU</returns>
    public Msu SaveMsu(Msu msu, string outputPath, Msu? prevMsu = null);

    /// <summary>
    /// Converts a series of MSUs to a given MSU type
    /// </summary>
    /// <param name="msus">The series of MSUs to convert</param>
    /// <param name="msuType">The type to convert the MSUs to</param>
    /// <returns>The collection of converted MSUs</returns>
    public ICollection<Msu> ConvertMsus(ICollection<Msu> msus, MsuType msuType);
}