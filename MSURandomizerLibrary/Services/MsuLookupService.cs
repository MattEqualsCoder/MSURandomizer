﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using SnesConnectorLibrary.Responses;

namespace MSURandomizerLibrary.Services;

internal class MsuLookupService(
    ILogger<MsuLookupService> logger,
    IMsuTypeService msuTypeService,
    IMsuDetailsService msuDetailsService,
    MsuAppSettings msuAppSettings,
    IMsuCacheService msuCacheService,
    IMsuUserOptionsService msuUserOptionsService) : IMsuLookupService
{
    private readonly Dictionary<string, string> _errors = new();
    private IReadOnlyCollection<Msu> _msus = new List<Msu>();
    private MsuUserOptions MsuUserOptions => msuUserOptionsService.MsuUserOptions;

    public IReadOnlyCollection<Msu> LookupMsus()
    {
        return LookupMsus(MsuUserOptions.DefaultMsuPath, MsuUserOptions.MsuDirectories);
    }

    public void RefreshMsuDisplay()
    {
        OnMsuLookupComplete?.Invoke(this, new MsuListEventArgs(_msus, _errors));
    }

    public IReadOnlyCollection<Msu> LookupMsus(string? defaultDirectory, Dictionary<string, string>? msuDirectories = null, bool ignoreCache = false)
    {
        if (!msuTypeService.MsuTypes.Any())
        {
            throw new InvalidOperationException(
                "No valid MSU Types found. Make sure to call IMsuTypeService.GetMsuTypes first.");
        }

        msuDirectories ??= MsuUserOptions.MsuDirectories;

        logger.LogInformation("MSU lookup started");
        _errors.Clear();
        Status = MsuLoadStatus.Loading;
        OnMsuLookupStarted?.Invoke(this, EventArgs.Empty);
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!string.IsNullOrEmpty(defaultDirectory) && msuDirectories?.Count == 0)
        {
            msuDirectories = new Dictionary<string, string>()
            {
                { defaultDirectory, "" }
            };
        }

        if (msuDirectories == null || msuDirectories.Count == 0)
        {
            _msus = [];
            Status = MsuLoadStatus.Loaded;
            OnMsuLookupComplete?.Invoke(this, new(_msus, _errors));
            return [];
        }

        var msusToLoad = GetMsusToLoad(msuDirectories!);

        var msus = new ConcurrentBag<Msu>();

        Parallel.ForEach(msusToLoad, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, loadInfo =>
        {
            var msu = LoadMsu(loadInfo.msuPath, loadInfo.msuType, false, ignoreCache, parentDirectory: loadInfo.msuTypeDirectory);
            if (msu != null)
            {
                msus.Add(msu);
            }
        });

        _msus = msus.ToList();
        msuCacheService.Save();
        
        stopwatch.Stop();
        logger.LogInformation("MSU lookup complete in {Seconds} seconds (found {Count} MSUs)", stopwatch.Elapsed.TotalSeconds, _msus.Count);
        Status = MsuLoadStatus.Loaded;
        OnMsuLookupComplete?.Invoke(this, new(_msus, _errors));
        return _msus;
    }

    private List<(string msuPath, MsuType? msuType, string msuTypeDirectory)> GetMsusToLoad(Dictionary<string, string?> msuDirectories)
    {
        var msuLookups = new List<(string, MsuType?, string)>();
        
        foreach (var msuDirectory in msuDirectories)
        {
            if (!Directory.Exists(msuDirectory.Key)) continue;
            foreach (var msuFile in Directory.EnumerateFiles(msuDirectory.Key, "*.msu", SearchOption.AllDirectories))
            {
                var msuType = msuTypeService.GetMsuType(msuDirectory.Value);
                msuLookups.Add((msuFile, msuType, msuDirectory.Key));
            }  
        }

        return msuLookups;
    }

    private Dictionary<string, string> _yamlFiles = [];
    
    public Msu? LoadMsu(string msuPath, MsuType? msuTypeFilter = null, bool saveToCache = true, bool ignoreCache = false, bool forceLoad = false, string parentDirectory = "")
    {
        var directory = new FileInfo(msuPath).Directory!.FullName;

        if (!forceLoad && File.Exists(Path.Combine(directory, "msu-randomizer-output.txt")))
        {
            return null;
        }
        
        var msuDetails = msuDetailsService.GetMsuDetails(msuPath, out var yamlHash, out var yamlError);
        if (!string.IsNullOrEmpty(yamlError))
        {
            _errors[msuPath] = yamlError;
        }
        
        var baseName = Path.GetFileName(msuPath).Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
        var pcmFiles = Directory.EnumerateFiles(directory, $"{baseName}-*.pcm", SearchOption.AllDirectories).ToList();
        var msuDetailMismatch = false;

        if (msuDetails?.Tracks != null && pcmFiles.Count > 0)
        {
            var msuDetailSongCount = msuDetails.Tracks.Values.Sum(x => 1 + (x.Alts?.Count ?? 0)) * 0.75;
            if (pcmFiles.Count < msuDetailSongCount)
            {
                yamlHash = "";
                msuDetailMismatch = true;
            }
        }

        // Load the MSU from cache if possible
        if (!ignoreCache)
        {
            var cacheMsu = msuCacheService.Get(msuPath, yamlHash, pcmFiles);
            if (cacheMsu != null)
            {
                var cacheSettings = MsuUserOptions.GetMsuSettings(msuPath);
                ApplyMsuSettings(cacheMsu, cacheSettings);
                return cacheMsu;
            }
        }
        
        var msuSettings = MsuUserOptions.GetMsuSettings(msuPath);
        
        msuSettings.MsuType = msuTypeService.GetMsuType(msuSettings.MsuTypeName);

        // Try to load the MSU type specified by the MSU details if possible, otherwise try to auto detect
        var originalMsuType = !string.IsNullOrEmpty(msuDetails?.MsuType) 
            ? (msuTypeService.GetMsuType(msuDetails.MsuType) ?? GetMsuType(baseName, pcmFiles, msuTypeFilter))
            : GetMsuType(baseName, pcmFiles, msuTypeFilter);
        
        var msuType = msuSettings.MsuType ?? originalMsuType;
        
        logger.LogInformation("MSU {Name} found as MSU Type {Type}", baseName, msuType?.DisplayName ?? "Unknown");

        Msu msu;
        
        // If it's an unknown MSU type, simply load the details as is
        if (msuType == null || msuDetailMismatch)
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

        ApplyMsuSettings(msu, msuSettings);

        msu.MsuType = originalMsuType;
        msu.Version = msuDetails?.PackVersion;
        msu.Artist = msuDetails?.Artist;
        msu.Album = msuDetails?.Album;
        msu.Url = msuDetails?.Url;
        
        if (string.IsNullOrEmpty(parentDirectory))
        {
            var directoryInfo = new DirectoryInfo(directory);
            msu.ParentMsuTypeDirectory = directoryInfo.Parent?.FullName ?? directory;
        }
        else
        {
            msu.ParentMsuTypeDirectory = parentDirectory;
        }
        
        msuCacheService.Put(msu, yamlHash, pcmFiles, saveToCache);

        return msu;
    }

    public Msu LoadHardwareMsu(SnesFile snesMsu, IEnumerable<SnesFile> hardwarePcmFiles)
    {
        logger.LogInformation("{MsuFilePath} msu file path found", snesMsu.FullPath);
        
        var pcmFilePaths = hardwarePcmFiles.Select(x => x.FullPath).ToList();
        var baseName = Path.GetFileName(snesMsu.FullPath).Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
        var msuType = GetMsuType(baseName, pcmFilePaths);
        
        var msuSettings = MsuUserOptions.GetMsuSettings(snesMsu.FullPath);
        msuSettings.MsuType = msuTypeService.GetMsuType(msuSettings.MsuTypeName);
        if (msuSettings.MsuType != null)
        {
            msuType = msuSettings.MsuType;
            msuSettings.MsuTypeName = msuType.DisplayName;
        }

        if (msuSettings.MsuType != null)
        {
            msuType = msuSettings.MsuType;
        }
        
        var fullMsu = Msus.FirstOrDefault(x =>
            x.MsuTypeName == msuType?.DisplayName &&
            x.FolderName == snesMsu.ParentName && 
            x.FileName == snesMsu.Name.Replace(".msu", "", StringComparison.OrdinalIgnoreCase));

        if (fullMsu != null && fullMsu.ValidTracks.Count >= pcmFilePaths.Count)
        {
            logger.LogInformation("MSU {Name} found for {Path}", fullMsu.DisplayName, snesMsu.FullPath);
            return new Msu(
                type: fullMsu.MsuType, 
                name: fullMsu.DisplayName, 
                folderName: fullMsu.FolderName, 
                fileName: fullMsu.FileName, 
                path: snesMsu.FullPath, 
                tracks: fullMsu.Tracks.Where(x => !x.IsAlt).Select(x => new Track(x)).ToList(),
                msuDetails: null,
                prevMsu: fullMsu,
                isHardwareMsu: true)
            {
                Settings = msuSettings
            };
        }
        else
        {
            var msuDetails = msuDetailsService.GetMsuDetailsForPath(snesMsu.FullPath, msuType);
            
            if (msuDetails != null)
            {
                var msuDetailsType = msuTypeService.GetMsuType(msuDetails.MsuType) ?? msuType;

                if (msuDetailsType != null)
                {
                    logger.LogInformation("Yaml Details found for {Path}", snesMsu.FullPath);
                    var msu = msuDetailsService.ConvertToMsu(
                        msuDetails: msuDetails,
                        msuType: msuDetailsType,
                        msuPath: snesMsu.FullPath,
                        msuDirectory: new FileInfo(snesMsu.FullPath).Directory?.Name ?? msuDetails.PackName ?? "",
                        msuBaseName: Path.GetFileNameWithoutExtension(snesMsu.FullPath),
                        error: out _,
                        ignoreAlts: true,
                        pcmPaths: pcmFilePaths
                    );

                    if (msu != null)
                    {
                        msu.IsHardwareMsu = true;
                        msu.Settings = msuSettings;
                        return msu;
                    }
                }
            }
        }
        
        logger.LogInformation("No MSU found for {Path}", snesMsu.FullPath);

        if (msuType == null)
        {
            logger.LogInformation("Unknown MSU {Path} found", snesMsu.FullPath);
            var msu = LoadUnknownMsu(snesMsu.FullPath, snesMsu.FullPath.Replace(snesMsu.Name, ""), baseName, pcmFilePaths);
            msu.IsHardwareMsu = true;
            msu.Settings = msuSettings;
            return msu;
        }
        else
        {
            logger.LogInformation("{MsuType} MSU {Path} found", msuType.DisplayName, snesMsu.FullPath);
            var msu = LoadBasicMsu(snesMsu.FullPath, snesMsu.FullPath.Replace(snesMsu.Name, ""), baseName, msuType, pcmFilePaths, null);
            msu.IsHardwareMsu = true;
            msu.Settings = msuSettings;
            return msu;
        }
    }

    public IReadOnlyCollection<Msu> Msus => _msus.ToList();

    private void ApplyMsuSettings(Msu msu, MsuSettings msuSettings)
    {
        msu.Settings = msuSettings;

        if (!(msuSettings.IsCopyrightSafeOverrides?.Count > 0))
        {
            msu.AreAllTracksCopyrightSafe = msu.Tracks.All(x => x.IsCopyrightSafeCombined == true);
            return;
        }
        
        foreach (var overrideDetails in msuSettings.IsCopyrightSafeOverrides)
        {
            var track = msu.Tracks.FirstOrDefault(x => x.Path == overrideDetails.Key);
            if (track != null)
            {
                track.IsCopyrightSafeOverride = overrideDetails.Value;
            }
        }

        msu.AreAllTracksCopyrightSafe = msu.Tracks.All(x => x.IsCopyrightSafeCombined == true);
    }
    
    private Msu LoadDetailedMsu(string msuPath, string directory, string baseName, MsuType msuType, IEnumerable<string> pcmFiles, MsuDetails basicDetails)
    {
        var msu = msuDetailsService.ConvertToMsu(basicDetails, msuType, msuPath, directory, baseName, out var yamlError);
        
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
        if (directory.EndsWith('/') || directory.EndsWith('\\'))
        {
            directory = directory.Substring(0, directory.Length - 1);
        }
        
        var tracks = pcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "", StringComparison.OrdinalIgnoreCase).Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(x => new Track
                (
                    trackName: $"Track #{x}",
                    number: int.Parse(x),
                    songName: $"Track #{x}",
                    path: $"{directory}{Path.DirectorySeparatorChar}{baseName}-{x}.pcm",
                    isBaseFile: true
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
        if (directory.EndsWith('/') || directory.EndsWith('\\'))
        {
            directory = directory.Substring(0, directory.Length - 1);
        }

        var allPcmFiles = pcmFiles.ToList();
        
        var trackNumbers = allPcmFiles
            .Select(x =>
                Path.GetFileName(x).Replace($"{baseName}-", "", StringComparison.OrdinalIgnoreCase).Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();

        var basicPcmFiles = trackNumbers.Select(x => Path.Combine(directory, $"{baseName}-{x}.pcm")).ToList();
        var extraPcmFiles = allPcmFiles.Where(x => !basicPcmFiles.Contains(x)).ToList();
        
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

            var path = allPcmFiles.FirstOrDefault(x =>
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
                path: path,
                isBaseFile: true
            ){
                IsCopied = trackNumber != track.Number
            });

            // See if there are any alt tracks to add
            var alts = extraPcmFiles.Where(x => x != path && Regex.IsMatch(x.Replace(Path.Combine(directory, baseName), ""), $"-{trackNumber}[^0-9]")).ToList();
            if (!alts.Any()) continue;
            var altIndex = 1;
            
            foreach (var alt in alts)
            {
                tracks.Add(new Track
                (
                    trackName: msuType.Tracks.FirstOrDefault(x => x.Number == track.Number)?.Name ?? $"Track #{trackNumber}",
                    number: track.Number,
                    songName: $"Track #{trackNumber} (alt {altIndex})",
                    path: alt,
                    isAlt: true,
                    isBaseFile: false
                ){
                    IsCopied = trackNumber != track.Number
                });

                altIndex++;
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
                Path.GetFileName(x).Replace($"{baseName}-", "", StringComparison.OrdinalIgnoreCase).Replace(".pcm", "", StringComparison.OrdinalIgnoreCase))
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .ToHashSet();
        
        var matchingMsus = new List<(MsuType Type, float PrimaryConfidence, int PrimaryCount, float SecondaryConfidence, int SecondaryCount)>();
        
        var allowedMsuTypes = msuTypeService.MsuTypes.ToList();
        if (msuTypeFilter != null && !msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(msuTypeFilter.DisplayName))
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
            .OrderByDescending(x => msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(x.Type.DisplayName) || msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(x.Type.Name))
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
    
    public event EventHandler? OnMsuLookupStarted;
    
    public event EventHandler<MsuListEventArgs>? OnMsuLookupComplete;
    
    
    public void RefreshMsu(Msu msu)
    {
        Status = MsuLoadStatus.Loading;
        _errors.Clear();
        
        // Reload the MSU
        var newMsu = LoadMsu(msu.Path, parentDirectory: msu.ParentMsuTypeDirectory);
        if (newMsu == null)
        {
            logger.LogInformation("Reloaded MSU for Path {Path} returned null", msu.Path);
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