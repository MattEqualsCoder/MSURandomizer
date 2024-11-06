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
    /// <returns>Returns the MSU if one was successfully picked and uploaded to</returns>
    public Task<Msu?> UploadMsuRom(List<Msu> msus, string romFilePath, bool bootRomAfter);

    /// <summary>
    /// Refreshes the MSU matching the given path
    /// </summary>
    /// <param name="path">The path of the MSU to refresh</param>
    /// <returns>The reloaded MSU</returns>
    public Msu RefreshMsu(string path);
    
    /// <summary>
    /// Gets the current cached MSUs
    /// </summary>
    /// <returns></returns>
    public List<Msu> Msus { get; }

    /// <summary>
    /// Event fired once the list of hardware MSUs has been updated
    /// </summary>
    public event EventHandler<MsuListEventArgs> HardwareMsusLoaded;
    
    /// <summary>
    /// Event fired once the list of hardware MSUs has been updated
    /// </summary>
    public event EventHandler<MsuListEventArgs> HardwareMsusChanged;

}