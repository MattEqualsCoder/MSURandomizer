using Microsoft.Extensions.Logging;

namespace MSURandomizerLibrary.Services;

internal class RomCopyService(IMsuUserOptionsService options, ILogger<RomCopyService> logger) : IRomCopyService
{
    public bool CopyRom(string romPath, out string? outPath, out string? error)
    {
        outPath = null;
        error = null;
        
        if (!File.Exists(romPath))
        {
            error = $"{romPath} does not exist";
            return false;
        }
        
        if (string.IsNullOrEmpty(options.MsuUserOptions.CopyRomDirectory))
        {
            error = $"No Copy Rom Directory specified";
            return false;
        }

        if (!Directory.Exists(options.MsuUserOptions.CopyRomDirectory))
        {
            try
            {
                Directory.CreateDirectory(options.MsuUserOptions.CopyRomDirectory);
            }
            catch (Exception e)
            {
                error = $"Unable to create directory {options.MsuUserOptions.CopyRomDirectory}";
                logger.LogError(e, "Unable to create directory {Path}", options.MsuUserOptions.CopyRomDirectory);
                return false;
            }
        }
        
        var fileInfo = new FileInfo(romPath);

        var outputFolder = Path.Combine(options.MsuUserOptions.CopyRomDirectory,
            fileInfo.Name.Replace(fileInfo.Extension, ""));
        
        if (!Directory.Exists(outputFolder))
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (Exception e)
            {
                error = $"Unable to create directory {outputFolder}";
                logger.LogError(e, "Unable to create directory {Path}", outputFolder);
                return false;
            }
        }
        
        outPath = Path.Combine(outputFolder, fileInfo.Name);

        if (File.Exists(outPath))
        {
            return true;
        }
        
        try
        {
            File.Move(romPath, outPath);
            return true;
        }
        catch (Exception e)
        {
            error = $"Unable to move file to {outPath}";
            logger.LogError(e, "Unable to move file to {Path}", outPath);
            return false;
        }
    }
}