namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for copying a rom to a specified directory
/// </summary>
public interface IRomCopyService
{
    /// <summary>
    /// Copies a rom to the user specified directory
    /// </summary>
    /// <param name="romPath">The path to the rom</param>
    /// <param name="outPath">The path to the copied rom</param>
    /// <param name="error">The error from copying the folder</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool CopyRom(string romPath, out string? outPath, out string? error);
}