using MSURandomizerLibrary.MSUTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace MSURandomizerLibrary;

public static class MSURandomizerService
{
    private static List<MSUType> _msuTypes = new();
    private static List<MSU> _msus = new();
    
    public static ICollection<MSU>? GetMSUs(MSURandomizerOptions options, out string? error)
    {
        if (options.Directory == null)
        {
            error = "No MSU directory set";
            return null;
        }

        if (!Directory.Exists(options.Directory))
        {
            error = "Invalid MSU directory";
            return null;
        }
        
        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes() : _msuTypes;
        var directories = Directory.GetDirectories(options.Directory);
        var msuList = new List<MSU>();
        foreach (var directory in directories)
        {
            if (File.Exists(Path.Combine(directory, "msu-randomizer-output.txt"))) continue;
            var msus = Directory.EnumerateFiles(directory, "*.msu");
            foreach (var msuFile in msus)
            {
                var msu = LoadMSU(directory, msuFile, msuTypes);
                if (msu != null) msuList.Add(msu);
            }

        }

        error = null;
        _msus = msuList;
        return msuList;
    }

    public static List<MSUType> GetMSUTypes()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames();
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        var types = new List<MSUType>();

        foreach (var resource in resources.Where(x => x.EndsWith(".yaml") || x.EndsWith(".yml")))
        {
            using var resourceStream = assembly.GetManifestResourceStream(resource);
            if (resourceStream == null) continue;
            using var reader = new StreamReader(resourceStream);
            var msuType = deserializer.Deserialize<MSUType>(reader.ReadToEnd());
            types.Add(msuType);
        }

        _msuTypes = types;

