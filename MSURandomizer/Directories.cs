using System;
using System.IO;

namespace MSURandomizer;

public class Directories
{
    public static string AppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSURandomizer");
    public static string LogFolder => Path.Combine(AppDataFolder, "Logs");
#if DEBUG
    public static string LogPath => Path.Combine(LogFolder, "msu-randomizer-debug_.log");
#else
    public static string LogPath => Path.Combine(LogFolder, "msu-randomizer_.log");
#endif
}