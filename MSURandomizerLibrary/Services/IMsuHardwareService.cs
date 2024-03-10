using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Interface for retrieving and booting MSUs on hardware
/// </summary>
public interface IMsuHardwareService
{
    /// <summary>
    /// Retrieves the MSUs found on the connected hardware
    /// </summary>
    /// <returns>A list of MSUs found</returns>
    public Task<List<Msu>?> GetMsusFromDevice();

    /// <summary>
    /// Uploads a rom to an MSU folder on the SNES
    /// </summary>
    /// <param name="msus">The MSU list to pick from. If more than is provided, a random one will be picked</param>
    /// <param name="romFilePath">The path to the rom to upload to the SNEs</param>
    /// <param name="bootRomAfter"></param>
    /// <returns>True if the MSU was successfully uploaded</returns>
    public Task<bool> UploadMsuRom(List<Msu> msus, string romFilePath, bool bootRomAfter);
}