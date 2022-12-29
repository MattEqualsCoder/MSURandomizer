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

/// <summary>
/// Service for retrieving available MSUs and generating random MSUs
/// </summary>
public static class MSURandomizerService
{
    private static List<MSUType> _msuTypes = new();
    private static List<MSU> _msus = new();
    
    /// <summary>
    /// Gets a list of all available MSUs found in the MSU directory
    /// </summary>
    /// <param name="options">MSURandomizerOptions with input directory information</param>
    /// <param name="error">An error message if getting the MSUs was unsuccessful</param>
    /// <returns>A collection of all found MSUs</returns>
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

    /// <summary>
    /// Gets a list of all identified MSU types
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Generates an MSU that has tracks randomly shuffled from the selected MSUs
    /// </summary>
    /// <param name="options">Options with all of the selected MSUs and output options</param>
    /// <param name="error">Error message to display to the user</param>
    /// <returns>True if generated successfully, false otherwise</returns>
    public static bool ShuffleMSU(MSURandomizerOptions options, out string? error)
    {
        error = ValidateOptions(options);
        if (!string.IsNullOrEmpty(error)) return false;
        
        var msus = _msus.Count == 0 ? GetMSUs(options, out _) : _msus;
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.Name) == true).ToList();

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes() : _msuTypes;
        var type = msuTypes.FirstOrDefault(x => x.Name == options.OutputType);
        
        var outputData = GetOutputData(options);
        var outputFolder = outputData.folder;
        var outputFileName = outputData.fileName;
        
        var pcmFiles = new Dictionary<int, List<string>>();

        var requiredRemappings = GetRemapDictionary(type!.Remaps?.Where(x => !x.OnlyAddIfMissing));
        var optionalRemappings = GetRemapDictionary(type.Remaps?.Where(x => x.OnlyAddIfMissing));
        var conversionTypes = type.Conversions?.ToDictionary(x => x.MSUType, x => x) ??
                              new Dictionary<string, MSUConversion>();
        var conversionRemappings =
            type.Conversions?.ToDictionary(x => x.MSUType, x => GetRemapDictionary(x.ManualRemaps)) ??
            new Dictionary<string, Dictionary<int, List<int>>>();

        // Loop through all MSUs to collect all of the PCMs for each 
        foreach (var msu in msus!)
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
                var conversion = conversionTypes[msu.TypeName];
                var mappings = conversionRemappings[msu.TypeName];

                foreach (var (originalId, path) in msu.PCMFiles)
                {
                    if (originalId < conversion.MinimumTrackNumber ||
                        originalId > conversion.MaximumTrackNumber) continue;
                    
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
                        var otherId = originalId + conversion.DefaultModifier;
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
        
        var minPcm = pcmFiles.Keys.Min();
        var maxPcm = pcmFiles.Keys.Max();
        var placedIndexes = new List<int>();
        var usedPaths = new List<string>();
        var random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        var pcmPath = Path.Combine(outputFolder, $"{outputFileName}-<id>.pcm");
        var pickedPcms = new Dictionary<int, string>();

        for (var pcmIndex = minPcm; pcmIndex <= maxPcm; pcmIndex++)
        {
            if (placedIndexes.Contains(pcmIndex) || !pcmFiles.ContainsKey(pcmIndex)) continue;
            var path = pcmFiles[pcmIndex].Where(x => !options.AvoidDuplicates || !usedPaths.Contains(x))
                .Random(random) ?? pcmFiles[pcmIndex].First();
            CreatePCMFile(pcmPath, path, pcmIndex);
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
                CreatePCMFile(pcmPath, path, newIndex);
                pickedPcms.Add(newIndex, path);
                placedIndexes.Add(newIndex);
                usedPaths.Add(path);
            }
        }

        var msuPath = Path.Combine(outputFolder, $"{outputFileName}.msu");
        
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

    /// <summary>
    /// Picks a random MSU from the list of selected MSUs
    /// </summary>
    /// <param name="options">Options with all of the selected MSUs and output options</param>
    /// <param name="error">Error message to display to the user</param>
    /// <returns>True if generated successfully, false otherwise</returns>
    public static bool PickRandomMSU(MSURandomizerOptions options, out string? error)
    {
        error = ValidateOptions(options);
        if (!string.IsNullOrEmpty(error)) return false;

        var msus = _msus.Count == 0 ? GetMSUs(options, out _) : _msus;
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.Name) == true).ToList();

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes() : _msuTypes;
        var type = msuTypes.FirstOrDefault(x => x.Name == options.OutputType);

        var outputData = GetOutputData(options);
        var outputFolder = outputData.folder;
        var outputFileName = outputData.fileName;
        
        var conversionTypes = type!.Conversions?.ToDictionary(x => x.MSUType, x => x) ??
                              new Dictionary<string, MSUConversion>();
        var conversionRemappings =
            type.Conversions?.ToDictionary(x => x.MSUType, x => GetRemapDictionary(x.ManualRemaps)) ??
            new Dictionary<string, Dictionary<int, List<int>>>();
        var msuOptions = new List<Dictionary<int, string>>();
        
        // Loop through all MSUs to collect all of the PCMs for each 
        foreach (var msu in msus!)
        {
            var msuPcms = new Dictionary<int, string>();
            
            // If this MSU is an exact type match, copy over the PCM ids as-is or do remappings
            if (msu.Type != null && msu.Type.Name == type.Name)
            {
                msuPcms = msu.PCMFiles;
            }
            // If this MSU is in the accepted conversions, copy over PCMs that have found conversions
            else if (msu.Type != null && conversionTypes.ContainsKey(msu.TypeName))
            {
                var conversion = conversionTypes[msu.TypeName];
                var mappings = conversionRemappings[msu.TypeName];

                foreach (var (originalId, path) in msu.PCMFiles)
                {
                    if (originalId < conversion.MinimumTrackNumber ||
                        originalId > conversion.MaximumTrackNumber) continue;
                    
                    if (mappings.ContainsKey(originalId))
                    {
                        foreach (var otherId in mappings[originalId])
                        {
                            msuPcms[otherId] = path;
                        }
                    }
                    else
                    {
                        var otherId = originalId + conversion.DefaultModifier;
                        msuPcms[otherId] = path;
                    }
                }
            }
            // Else it's unsupported, so just copy the PCMs 1 to 1
            else
            {
                foreach (var (id, path) in msu.PCMFiles)
                {
                    msuPcms[id] = path;
                }
            }

            msuOptions.Add(msuPcms);
        }

        var random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        var pickedMsu = msuOptions.Random(random) ?? msuOptions.First();
        
        var pcmPath = Path.Combine(outputFolder, $"{outputFileName}-<id>.pcm");
        var msuPath = Path.Combine(outputFolder, $"{outputFileName}.msu");
        var pickedPcms = new Dictionary<int, string>();

        foreach (var (index, path) in pickedMsu)
        {
            pickedPcms.Add(index, path);
            CreatePCMFile(pcmPath, path, index);
        }
        
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

    /// <summary>
    /// Applies the selected filter to the list of MSUs
    /// </summary>
    /// <param name="msus">The list of MSUs</param>
    /// <param name="options">The selected MSURandomizerOptions with the filter details</param>
    /// <returns>The list of matching MSUs</returns>
    public static IEnumerable<MSU> ApplyFilter(IEnumerable<MSU> msus, MSURandomizerOptions options)
    {
        return msus.Where(x => MatchesFilter(x, options.OutputType, options.Filter));
    }

    /// <summary>
    /// Applies the selected filter to the list of MSUs
    /// </summary>
    /// <param name="msus">The list of MSUs</param>
    /// <param name="outputType">The type of MSU that is being generated as output</param>
    /// <param name="filter">The filter to apply</param>
    /// <returns>The list of matching MSUs</returns>
    public static IEnumerable<MSU> ApplyFilter(IEnumerable<MSU> msus, MSUType outputType, MSUFilter filter)
    {
        return msus.Where(x => MatchesFilter(x, outputType.Name, filter));
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
    
    private static void CreatePCMFile(string pcmPath, string originalPath, int pcmIndex)
    {
        pcmPath = pcmPath.Replace("<id>", pcmIndex.ToString());
        if (File.Exists(pcmPath)) File.Delete(pcmPath);
        
        if (pcmPath.StartsWith(originalPath.Substring(0,1)))
            NativeMethods.CreateHardLink(pcmPath, originalPath, IntPtr.Zero);
        else
            File.Copy(originalPath, pcmPath);
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

    private static string ValidateOptions(MSURandomizerOptions options)
    {
        var msus = _msus.Count == 0 ? GetMSUs(options, out _) : _msus;
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.Name) == true).ToList();
        if (msus == null || msus.Count == 0)
        {
            return "No selected MSUs found";
        }

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes() : _msuTypes;
        var type = msuTypes.FirstOrDefault(x => x.Name == options.OutputType);
        if (type == null)
        {
            return "No valid MSU type selected";
        }

        if (string.IsNullOrEmpty(options.RomPath) && string.IsNullOrEmpty(options.Name))
        {
            return "No output option selected";
        }

        if (!string.IsNullOrEmpty(options.RomPath) && !File.Exists(options.RomPath))
        {
            return "Rom file does not exist";
        }

        return "";
    }

    private static (string folder, string fileName) GetOutputData(MSURandomizerOptions options)
    {
        // Check the output settings
        if (string.IsNullOrEmpty(options.RomPath))
        {
            var outputFolder = Path.Combine(options.Directory!, options.Name);
            if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, true);
            Directory.CreateDirectory(outputFolder);
            return (outputFolder, options.Name);
        }
        else
        {
            var file = new FileInfo(options.RomPath);
            var outputFileName = file.Name.Replace(file.Extension, "");
            options.RomPath = null;
            return (file.DirectoryName ?? "", outputFileName);
        }
    }

}
