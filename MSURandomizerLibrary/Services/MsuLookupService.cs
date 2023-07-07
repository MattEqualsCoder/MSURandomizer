using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

internal class MsuLookupService : IMsuLookupService
{
    private readonly ILogger<MsuLookupService> _logger;
    private readonly IMsuTypeService _msuTypeService;
    private readonly IMsuSettingsService _msuSettingsService;
    private readonly IMsuDetailsService _msuDetailsService;
    private IReadOnlyCollection<Msu> _msus = new List<Msu>();

    public MsuLookupService(ILogger<MsuLookupService> logger, IMsuTypeService msuTypeService, IMsuSettingsService msuSettingsService, IMsuDetailsService msuDetailsService)
    {
        _logger = logger;
        _msuTypeService = msuTypeService;
        _msuSettingsService = msuSettingsService;
        _msuDetailsService = msuDetailsService;
    }
    
    public IReadOnlyCollection<Msu> LookupMsus(string directory)
    {
        
        if (!_msuTypeService.MsuTypes.Any())
        {
            throw new InvalidOperationException(
                "No valid MSU Types found. Make sure to call IMsuTypeService.GetMsuTypes first.");
        }

        Status = MsuLoadStatus.Loading;
        _msus = new List<Msu>();
        var msuFiles = Directory.EnumerateFiles(directory, "*.msu", SearchOption.AllDirectories);
        _msus = msuFiles.Select(x => LoadMsu(x)).Where(x => x != null).Cast<Msu>().ToList();
        OnMsuLookupComplete?.Invoke(this, new(_msus));
        Status = MsuLoadStatus.Loaded;
        return _msus;
    }

    public Msu? LoadMsu(string msuPath, MsuType? msuType = null)
    {
        var directory = new FileInfo(msuPath).Directory!.FullName;

        if (File.Exists(Path.Combine(directory, "msu-randomizer-output.txt")))
        {
            return null;
        }
        
        var baseName = Path.GetFileName(msuPath).Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
        var pcmFiles = Directory.EnumerateFiles(directory, $"{baseName}-*.pcm", SearchOption.AllDirectories).ToList();
        var msuSettings = _msuSettingsService.GetMsuSettings(msuPath);

        if (msuType == null && !string.IsNullOrWhiteSpace(msuSettings.MsuType))
        {
            msuType = _msuTypeService.MsuTypes.FirstOrDefault(x => x.Name == msuSettings.MsuType);
        }

        msuType ??= GetMsuType(baseName, pcmFiles);
        
        _logger.LogInformation("MSU {Name} found as MSU Type {Type}", baseName, msuType?.Name ?? "Unknown");

        Msu msu;

        if (msuType == null)
        {
            msu = LoadUnknownMsu(msuPath, directory, baseName, pcmFiles);
        }
        else if (File.Exists(directory + Path.DirectorySeparatorChar + baseName + ".yml"))
        {
            msu = LoadDetailedMsu(msuPath, directory, baseName, msuType, pcmFiles,
                directory + Path.DirectorySeparatorChar + baseName + ".yml");
        }
        else if (File.Exists(directory + Path.DirectorySeparatorChar + baseName + ".yaml"))
        {
            msu = LoadDetailedMsu(msuPath, directory, baseName, msuType, pcmFiles,
                directory + Path.DirectorySeparatorChar + baseName + ".yaml");
        }
        else
        {
            msu = LoadBasicMsu(msuPath, directory, baseName, msuType, pcmFiles);
        }

        msu.Settings = msuSettings;

        if (!string.IsNullOrWhiteSpace(msuSettings.Name))
        {
            msu.Name = msuSettings.Name;
        }
        
        return msu;
    }

    public IReadOnlyCollection<Msu> Msus => _msus.ToList();

