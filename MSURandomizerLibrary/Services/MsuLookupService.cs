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
    private readonly MsuUserOptions _msuUserOptions;
    private readonly IMsuDetailsService _msuDetailsService;
    private readonly MsuAppSettings _msuAppSettings;
    private IReadOnlyCollection<Msu> _msus = new List<Msu>();

    public MsuLookupService(ILogger<MsuLookupService> logger, IMsuTypeService msuTypeService, MsuUserOptions msuUserOptions, IMsuDetailsService msuDetailsService, MsuAppSettings msuAppSettings)
    {
        _logger = logger;
        _msuTypeService = msuTypeService;
        _msuUserOptions = msuUserOptions;
        _msuDetailsService = msuDetailsService;
        _msuAppSettings = msuAppSettings;
    }

    public IReadOnlyCollection<Msu> LookupMsus()
    {
        return LookupMsus(_msuUserOptions.DefaultMsuPath, _msuUserOptions.MsuTypePaths);
    }

    public IReadOnlyCollection<Msu> LookupMsus(string? defaultDirectory, Dictionary<MsuType, string>? msuTypeDirectories = null)
    {
        if (!_msuTypeService.MsuTypes.Any())
        {
            throw new InvalidOperationException(
                "No valid MSU Types found. Make sure to call IMsuTypeService.GetMsuTypes first.");
        }

        _logger.LogInformation("MSU lookup started");
        Status = MsuLoadStatus.Loading;
        var msus = new List<Msu>();
        
        if (Directory.Exists(defaultDirectory))
        {
            var msuFiles = Directory.EnumerateFiles(defaultDirectory, "*.msu", SearchOption.AllDirectories);
            msus.AddRange(msuFiles.Select(x => LoadMsu(x)).Where(x => x != null).Cast<Msu>());    
        }
        
        if (msuTypeDirectories != null)
        {
            foreach (var msuTypeDirectory in msuTypeDirectories)
            {
                if (!Directory.Exists(msuTypeDirectory.Value)) continue;
                var msuFiles = Directory.EnumerateFiles(msuTypeDirectory.Value, "*.msu", SearchOption.AllDirectories);
                msus.AddRange(msuFiles.Select(x => LoadMsu(x, msuTypeDirectory.Key)).Where(x => x != null).Cast<Msu>());   
            }
        }

        _msus = msus;
        _logger.LogInformation("MSU lookup complete");
        Status = MsuLoadStatus.Loaded;
        OnMsuLookupComplete?.Invoke(this, new(_msus));
        return _msus;
    }

    public Msu? LoadMsu(string msuPath, MsuType? msuTypeFilter = null)
    {
        var directory = new FileInfo(msuPath).Directory!.FullName;

        if (File.Exists(Path.Combine(directory, "msu-randomizer-output.txt")))
        {
            return null;
        }
        
        var baseName = Path.GetFileName(msuPath).Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
        var pcmFiles = Directory.EnumerateFiles(directory, $"{baseName}-*.pcm", SearchOption.AllDirectories).ToList();
        var msuSettings = _msuUserOptions.GetMsuSettings(msuPath);

        var msuType = GetMsuType(baseName, pcmFiles, msuTypeFilter);
        
        _logger.LogInformation("MSU {Name} found as MSU Type {Type}", baseName, msuType?.Name ?? "Unknown");

        Msu msu;
        
        // If it's an unknown MSU type, simply load the details as is
        if (msuType == null)
        {
            msu = LoadUnknownMsu(msuPath, directory, baseName, pcmFiles);
        }
        // Check if there's a YAML file associated with the MSU to pull MSU and track information from
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
        // Otherwise load it using the details from the MSU type
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

    private MsuType? GetMsuType(string baseName, IEnumerable<string> pcmFiles, MsuType? msuTypeFilter = null)
    {
        var trackNumbers = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "").Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();
        
        var matchingMsus = new List<(MsuType Type, float Confidence, int ValidTracks)>();
        
        var allowedMsuTypes = _msuTypeService.MsuTypes.ToList();
        if (msuTypeFilter != null && !_msuAppSettings.Smz3MsuTypes.Contains(msuTypeFilter.Name))
        {
            allowedMsuTypes = allowedMsuTypes.Where(x => x.IsCompatibleWith(msuTypeFilter)).ToList();
        }
        
        foreach (var msuType in allowedMsuTypes)
        {
            if (trackNumbers.Count > msuType.ValidTrackNumbers.Count)
            {
                continue;
            }
            var requiredConfidence = 1.0f * msuType.RequiredTrackNumbers.Intersect(trackNumbers).Count() / msuType.RequiredTrackNumbers.Count;
            var allConfidence = 1.0f * msuType.ValidTrackNumbers.Intersect(trackNumbers).Count() / msuType.ValidTrackNumbers.Count;
            var confidence = Math.Max(requiredConfidence, allConfidence);
            if (confidence <= .85)
            {
                continue;
            }
            var validTracks = msuType.ValidTrackNumbers.Intersect(trackNumbers).Count();
            matchingMsus.Add((msuType, confidence, validTracks));
        }
        
        var matchedType = matchingMsus
            .OrderByDescending(x => _msuAppSettings.Smz3MsuTypes.Contains(x.Type.Name))
            .ThenByDescending(x => x.Confidence)
            .ThenByDescending(x => x.ValidTracks)
            .Select(x => x.Type)
            .FirstOrDefault();
        
        if (msuTypeFilter != null && matchedType != null && !matchedType.IsCompatibleWith(msuTypeFilter))
        {
            return null;
        }

        return matchedType;
    }
    
    public event EventHandler<MsuListEventArgs>? OnMsuLookupComplete;
    
    public MsuLoadStatus Status { get; set; }
}