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
}