using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public class MsuSelectorService : IMsuSelectorService
{
    private readonly ILogger<MsuSelectorService> _logger;
    private readonly Random _random;
    private readonly IMsuDetailsService _msuDetailsService;
    private readonly IMsuTypeService _msuTypeService;
    private readonly IMsuLookupService _msuLookupService;
    private readonly IMsuUserOptionsService _msuUserOptionsService;

    public MsuSelectorService(ILogger<MsuSelectorService> logger, IMsuDetailsService msuDetailsService, IMsuTypeService msuTypeService, IMsuLookupService msuLookupService, IMsuUserOptionsService msuUserOptionsService)
    {
        _logger = logger;
        _msuDetailsService = msuDetailsService;
        _msuTypeService = msuTypeService;
        _msuLookupService = msuLookupService;
        _msuUserOptionsService = msuUserOptionsService;
        _random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        for (var i = 0; i < 10; i++)
        {
            _random.Next();
        }
    }
    
    public Msu AssignMsu(Msu msu, MsuType msuType, string outputPath)
    {
        var convertedMsu = ConvertMsu(msu, msuType);
        return SaveMsu(convertedMsu, outputPath);
    }

    public Msu PickRandomMsu(bool emptyFolder = true)
    {
        var msuUserOptions = _msuUserOptionsService.MsuUserOptions;
        
        if (msuUserOptions.SelectedMsus == null || !msuUserOptions.SelectedMsus.Any())
        {
            _logger.LogError("No selected MSUs");
            throw new InvalidOperationException("No selected MSUs");
        }
        
        if (string.IsNullOrEmpty(msuUserOptions.OutputMsuType))
        {
            _logger.LogError("No output MSU type selected");
            throw new InvalidOperationException("No output MSU type selected");
        }
        
        var msus = _msuLookupService.Msus
            .Where(x => msuUserOptions.SelectedMsus?.Contains(x.Path) == true)
            .ToList();

        if (!msus.Any())
        {
            _logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        var msuType = _msuTypeService.GetMsuType(msuUserOptions.OutputMsuType);

        if (msuType == null)
        {
            _logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }
        
        var path = !string.IsNullOrEmpty(msuUserOptions.OutputFolderPath) 
            ? Path.Combine(msuUserOptions.OutputFolderPath, $"{msuUserOptions.Name}.msu") 
            : msuUserOptions.OutputRomPath ?? "";

        return PickRandomMsu(msus, msuType, path, emptyFolder, msuUserOptions.OpenFolderOnCreate);
    }

    public Msu PickRandomMsu(ICollection<string> msuPaths, string msuTypeName, string outputPath, bool emptyFolder = true,
        bool openFolder = false)
    {
        if (msuPaths.Any())
        {
            _logger.LogError("No selected MSUs");
            throw new InvalidOperationException("No selected MSUs");
        }
        
        var msus = _msuLookupService.Msus
            .Where(x => msuPaths.Contains(x.Path))
            .ToList();

        if (!msus.Any())
        {
            _logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        var msuType = _msuTypeService.GetMsuType(msuTypeName);

        if (msuType == null)
        {
            _logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }

        return PickRandomMsu(msus, msuType, outputPath, emptyFolder, openFolder);
    }

    public Msu PickRandomMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, bool emptyFolder = true, bool openFolder = false)
    {
        if (!msus.Any())
        {
            throw new InvalidOperationException("No MSUs were passed in");
        }
        
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("No output path specified");
        }
        
        var convertedMsu = ConvertMsus(msus, msuType).Random(_random)!;
        if (emptyFolder)
        {
            EmptyFolder(Path.GetDirectoryName(outputPath)!);
        }
        
        var msu = SaveMsu(convertedMsu, outputPath);
        
        if (openFolder)
        {
            OpenMsuDirectory(msu);
        }

        return msu;
    }

    public Msu CreateShuffledMsu(Msu? prevMsu = null, bool emptyFolder = true)
    {
        var msuUserOptions = _msuUserOptionsService.MsuUserOptions;
        
        if (msuUserOptions.SelectedMsus == null || !msuUserOptions.SelectedMsus.Any())
        {
            _logger.LogError("No selected MSUs");
            throw new InvalidOperationException("No selected MSUs");
        }
        
        if (string.IsNullOrEmpty(msuUserOptions.OutputMsuType))
        {
            _logger.LogError("No output MSU type selected");
            throw new InvalidOperationException("No output MSU type selected");
        }
        
        var msus = _msuLookupService.Msus
            .Where(x => msuUserOptions.SelectedMsus?.Contains(x.Path) == true)
            .ToList();
        
        if (!msus.Any())
        {
            _logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        var msuType = _msuTypeService.GetMsuType(msuUserOptions.OutputMsuType);

        if (msuType == null)
        {
            _logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }
        
        var path = !string.IsNullOrEmpty(msuUserOptions.OutputFolderPath) 
            ? Path.Combine(msuUserOptions.OutputFolderPath, $"{msuUserOptions.Name}.msu") 
            : msuUserOptions.OutputRomPath ?? "";

        return CreateShuffledMsu(msus, msuType, path, prevMsu, emptyFolder, msuUserOptions.AvoidDuplicates, msuUserOptions.OpenFolderOnCreate);
    }

    public Msu CreateShuffledMsu(ICollection<string> msuPaths, string msuTypeName, string outputPath,
        Msu? prevMsu = null, bool emptyFolder = true, bool avoidDuplicates = true, bool openFolder = false)
    {
        if (msuPaths.Any())
        {
            _logger.LogError("No selected MSUs");
            throw new InvalidOperationException("No selected MSUs");
        }
        
        var msus = _msuLookupService.Msus
            .Where(x => msuPaths.Contains(x.Path))
            .ToList();

        if (!msus.Any())
        {
            _logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        var msuType = _msuTypeService.GetMsuType(msuTypeName);

        if (msuType == null)
        {
            _logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }

        return CreateShuffledMsu(msus, msuType, outputPath, prevMsu, emptyFolder, avoidDuplicates, openFolder);
    }

    public Msu CreateShuffledMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, Msu? prevMsu = null, bool emptyFolder = true, bool avoidDuplicates = true, bool openFolder = false)
    {
        if (!msus.Any())
        {
            throw new InvalidOperationException("No MSUs were passed in");
        }
        
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("No output path specified");
        }
        
        var convertedMsu = ConvertMsus(msus, msuType);
        var tracks = convertedMsu.SelectMany(x => x.ValidTracks).ToList();
        var selectedTracks = new List<Track>();
        var selectedPaths = new List<string>();
        foreach (var msuTypeTrack in msuType.Tracks)
        {
            if (selectedTracks.Any(x => x.Number == msuTypeTrack.Number))
            {
                continue;
            }
            var possibleTracks = tracks.Where(x => x.Number == msuTypeTrack.Number);
            if (avoidDuplicates && possibleTracks.Any(x => !selectedPaths.Contains(x.Path)))
            {
                possibleTracks = possibleTracks.Where(x => !selectedPaths.Contains(x.Path));
            }

            var track = possibleTracks.Random(_random);
            if (track == null)
            {
                continue;
            }
            selectedTracks.Add(track);
            selectedPaths.Add(track.Path);

            if (msuTypeTrack.PairedTrack > 0 && selectedTracks.All(x => x.Number != msuTypeTrack.PairedTrack))
            {
                var pairedTrack = tracks.Where(x => x.Number == msuTypeTrack.PairedTrack && x.MsuPath == track.MsuPath).Random(_random);
                if (pairedTrack != null)
                {
                    selectedTracks.Add(pairedTrack);
                    selectedPaths.Add(pairedTrack.Path);
                }
            }
        }
        
        if (emptyFolder)
        {
            EmptyFolder(Path.GetDirectoryName(outputPath)!);
        }

        var msu = SaveMsu(new Msu()
        {
            MsuType = msuType,
            Name = "Shuffled MSU",
            Creator = "Various Creators",
            FileName = "shuffled-msu",
            FolderName = "",
            Path = "",
            Tracks = selectedTracks,
            Settings = new MsuSettings()
            {
                MsuPath = ""
            }
        }, outputPath, prevMsu);

        if (openFolder)
        {
            OpenMsuDirectory(msu);
        }

        return msu;
    }

    public Msu SaveMsu(Msu msu, string outputPath, Msu? prevMsu = null)
    {
        string outputDirectory;
        string outputFilename;
        
        if (string.IsNullOrEmpty(Path.GetExtension(outputPath)))
        {
            outputDirectory = outputPath;
            outputFilename = msu.FileName;
        }
        else
        {
            var file = new FileInfo(outputPath);
            outputDirectory = file.DirectoryName!;
            outputFilename = file.Name.Replace(file.Extension, "");
        }

        if (string.IsNullOrWhiteSpace(outputDirectory) || string.IsNullOrWhiteSpace(outputFilename))
        {
            throw new InvalidOperationException("Missing output directory or filename");
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var msuPath = outputDirectory + Path.DirectorySeparatorChar + outputFilename + ".msu";
        if (!File.Exists(msuPath))
        {
            using (File.Create(msuPath)) {}
        }

        var tracks = new List<Track>();
        var allowAltTracks = msu.Settings.AllowAltTracks;

        foreach (var trackNumber in msu.Tracks.Select(x => x.Number))
        {
            var track = allowAltTracks ? msu.Tracks.Where(x => x.Number == trackNumber).Random(_random)! : msu.Tracks.FirstOrDefault(x => x.Number == trackNumber && !x.IsAlt);
            track ??= msu.Tracks.First(x => x.Number == trackNumber);
            var source = track.Path;
            var destination = $"{outputDirectory}{Path.DirectorySeparatorChar}{outputFilename}-{trackNumber}.pcm";
            var trackName = msu.MsuType?.Tracks.FirstOrDefault(x => x.Number == trackNumber)?.Name ?? track.TrackName;
            try
            {
                CreatePcmFile(source, destination);
                _logger.LogInformation("Created PCM {Source} => {Destination}", source, destination);
                tracks.Add(new Track(track, number: trackNumber, path: destination, trackName: trackName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                track = prevMsu?.Tracks.FirstOrDefault(x => x.Path == destination);
                if (track != null)
                {
                    tracks.Add(new Track(track));
                    _logger.LogInformation("Used previous PCM track {Destination}", destination);
                }
                else
                {
                    _logger.LogWarning("Could not create PCM {Source} => {Destination}", source, destination);
                }
            }
        }

        var outputMsu = new Msu
        {
            MsuType = msu.MsuType,
            Creator = msu.Creator,
            Name = msu.Name,
            FileName = new FileInfo(msuPath).Name,
            FolderName = new DirectoryInfo(outputDirectory).Name,
            Path = msuPath,
            Tracks = tracks,
            Settings = new MsuSettings()
            {
                MsuPath = msuPath
            }
        }
        ;
        WriteTrackFile(tracks, $"{outputDirectory}{Path.DirectorySeparatorChar}msu-randomizer-output.txt");
        _msuDetailsService.SaveMsuDetails(outputMsu, msuPath.Replace(".msu", ".yml"));

        return outputMsu;
    }

    public ICollection<Msu> ConvertMsus(ICollection<Msu> msus, MsuType msuType)
    {
        return msus.Select(x => ConvertMsu(x, msuType)).ToList();
    }

    public Msu ConvertMsu(Msu msu, MsuType msuType)
    {
        if (msu.MsuTypeName == msuType.Name && msu.Settings.AllowAltTracks)
        {
            return msu;
        }

        var conversion = msu.MsuType != null && msuType.Conversions.ContainsKey(msu.MsuType!)
            ? msuType.Conversions[msu.MsuType]
            : x => x;

        var trackNumbers = msuType.ValidTrackNumbers;

        var newTracks = msu.Tracks.Select(x => new Track(x, number: conversion(x.Number)))
            .Where(x => trackNumbers.Contains(x.Number) && (msu.Settings.AllowAltTracks || !x.IsAlt))
            .ToList();

        return new Msu()
        {
            MsuType = msuType,
            Name = msu.Name,
            Creator = msu.Creator,
            FileName = msu.FileName,
            FolderName = msu.FolderName,
            Path = msu.Path,
            Settings = msu.Settings,
            Tracks = newTracks
        };
    }

    private static void WriteTrackFile(ICollection<Track> tracks, string path)
    {
        var output = tracks
            .OrderBy(x => x.Number)
            .Select(x => $"{x.Number}: {x.OriginalPath}");
        
        File.WriteAllLines(path, output);
    }
    
    private static void CreatePcmFile(string source, string destination)
    {
        if (File.Exists(destination))
        {
            try
            {
                File.Delete(destination);
            }
            catch
            {
                throw new InvalidOperationException($"File {destination} could not be deleted");
            }
        }
        
        if (destination.StartsWith(source.Substring(0,1)))
            NativeMethods.CreateHardLink(destination, source, IntPtr.Zero);
        else
            File.Copy(source, destination);
    }

    private void EmptyFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }
        
        foreach(var filePath in Directory.EnumerateFiles(path, "*.msu").Concat(Directory.EnumerateFiles(path, "*.pcm")))
        {
            try
            {
                File.Delete(filePath);
                if (filePath.EndsWith(".msu") && File.Exists(filePath.Replace(".msu", ".yml")))
                {
                    File.Delete(filePath.Replace(".msu", ".yml"));
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to delete file {Path}", filePath);
            }
        }
    }

    private void OpenMsuDirectory(Msu msu)
    {
        if (!File.Exists(msu.Path)) return;
        var directory = new FileInfo(msu.Path).DirectoryName;
        if (Directory.Exists(directory))
            Process.Start("explorer.exe", directory);
    }
}