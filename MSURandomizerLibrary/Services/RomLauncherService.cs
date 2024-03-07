using System.Diagnostics;

namespace MSURandomizerLibrary.Services;

internal class RomLauncherService(IMsuUserOptionsService msuUserOptionsService) : IRomLauncherService
{
    public Process? LaunchRom(string romPath, string? launchApplication = null, string? launchArguments = null)
    {
        if (!File.Exists(romPath))
        {
            throw new FileNotFoundException($"{romPath} not found");
        }

        launchApplication ??= msuUserOptionsService.MsuUserOptions.LaunchApplication;
        launchArguments ??= msuUserOptionsService.MsuUserOptions.LaunchArguments;
        
        if (string.IsNullOrEmpty(launchApplication))
        {
            launchApplication = romPath;
        }
        else
        {
            if (string.IsNullOrEmpty(launchArguments))
            {
                launchArguments = $"\"{romPath}\"";
            }
            else if (launchArguments.Contains("%rom%"))
            {
                launchArguments = launchArguments.Replace("%rom%", $"{romPath}");
            }
            else
            {
                launchArguments = $"{launchArguments} \"{romPath}\"";
            }
        }

        return Process.Start(new ProcessStartInfo
        {
            FileName = launchApplication,
            Arguments = launchArguments,
            UseShellExecute = true,
        });
    }
}