using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

internal class MsuLookupService : IMsuLookupService
{
    private ILogger<MsuLookupService> _logger;
    private IMsuTypeService _msuTypeService;

    public MsuLookupService(ILogger<MsuLookupService> logger, IMsuTypeService msuTypeService)
    {
        _logger = logger;
        _msuTypeService = msuTypeService;
    }
    
    public IEnumerable<Msu> LookupMsus(string directory)
    {
        if (!_msuTypeService.MsuTypes.Any())
        {
            throw new InvalidOperationException(
                "No valid MSU Types found. Make sure to call IMsuTypeService.GetMsuTypes first.");
        }
        
        var msuFiles = Directory.EnumerateFiles(directory, "*.msu", SearchOption.AllDirectories);
        return msuFiles.Select(LoadMsu).ToList();
    }

    public Msu LoadMsu(string msuPath)
    {
        var minTracks = _msuTypeService.MsuTypes.Min(x => x.RequiredTrackNumbers.Count);
        
        var directory = new FileInfo(msuPath).Directory!.FullName;
        var baseName = Path.GetFileName(msuPath).Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
        var pcmFiles = Directory.EnumerateFiles(directory, $"{baseName}-*.pcm", SearchOption.AllDirectories);

        var type = GetMsuType(baseName, pcmFiles);
        
        _logger.LogInformation("MSU {Name} found as MSU Type {Type}", baseName, type?.Name ?? "Unknown");

        if (File.Exists(directory + Path.DirectorySeparatorChar + baseName + ".yml"))
        {
            return LoadDetailedMsu(directory, baseName, pcmFiles,
                directory + Path.DirectorySeparatorChar + baseName + ".yml");
        }
        else if (File.Exists(directory + Path.DirectorySeparatorChar + baseName + ".yaml"))
        {
            return LoadDetailedMsu(directory, baseName, pcmFiles,
                directory + Path.DirectorySeparatorChar + baseName + ".yaml");
        }
        else
        {
            return LoadBasicMsu(directory, baseName, pcmFiles);
        }
    }

    private Msu LoadDetailedMsu(string directory, string baseName, IEnumerable<string> pcmFiles, string ymlPath)
    {
        //_logger.LogInformation("Loading detailed MSU {Directory}\\{Msu}.msu with YAML file: {Yaml}", directory, baseName, ymlPath);
        return new Msu
        {
            FolderName = "",
            FileName = "",
            Path = "",
            Tracks = new Dictionary<int, List<Track>>()
        };
    }

    private Msu LoadBasicMsu(string directory, string baseName, IEnumerable<string> pcmFiles)
    {
        //_logger.LogInformation("Loading basic MSU {Directory}\\{Msu}.msu", directory, baseName);
        return new Msu
        {
            FolderName = "",
            FileName = "",
            Path = "",
            Tracks = new Dictionary<int, List<Track>>()
        };
    }

    private MsuType? GetMsuType(string baseName, IEnumerable<string> pcmFiles)
    {
        var trackNumbers = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "").Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();
        
        var matchingMsus = new List<(MsuType Type, float RequiredConfidence, float AllConfidence)>();

        foreach (var msuType in _msuTypeService.MsuTypes)
        {
            if (trackNumbers.Count > msuType.ValidTrackNumbers.Count)
            {
                continue;
            }
            var requiredConfidence = 1.0f * msuType.RequiredTrackNumbers.Intersect(trackNumbers).Count() / msuType.RequiredTrackNumbers.Count;
            if (requiredConfidence <= 0.9)
            {
                continue;
            }
            var allConfidence = 1.0f * msuType.ValidTrackNumbers.Intersect(trackNumbers).Count() / msuType.ValidTrackNumbers.Count;
            matchingMsus.Add((msuType, requiredConfidence, allConfidence));
        }

        return matchingMsus
            .OrderByDescending(x => x.Type.Name is "Super Metroid" or "The Legend of Zelda: A Link to the Past")
            .ThenByDescending(x => x.RequiredConfidence)
            .ThenByDescending(x => x.AllConfidence)
            .Select(x => x.Type)
            .FirstOrDefault();
    }
}