using System.Diagnostics;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for launching a rom
/// </summary>
public interface IRomLauncherService
{
    /// <summary>
    /// Launches the requested rom
    /// </summary>
    /// <param name="romPath">The path to the rom file to launch</param>
    /// <param name="launchApplication">The application to launch the argument. If not specified, it'll use the OS default</param>
    /// <param name="launchArguments">Arguments to pass to the application when launching. {RomPath} will be replaced
    /// with the rom path. If {RomPath} is not added, then the rom path will be appended to the end.</param>
    public Process? LaunchRom(string romPath, string? launchApplication = null, string? launchArguments = null);
}