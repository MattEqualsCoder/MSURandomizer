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
        
        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes(options) : _msuTypes;
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
    public static List<MSUType> GetMSUTypes(MSURandomizerOptions options)
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
        
#if DEBUG
        _msuTypes = types;
        return types;
#else
        // If a config file is passed in, load all yaml/yml files there
        if (!string.IsNullOrEmpty(options.MsuTypeConfigPath))
        {
            var directory = new DirectoryInfo(options.MsuTypeConfigPath);
            if (directory.Exists)
            {
                foreach (var file in directory.EnumerateFiles()
                             .Where(f => f.Extension.EndsWith("yml") || f.Extension.EndsWith("yaml")))
                {
                    using var fileStream = File.OpenRead(file.FullName);
                    using var reader = new StreamReader(fileStream);
                    var msuType = deserializer.Deserialize<MSUType>(reader.ReadToEnd());
                    var oldType = types.FirstOrDefault(x => x.Name == msuType.Name);
                    if (oldType != null) types.Remove(oldType);
                    types.Add(msuType);
                }
            }
        }

        _msuTypes = types;

        return types;
#endif
        
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
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.FileName) == true).ToList();

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes(options) : _msuTypes;
        var type = msuTypes.FirstOrDefault(x => x.Name == options.OutputType);

        var outputData = GetOutputData(options);
        var outputFolder = outputData.folder;
        var outputFileName = outputData.fileName;
        
        var pcmFiles = new Dictionary<int, List<string>>();

        var remappings = GetRemapDictionary(type!.Remaps);
        var conversionTypes = type.Conversions?.ToDictionary(x => x.MSUType, x => x);
        var conversionRemappings =
            type.Conversions?.ToDictionary(x => x.MSUType, x => GetRemapDictionary(x.ManualRemaps));
        var anyPerfectMatch = false;

        // Loop through all MSUs to collect all of the PCMs for each 
        foreach (var msu in msus!)
        {
            // If this MSU is an exact type match, copy over the PCM ids as-is or do remappings
            if (msu.Type != null && msu.Type.Name == type.Name)
            {
                anyPerfectMatch = true;
                var msuPcmIds = msu.PCMFiles.Select(x => x.Key).ToHashSet();
                
                foreach (var (id, path) in msu.PCMFiles)
                {
                    // Copy the exact file
                    if (!pcmFiles.ContainsKey(id)) pcmFiles.Add(id, new List<string>());
                    pcmFiles[id].Add(path);

                    if (!remappings.ContainsKey(id)) continue;
                    var pcmRemapping = remappings[id];

                    // Perform remaps
                    foreach (var otherId in pcmRemapping.mappedIds.Where(otherId => !msuPcmIds.Contains(otherId) || !pcmRemapping.optional))
                    {
                        if (!pcmFiles.ContainsKey(otherId)) pcmFiles.Add(otherId, new List<string>());
                        pcmFiles[otherId].Add(path);
                    }
                }
            }
            // If this MSU is in the accepted conversions, copy over PCMs that have found conversions
            else if (msu.Type != null && conversionTypes?.ContainsKey(msu.TypeName) == true)
            {
                var conversion = conversionTypes[msu.TypeName];
                var conversionMapping = conversionRemappings?[msu.TypeName];
                var msuPcmIds = msu.PCMFiles.Select(x => x.Key + conversion.DefaultModifier).ToHashSet();

                foreach (var (originalId, path) in msu.PCMFiles.Where(x => x.Key >= conversion.MinimumTrackNumber && x.Key <= conversion.MaximumTrackNumber))
                {
                    var id = originalId + conversion.DefaultModifier;
                    
                    // Copy the exact file
                    if (!pcmFiles.ContainsKey(id)) pcmFiles.Add(id, new List<string>());
                    pcmFiles[id].Add(msu.PCMFiles[originalId]);
                    
                    if (conversionMapping == null || conversionMapping.ContainsKey(originalId) == false) continue;
                    var pcmRemapping = conversionMapping[originalId];
                    
                    // Perform remaps
                    foreach (var otherId in pcmRemapping.mappedIds.Where(otherId => !msuPcmIds.Contains(otherId) || !pcmRemapping.optional))
                    {
                        if (!pcmFiles.ContainsKey(otherId)) pcmFiles.Add(otherId, new List<string>());
                        pcmFiles[otherId].Add(path);
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
            
            pickedPcms.Add(pcmIndex, path);
            placedIndexes.Add(pcmIndex);
            usedPaths.Add(path);

            // If there are any pairs of PCM files that should go together
            // For example, the intro songs for Super Metroid
            if (type.Pairs?.ContainsKey(pcmIndex) == true && path.Contains($"-{pcmIndex}.pcm"))
            {
                var newIndex = type.Pairs[pcmIndex];
                path = path.Replace($"-{pcmIndex}.pcm", $"-{newIndex}.pcm");
                if (!File.Exists(path)) continue;
                
                pickedPcms.Add(newIndex, path);
                placedIndexes.Add(newIndex);
                usedPaths.Add(path);
            }
        }

        foreach (var (pcmIndex, songPath) in pickedPcms)
        {
            CreatePCMFile(pcmPath, songPath, pcmIndex);
        }

        var msuPath = Path.Combine(outputFolder, $"{outputFileName}.msu");
        
        if (!File.Exists(msuPath))
        {
            using (File.Create(msuPath)) { }
        }

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
        if (!anyPerfectMatch)
        {
            error = "None of the selected MSUs exactly match the requested MSU type. Some tracks could be missing.";
        }
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
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.FileName) == true).ToList();

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes(options) : _msuTypes;
        var type = msuTypes.FirstOrDefault(x => x.Name == options.OutputType);

        var outputData = GetOutputData(options);
        var outputFolder = outputData.folder;
        var outputFileName = outputData.fileName;
        
        var conversionTypes = type!.Conversions?.ToDictionary(x => x.MSUType, x => x) ??
                              new Dictionary<string, MSUConversion>();
        var conversionRemappings =
            type.Conversions?.ToDictionary(x => x.MSUType, x => GetRemapDictionary(x.ManualRemaps));
        var msuOptions = new List<(MSU msu, Dictionary<int, string> tracks)>();
        
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
                var conversionMapping = conversionRemappings?[msu.TypeName];
                var msuPcmIds = msu.PCMFiles.Select(x => x.Key + conversion.DefaultModifier).ToHashSet();

                foreach (var (originalId, path) in msu.PCMFiles.Where(x => x.Key >= conversion.MinimumTrackNumber && x.Key <= conversion.MaximumTrackNumber))
                {
                    var id = originalId + conversion.DefaultModifier;
                    
                    // Copy the exact file
                    msuPcms[id] = path;
                    
                    if (conversionMapping == null || conversionMapping.ContainsKey(originalId) == false) continue;
                    var pcmRemapping = conversionMapping[originalId];
                    
                    // Perform remaps
                    foreach (var otherId in pcmRemapping.mappedIds.Where(otherId => !msuPcmIds.Contains(otherId)))
                    {
                        msuPcms[id] = path;
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

            msuOptions.Add((msu, msuPcms));
        }

        var random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        var pickedMsu = msuOptions.Random(random);
            
        var pcmPath = Path.Combine(outputFolder, $"{outputFileName}-<id>.pcm");
        var msuPath = Path.Combine(outputFolder, $"{outputFileName}.msu");
        var pickedPcms = new Dictionary<int, string>();

        foreach (var (index, path) in pickedMsu.tracks)
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
        if (type.Name != pickedMsu.msu.TypeName)
        {
            error = "Randomly selected MSU type does not match requested type. Some tracks could be missing.";
        }
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
        return msus.Where(x => MatchesFilter(x, options.OutputType, options.Filter, options));
    }

    /// <summary>
    /// Applies the selected filter to the list of MSUs
    /// </summary>
    /// <param name="msus">The list of MSUs</param>
    /// <param name="outputType">The type of MSU that is being generated as output</param>
    /// <param name="filter">The filter to apply</param>
    /// <param name="options"></param>
    /// <returns>The list of matching MSUs</returns>
    public static IEnumerable<MSU> ApplyFilter(IEnumerable<MSU> msus, MSUType outputType, MSUFilter filter, MSURandomizerOptions options)
    {
        return msus.Where(x => MatchesFilter(x, outputType.Name, filter, options));
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
        
        // Find all MSU types that match
        // If there are more than one, then check for PCM files that are set to loop/not loop and find one that matches
        var validMsuTypes = msuTypes.Where(x => x.Matches(validPcmFiles));
        var msuType = validMsuTypes.Count() > 1 ? validMsuTypes.FirstOrDefault(x => x.MatchesOnPCMLoops(validPcmFiles)) : validMsuTypes.FirstOrDefault();
        
        return new MSU
        {
            FileName = msuName, 
            MSUPath = msuFile, 
            PCMFiles = validPcmFiles, 
            Type = msuType,
            FolderName = new DirectoryInfo(directory).Name
        };
    }

    private static bool MatchesFilter(MSU msu, string? typeName, MSUFilter filter, MSURandomizerOptions options)
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
                var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes(options) : _msuTypes;
                var type = msuTypes.FirstOrDefault(x => x.Name == typeName);
                return (type?.Conversions?.Any(x => x.MSUType == msu.Type?.Name) == true);
            }
        }
    }
    
    private static void CreatePCMFile(string pcmPath, string originalPath, int pcmIndex)
    {
        pcmPath = pcmPath.Replace("<id>", pcmIndex.ToString());
        if (File.Exists(pcmPath))
        {
            try
            {
                File.Delete(pcmPath);
            }
            catch
            {
                return;
            }
        }
        
        if (pcmPath.StartsWith(originalPath.Substring(0,1)))
            NativeMethods.CreateHardLink(pcmPath, originalPath, IntPtr.Zero);
        else
            File.Copy(originalPath, pcmPath);
    }

    private static Dictionary<int, (bool optional, List<int> mappedIds)> GetRemapDictionary(IEnumerable<MSUTrackRemapping>? remappings)
    {
        var toReturn = new Dictionary<int, (bool required, List<int> mappedIds)>();

        if (remappings == null) return toReturn;

        foreach (var remap in remappings)
        {
            foreach (var originalTrackNumber in remap.OriginalTrackNumbersList)
            {
                if (toReturn.ContainsKey(originalTrackNumber))
                    throw new InvalidOperationException($"{originalTrackNumber} is mapped twice");
                toReturn[originalTrackNumber] = (remap.OnlyAddIfMissing, remap.RemappedTrackNumbersList);
            }
        }

        return toReturn;
    }

    private static string ValidateOptions(MSURandomizerOptions options)
    {
        var msus = _msus.Count == 0 ? GetMSUs(options, out _) : _msus;
        msus = msus?.Where(x => options.SelectedMSUs?.Contains(x.FileName) == true).ToList();
        if (msus == null || msus.Count == 0)
        {
            return "No selected MSUs found";
        }

        var msuTypes = _msuTypes.Count == 0 ? GetMSUTypes(options) : _msuTypes;
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
            if (Directory.Exists(outputFolder))
            {
                if (options.DeleteFolder)
                {
                    Directory.Delete(outputFolder, true);
                }
                else
                {
                    return (outputFolder, options.Name);
                }
            }
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

    public static bool DoesPCMLoop(string fileName)
    {
        return File.ReadAllBytes(fileName).Skip(4).Take(4).Any(x => x != 0);
    }

}
