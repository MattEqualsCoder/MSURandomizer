﻿using System.Text.RegularExpressions;
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
    private readonly Dictionary<string, string> _errors = new();
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
        _errors.Clear();
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
        OnMsuLookupComplete?.Invoke(this, new(_msus, _errors));
        return _msus;
    }

    public Msu? LoadMsu(string msuPath, MsuType? msuTypeFilter = null)
    {
        var directory = new FileInfo(msuPath).Directory!.FullName;

        if (File.Exists(Path.Combine(directory, "msu-randomizer-output.txt")))
        {
            return null;
        }

        var msuDetails = _msuDetailsService.GetMsuDetails(msuPath, out var yamlPath, out var yamlError);
        if (!string.IsNullOrEmpty(yamlError))
        {
            _errors[msuPath] = yamlError;
        }
        
        var baseName = Path.GetFileName(msuPath).Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
        var pcmFiles = Directory.EnumerateFiles(directory, $"{baseName}-*.pcm", SearchOption.AllDirectories).ToList();
        var msuSettings = _msuUserOptions.GetMsuSettings(msuPath);
        
        msuSettings.MsuType = _msuTypeService.GetMsuType(msuSettings.MsuTypeName);

        // Try to load the MSU type specified by the MSU details if possible, otherwise try to auto detect
        var originalMsuType = !string.IsNullOrEmpty(msuDetails?.MsuType) 
            ? (_msuTypeService.GetMsuType(msuDetails.MsuType) ?? GetMsuType(baseName, pcmFiles, msuTypeFilter))
            : GetMsuType(baseName, pcmFiles, msuTypeFilter);
        
        var msuType = msuSettings.MsuType ?? originalMsuType;
        
        _logger.LogInformation("MSU {Name} found as MSU Type {Type}", baseName, msuType?.DisplayName ?? "Unknown");

        Msu msu;
        
        // If it's an unknown MSU type, simply load the details as is
        if (msuType == null)
        {
            msu = LoadUnknownMsu(msuPath, directory, baseName, pcmFiles);
        }
        // Check if there's a YAML file associated with the MSU to pull MSU and track information from
        else if (msuDetails != null && msuDetails.Tracks?.Any() == true)
        {
            msu = LoadDetailedMsu(msuPath, directory, baseName, msuType, pcmFiles, msuDetails);
        }
        // Otherwise load it using the details from the MSU type
        else
        {
            msu = LoadBasicMsu(msuPath, directory, baseName, msuType, pcmFiles, msuDetails);
        }

        msu.Settings = msuSettings;
        msu.MsuType = originalMsuType;
        msu.Version = msuDetails?.PackVersion;
        msu.Artist = msuDetails?.Artist;
        msu.Album = msuDetails?.Album;
        msu.Url = msuDetails?.Url;

        return msu;
    }

    public IReadOnlyCollection<Msu> Msus => _msus.ToList();

    private Msu LoadDetailedMsu(string msuPath, string directory, string baseName, MsuType msuType, IEnumerable<string> pcmFiles, MsuDetails basicDetails)
    {
        var msu = _msuDetailsService.ConvertToMsu(basicDetails, msuType, msuPath, directory, baseName, out var yamlError);
        
        if (!string.IsNullOrEmpty(yamlError))
        {
            _errors[msuPath] = yamlError;
        }
        
        if (msu?.Tracks.Any() != true)
        {
            return LoadBasicMsu(msuPath, directory, baseName, msuType, pcmFiles, basicDetails);
        }
        var trackDetails = msu.Tracks;
        
        var tracks = new List<Track>();
        foreach (var track in msuType.Tracks)
        {
            var trackNumber = track.Number;
            
            // If this track was not found and it's extended, try to use the track that it's extending
            if (trackDetails.All(x => x.Number != trackNumber) && track.IsExtended)
            {
                var currentTrack = track;
                while (trackDetails.All(x => x.Number != trackNumber) && currentTrack?.IsExtended == true)
                {
                    trackNumber = currentTrack.Fallback;
                    currentTrack = msuType.Tracks.FirstOrDefault(x => x.Number == trackNumber);
                }
            }
            
            // Find all tracks matching the found track number and add them
            foreach (var innerTrackDetails in trackDetails.Where(x => x.Number == trackNumber))
            {
                var trackName = msuType.Tracks.FirstOrDefault(x => x.Number == track.Number)?.Name ??
                                innerTrackDetails.TrackName;
                tracks.Add(new Track(innerTrackDetails, number: track.Number, trackName: trackName)
                {
                    IsCopied = trackNumber != track.Number,
                    Msu = msu,
                });
            }
        }

        msu.Tracks = tracks;

        return msu;
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
                    path: $"{directory}{Path.DirectorySeparatorChar}{baseName}-{x}.pcm"
                ));

        return new Msu(
            type: null, 
            name: new DirectoryInfo(directory).Name, 
            folderName: new DirectoryInfo(directory).Name, 
            fileName: baseName,
            path: msuPath,
            tracks: tracks.ToList(),
            msuDetails: null,
            prevMsu: null
        );
    }

    private Msu LoadBasicMsu(string msuPath, string directory, string baseName, MsuType msuType, IEnumerable<string> pcmFiles, MsuDetails? msuDetails)
    {
        var trackNumbers = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "").Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();

        var basicPcmFiles = trackNumbers.Select(x => Path.Combine(directory, $"{baseName}-{x}.pcm")).ToList();
        var extraPcmFiles = pcmFiles.Where(x => !basicPcmFiles.Contains(x));
        
        var tracks = new List<Track>();
        foreach (var track in msuType.Tracks)
        {
            var trackNumber = track.Number;
            
            // If this is missing extended track, try to find the track it's extending until we find one
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

            var path = pcmFiles.FirstOrDefault(x =>
                x.Equals($"{directory}{Path.DirectorySeparatorChar}{baseName}-{trackNumber}.pcm",
                    StringComparison.OrdinalIgnoreCase));

            if (path == null)
            {
                continue;
            }
            
            // Add the base track
            tracks.Add(new Track
            (
                trackName: msuType.Tracks.FirstOrDefault(x => x.Number == track.Number)?.Name ?? $"Track #{trackNumber}",
                number: track.Number,
                songName: $"Track #{trackNumber}",
                path: path
            ){
                IsCopied = trackNumber != track.Number
            });

            // See if there are any alt tracks to add
            var alts = extraPcmFiles.Where(x => x != path && Regex.IsMatch(x.Replace(Path.Combine(directory, baseName), ""), $"-{trackNumber}[^0-9]")).ToList();
            if (!alts.Any()) continue;
            
            foreach (var alt in alts)
            {
                var relativePath = Path.GetRelativePath(directory, alt);
                var altName = new FileInfo(alt).Name;
                if (relativePath.Contains(Path.DirectorySeparatorChar))
                {
                    altName = relativePath.Substring(0, relativePath.IndexOf(Path.DirectorySeparatorChar));
                }
                
                tracks.Add(new Track
                (
                    trackName: msuType.Tracks.FirstOrDefault(x => x.Number == track.Number)?.Name ?? $"Track #{trackNumber}",
                    number: track.Number,
                    songName: $"Track #{trackNumber} ({altName})",
                    path: alt,
                    isAlt: true
                ){
                    IsCopied = trackNumber != track.Number
                });
            }
        }

        return new Msu(
            type: msuType,
            name: msuDetails?.PackName ?? new DirectoryInfo(directory).Name,
            folderName: new DirectoryInfo(directory).Name,
            fileName: baseName,
            path: msuPath,
            tracks: tracks,
            msuDetails: msuDetails,
            prevMsu: null
        );
    }

    private MsuType? GetMsuType(string baseName, IEnumerable<string> pcmFiles, MsuType? msuTypeFilter = null)
    {
        var trackNumbers = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "").Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();
        
        var matchingMsus = new List<(MsuType Type, float PrimaryConfidence, int PrimaryCount, float SecondaryConfidence, int SecondaryCount)>();
        
        var allowedMsuTypes = _msuTypeService.MsuTypes.ToList();
        if (msuTypeFilter != null && !_msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(msuTypeFilter.DisplayName))
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
            var requiredCount = msuType.RequiredTrackNumbers.Intersect(trackNumbers).Count();
            var allCount = msuType.ValidTrackNumbers.Intersect(trackNumbers).Count();
            float primaryConfidence;
            float secondaryConfidence;
            int primaryCount;
            int secondaryCount;

             if (allConfidence >= requiredConfidence)
            {
                primaryConfidence = allConfidence;
                secondaryConfidence = requiredConfidence;
                primaryCount = allCount;
                secondaryCount = requiredCount;
            }
            else
            {
                primaryConfidence = requiredConfidence;
                secondaryConfidence = allConfidence;
                primaryCount = requiredCount;
                secondaryCount = allCount;
            }

            if (primaryConfidence <= 0.85f)
            {
                continue;
            }
            
            matchingMsus.Add((msuType, primaryConfidence, primaryCount, secondaryConfidence, secondaryCount));
        }
        
        var matchedType = matchingMsus
            .OrderByDescending(x => _msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(x.Type.DisplayName) || _msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(x.Type.Name))
            .ThenByDescending(x => x.PrimaryConfidence)
            .ThenByDescending(x => x.PrimaryCount)
            .ThenByDescending(x => x.SecondaryConfidence)
            .ThenByDescending(x => x.SecondaryCount)
            .Select(x => x.Type)
            .FirstOrDefault();
        
        if (msuTypeFilter != null && matchedType != null && !matchedType.IsCompatibleWith(msuTypeFilter))
        {
            return null;
        }

        return matchedType;
    }
    
    public event EventHandler<MsuListEventArgs>? OnMsuLookupComplete;
    
    public void RefreshMsu(Msu msu)
    {
        Status = MsuLoadStatus.Loading;
        _errors.Clear();
        
        // Reload the MSU
        var newMsu = LoadMsu(msu.Path);
        if (newMsu == null)
        {
            _logger.LogInformation("Reloaded MSU for Path {Path} returned null", msu.Path);
            throw new InvalidOperationException("Could not refresh MSU");
        }
        
        // Remove the previous MSU, and add the new one
        var msus = _msus.ToList();
        msus.RemoveAll(x => x.Path == msu.Path);
        msus.Add(newMsu);
        _msus = msus;
        
        OnMsuLookupComplete?.Invoke(this, new MsuListEventArgs(_msus, _errors));
        Status = MsuLoadStatus.Loaded;
    }

    public MsuLoadStatus Status { get; set; }

    public ICollection<Msu> GetMsusByPath(ICollection<string>? paths)
    {
        if (paths == null)
        {
            return new List<Msu>();
        }

        return _msus.Where(x => paths.Contains(x.Path)).ToList();
    }
    
    public Msu? GetMsuByPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return _msus.FirstOrDefault(x => x.Path == path);
    }

    public IDictionary<string, string> Errors => _errors;
}