        return types;
    }

    public static bool RandomizeMSU(MSURandomizerOptions options, out string? error)
    {
        var msus = _msus.Count == 0 ? GetMSUs(options, out _) : _msus;
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.Name) == true).ToList();
        if (msus == null || msus.Count == 0)
        {
            error = "No selected MSUs found";
            return false;
        }

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes() : _msuTypes;
        var type = msuTypes.FirstOrDefault(x => x.Name == options.OutputType);
        if (type == null)
        {
            error = "No valid MSU type selected";
            return false;
        }
        
        var pcmFiles = new Dictionary<int, List<string>>();

        var requiredRemappings = GetRemapDictionary(type.Remaps?.Where(x => !x.OnlyAddIfMissing));
        var optionalRemappings = GetRemapDictionary(type.Remaps?.Where(x => x.OnlyAddIfMissing));
        var conversionTypes = type.Conversions?.ToDictionary(x => x.MSUType, x => x.DefaultModifier) ??
                              new Dictionary<string, int>();
        var conversionRemappings =
            type.Conversions?.ToDictionary(x => x.MSUType, x => GetRemapDictionary(x.ManualRemaps)) ??
            new Dictionary<string, Dictionary<int, List<int>>>();

        // Loop through all MSUs to collect all of the PCMs for each 
        foreach (var msu in msus)
        {
            // If this MSU is an exact type match, copy over the PCM ids as-is or do remappings
            if (msu.Type != null && msu.Type.Name == type.Name)
            {
                foreach (var (id, path) in msu.PCMFiles)
                {
                    if (!pcmFiles.ContainsKey(id)) pcmFiles.Add(id, new List<string>());
                    pcmFiles[id].Add(path);

                    // If there are tracks that should be mapped because they are additional unused tracks
                    // For example, the extra boss themes in SMZ3
                    if (requiredRemappings.ContainsKey(id))
                    {
                        foreach (var otherId in requiredRemappings[id])
                        {
                            if (!pcmFiles.ContainsKey(otherId)) pcmFiles.Add(otherId, new List<string>());
                            pcmFiles[otherId].Add(path);    
                        }
                    }
                }

                // If there are extended tracks that are missing in this particular MSU
                // For example, this MSU does not have extended dungeon support
                foreach (var (originalId, mappingIds) in optionalRemappings)
                {
                    foreach (var otherId in mappingIds.Where(otherId => !msu.PCMFiles.ContainsKey(otherId)))
                    {
                        if (!pcmFiles.ContainsKey(otherId)) pcmFiles.Add(otherId, new List<string>());
                        pcmFiles[otherId].Add(msu.PCMFiles[originalId]);
                    }
                }

            }
            // If this MSU is in the accepted conversions, copy over PCMs that have found conversions
            else if (msu.Type != null && conversionTypes.ContainsKey(msu.TypeName))
            {
                var mappings = conversionRemappings[msu.TypeName];

                foreach (var (originalId, path) in msu.PCMFiles)
                {
                    if (mappings.ContainsKey(originalId))
                    {
                        foreach (var otherId in mappings[originalId])
                        {
                            if (!pcmFiles.ContainsKey(otherId)) pcmFiles.Add(otherId, new List<string>());
                            pcmFiles[otherId].Add(msu.PCMFiles[originalId]);
                        }
                    }
                    else
                    {
                        var otherId = originalId + conversionTypes[msu.TypeName];
                        if (!pcmFiles.ContainsKey(otherId)) pcmFiles.Add(otherId, new List<string>());
                        pcmFiles[otherId].Add(msu.PCMFiles[originalId]);
                    }
                }
            }
            // Else it's unsupported, so just copy the PCMs 1 to 1
            else
            {
                foreach (var (id, path) in msu.PCMFiles)
                {
                    if (!pcmFiles.ContainsKey(id)) pcmFiles.Add(id, new List<string>());
                    pcmFiles[id].Add(path);
                }
            }
        }
        
        // Recreate the folder
        var outputFolder = Path.Combine(options.Directory!, options.Name);
        if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, true);
        Directory.CreateDirectory(outputFolder);

        var minPcm = pcmFiles.Keys.Min();
        var maxPcm = pcmFiles.Keys.Max();
        var placedIndexes = new List<int>();
        var usedPaths = new List<string>();
        var random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        var pcmPath = Path.Combine(outputFolder, $"{options.Name}-<id>.pcm");
        var pickedPcms = new Dictionary<int, string>();

        for (var pcmIndex = minPcm; pcmIndex <= maxPcm; pcmIndex++)
        {
            if (placedIndexes.Contains(pcmIndex) || !pcmFiles.ContainsKey(pcmIndex)) continue;
            var path = pcmFiles[pcmIndex].Where(x => !options.AvoidDuplicates || !usedPaths.Contains(x))
                .Random(random) ?? pcmFiles[pcmIndex].First();
            CreateLink(pcmPath, path, pcmIndex);
            pickedPcms.Add(pcmIndex, path);
            placedIndexes.Add(pcmIndex);
            usedPaths.Add(path);

            // If there are any pairs of PCM files that should go together
            // For example, the intro songs for Super Metroid
            if (type.Pairs?.ContainsKey(pcmIndex) == true)
            {
                var newIndex = type.Pairs[pcmIndex];
                path = path.Replace($"-{pcmIndex}.pcm", $"-{newIndex}.pcm");
                if (!File.Exists(path)) continue;
                CreateLink(pcmPath, path, newIndex);
                pickedPcms.Add(newIndex, path);
                placedIndexes.Add(newIndex);
                usedPaths.Add(path);
            }
        }

        var msuPath = Path.Combine(outputFolder, $"{options.Name}.msu");
        
        using (File.Create(msuPath)) {}

        var output = pickedPcms
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}: {x.Value}");
        
        File.WriteAllLines(Path.Combine(outputFolder, "msu-randomizer-output.txt"), output);

        if (options.OpenFolderOnCreate)
        {
            Process.Start("explorer.exe", $"/select,\"{msuPath}\"");
        }

        options.CreatedMSUPath = msuPath;

        error = null;
        return true;
    }
    
    public static IEnumerable<MSU> ApplyFilter(IEnumerable<MSU> msus, MSURandomizerOptions options)
    {
        return msus.Where(x => MatchesFilter(x, options.OutputType, options.Filter));
    }
    
    private static MSU? LoadMSU(string directory, string msuFile, List<MSUType> msuTypes)
    {
        var msuName = Path.GetFileNameWithoutExtension(msuFile);
        var pcmFiles = Directory.EnumerateFiles(directory, $"{msuName}*.pcm");
        var validPcmFiles = new Dictionary<int, string>();
        foreach (var pcmFile in pcmFiles)
        {
            var pcmNumber = Path.GetFileNameWithoutExtension(pcmFile).Replace($"{msuName}-", "");
            if (!int.TryParse(pcmNumber, out var num)) continue;
            validPcmFiles.Add(num, pcmFile);
        }
        if (validPcmFiles.Count == 0) return null;
        var msuType = msuTypes.FirstOrDefault(x => x.Matches(validPcmFiles));
        return new MSU
            { Name = msuName, MSUPath = msuFile, PCMFiles = validPcmFiles, Type = msuType };
    }

    private static bool MatchesFilter(MSU msu, string? typeName, MSUFilter filter)
    {
        switch (filter)
        {
            case MSUFilter.All:
                return true;
            case MSUFilter.Exact:
                return msu.Type?.Name == typeName;
            default:
            {
                if (msu.Type?.Name == typeName) return true;
                var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes() : _msuTypes;
                var type = msuTypes.FirstOrDefault(x => x.Name == typeName);
                return (type?.Conversions?.Any(x => x.MSUType == msu.Type?.Name) == true);
            }
        }
    }
    
    private static void CreateLink(string pcmPath, string originalPath, int pcmIndex)
    {
        pcmPath = pcmPath.Replace("<id>", pcmIndex.ToString());
        NativeMethods.CreateHardLink(pcmPath, originalPath, IntPtr.Zero);
    }

    private static Dictionary<int, List<int>> GetRemapDictionary(IEnumerable<MSUTrackRemapping>? remappings)
    {
        var toReturn = new Dictionary<int, List<int>>();

        if (remappings == null) return toReturn;

        foreach (var remap in remappings)
        {
            foreach (var originalTrackNumber in remap.OriginalTrackNumbersList)
            {
                if (!toReturn.ContainsKey(originalTrackNumber)) toReturn.Add(originalTrackNumber, new List<int>());
                toReturn[originalTrackNumber].AddRange(remap.RemappedTrackNumbersList);
            }
        }

        return toReturn;
    }
}
