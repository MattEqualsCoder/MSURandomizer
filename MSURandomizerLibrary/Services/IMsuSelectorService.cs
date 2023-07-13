using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for selecting, converting, and creating generated MSUs
/// </summary>
public interface IMsuSelectorService
{
    /// <summary>
    /// Copies an MSU to a path as a given MSU type.
    /// <br></br>
    /// <para>Accepts the following request properties:</para>
    /// <list type="bullet">
    /// <item>Msu/MsuPath - The MSU or path of the MSU to assign</item>
    /// <item>OutputMsuType/OutputMsuTypeName - The MSU type or name of the MSU type to assign the MSU to</item>
    /// <item>OutputPath - Where to save the MSU to</item>
    /// <item>EmptyFolder - If the output directory should be emptied of MSU related files</item>
    /// <item>OpenFolder - If the output directory should be opened in Windows Explorer</item>
    /// <item>PrevMsu - The previous MSU for pulling the last tracks from if a track can't be saved</item>
    /// </list>
    /// </summary>
    /// <returns>The response object with the saved MSU if successful</returns>
    public MsuSelectorResponse AssignMsu(MsuSelectorRequest request);

    /// <summary>
    /// Picks a random MSU from a list of provided MSUs
    /// <br></br>
    /// <para>Accepts the following request properties:</para>
    /// <list type="bullet">
    /// <item>Msus/MsuPaths - The list of MSUs or paths of the MSUs to pick between</item>
    /// <item>OutputMsuType/OutputMsuTypeName - The MSU type or name of the MSU type to be generated</item>
    /// <item>OutputPath - Where to save the MSU to</item>
    /// <item>EmptyFolder - If the output directory should be emptied of MSU related files</item>
    /// <item>OpenFolder - If the output directory should be opened in Windows Explorer</item>
    /// <item>PrevMsu - The previous MSU for pulling the last tracks from if a track can't be saved</item>
    /// </list>
    /// </summary>
    /// <returns>The response object with the saved MSU if successful</returns>
    public MsuSelectorResponse PickRandomMsu(MsuSelectorRequest request);

    /// <summary>
    /// Creates a shuffled MSU from a list of provided MSUs
    /// <br></br>
    /// <para>Accepts the following request properties:</para>
    /// <list type="bullet">
    /// <item>Msus/MsuPaths - The list of MSUs or paths of the MSUs to pick between</item>
    /// <item>OutputMsuType/OutputMsuTypeName - The MSU type or name of the MSU type to be generated</item>
    /// <item>OutputPath - Where to save the MSU to</item>
    /// <item>EmptyFolder - If the output directory should be emptied of MSU related files</item>
    /// <item>OpenFolder - If the output directory should be opened in Windows Explorer</item>
    /// <item>PrevMsu - The previous MSU for pulling the last tracks from if a track can't be saved</item>
    /// </list>
    /// </summary>
    /// <returns>The response object with the saved MSU if successful</returns>
    public MsuSelectorResponse CreateShuffledMsu(MsuSelectorRequest request);

    /// <summary>
    /// Converts an MSU to a given MSU type
    /// <br></br>
    /// <para>Accepts the following request properties:</para>
    /// <list type="bullet">
    /// <item>Msu/MsuPath - The MSU or path of the MSU to convert</item>
    /// <item>OutputMsuType/OutputMsuTypeName - The MSU type or name of the MSU type to convert the MSU to</item>
    /// </list>
    /// </summary>
    /// <returns>The response object with the converted MSU if successful</returns>
    public MsuSelectorResponse ConvertMsu(MsuSelectorRequest request);
    
    /// <summary>
    /// Converts a series of MSUs to a given MSU type
    /// <para>Accepts the following request properties:</para>
    /// <list type="bullet">
    /// <item>Msus/MsuPaths - The list of MSUs or paths of the MSUs to convert</item>
    /// <item>OutputMsuType/OutputMsuTypeName - The MSU type or name of the MSU type to convert the MSUs to</item>
    /// </list>
    /// </summary>
    /// <returns>The response object with the converted MSUs if successful</returns>
    public MsuSelectorResponse ConvertMsus(MsuSelectorRequest request);

    /// <summary>
    /// Saves an MSU to a given location
    /// <br></br>
    /// <para>Accepts the following request properties:</para>
    /// <list type="bullet">
    /// <item>Msu/MsuPath - The MSU or path of the MSU to assign</item>
    /// <item>OutputPath - Where to save the MSU to</item>
    /// <item>EmptyFolder - If the output directory should be emptied of MSU related files</item>
    /// <item>OpenFolder - If the output directory should be opened in Windows Explorer</item>
    /// <item>PrevMsu - The previous MSU for pulling the last tracks from if a track can't be saved</item>
    /// </list>
    /// </summary>
    /// <returns>The response object with the saved MSU if successful</returns>
    public MsuSelectorResponse SaveMsu(MsuSelectorRequest request);
}