    private Msu LoadDetailedMsu(string msuPath, string directory, string baseName, MsuType msuType, IEnumerable<string> pcmFiles, string ymlPath)
    {
        var msuDetails = _msuDetailsService.LoadMsuDetails(msuType, msuPath, directory, baseName, ymlPath);
        if (msuDetails == null)
        {
            return LoadBasicMsu(msuPath, directory, baseName, msuType, pcmFiles);
        }
        var trackDetails = msuDetails.Tracks;
        
        var tracks = new List<Track>();
        foreach (var track in msuType.Tracks)
        {
            var trackNumber = track.Number;
            
            if (trackDetails.All(x => x.Number != trackNumber) && track.IsExtended)
            {
                var currentTrack = track;
                while (trackDetails.All(x => x.Number != trackNumber) && currentTrack?.IsExtended == true)
                {
                    trackNumber = currentTrack.Fallback;
                    currentTrack = msuType.Tracks.FirstOrDefault(x => x.Number == trackNumber);
                }
            }
            
            foreach (var innerTrackDetails in trackDetails.Where(x => x.Number == trackNumber))
            {
                tracks.Add(new Track(innerTrackDetails, number: track.Number));
            }
        }

        return new Msu
        {
            FolderName = new DirectoryInfo(directory).Name,
            FileName = baseName,
            Path = msuPath,
            Tracks = tracks,
            MsuType = msuType,
            Name = msuDetails.Name,
            Creator = msuDetails.Creator
        };
    }
    
    private Msu LoadUnknownMsu(string msuPath, string directory, string baseName, IEnumerable<string> pcmFiles)
    {
        _logger.LogInformation("Loading Unknown MSU {MsuPath}{Sep}{Name}.msu", directory, Path.DirectorySeparatorChar, baseName);

        var tracks = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "").Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(x => new Track
                (
                    trackName: $"Track #{x}",
                    number: int.Parse(x),
                    songName: $"Track #{x}",
                    path: $"{directory}{Path.DirectorySeparatorChar}{baseName}-{x}.pcm",
                    msuPath: $"{directory}{Path.DirectorySeparatorChar}{baseName}.msu",
                    msuName: baseName
                ));
        
        return new Msu
        {
            FolderName = new DirectoryInfo(directory).Name,
            FileName = baseName,
            Path = msuPath,
            Tracks = tracks.ToList(),
            Name = baseName
        };
    }

    private Msu LoadBasicMsu(string msuPath, string directory, string baseName, MsuType msuType, IEnumerable<string> pcmFiles)
    {
        var trackNumbers = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "").Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();

        var tracks = new List<Track>();
        foreach (var track in msuType.Tracks)
        {
            var trackNumber = track.Number;
            
            if (!trackNumbers.Contains(trackNumber) && track.IsExtended)
            {
                var currentTrack = track;
                while (!trackNumbers.Contains(trackNumber) && currentTrack?.IsExtended == true)
                {
                    trackNumber = currentTrack.Fallback;
                    currentTrack = msuType.Tracks.FirstOrDefault(x => x.Number == trackNumber);
                }
            }
            
            if (!trackNumbers.Contains(trackNumber))
            {
                continue;
            }

            tracks.Add(new Track
            (
                trackName: msuType.Tracks.FirstOrDefault(x => x.Number == track.Number)?.Name ?? $"Track #{trackNumber}",
                number: track.Number,
                songName: $"Track #{trackNumber}",
                path: pcmFiles.First(x =>
                    x.Equals($"{directory}{Path.DirectorySeparatorChar}{baseName}-{trackNumber}.pcm",
                        StringComparison.OrdinalIgnoreCase)),
                msuPath: $"{directory}{Path.DirectorySeparatorChar}{baseName}.msu",
                msuName: baseName
            ));
        }
        
        return new Msu
        {
            FolderName = new DirectoryInfo(directory).Name,
            FileName = baseName,
            Path = msuPath,
            Tracks = tracks,
            MsuType = msuType,
            Name = baseName
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
            if (requiredConfidence <= 0.85)
            {
                continue;
            }
            var allConfidence = 1.0f * msuType.ValidTrackNumbers.Intersect(trackNumbers).Count() / msuType.ValidTrackNumbers.Count;
            matchingMsus.Add((msuType, requiredConfidence, allConfidence));
        }

        return matchingMsus
            .OrderByDescending(x => x.Type.Name is "Super Metroid" or "The Legend of Zelda: A Link to the Past")
            .ThenByDescending(x => (x.RequiredConfidence + x.AllConfidence) / 2)
            .Select(x => x.Type)
            .FirstOrDefault();
    }
    
    public event EventHandler<MsuListEventArgs>? OnMsuLookupComplete;
    
    public MsuLoadStatus Status { get; set; }
}