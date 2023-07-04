using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary.Services;

public class MsuSelectorService : IMsuSelectorService
{
    private readonly ILogger<MsuSelectorService> _logger;
    private readonly Random _random;
    private readonly IMsuDetailsService _msuDetailsService;

    public MsuSelectorService(ILogger<MsuSelectorService> logger, IMsuDetailsService msuDetailsService)
    {
        _logger = logger;
        _msuDetailsService = msuDetailsService;
        _random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        for (int i = 0; i < 10; i++)
        {
            _random.Next();
        }
    }
    
    public Msu AssignMsu(Msu msu, MsuType msuType, string outputPath)
    {
        var convertedMsu = ConvertMsu(msu, msuType);
        return SaveMsu(convertedMsu, outputPath);
    }

    public Msu PickRandomMsu(ICollection<Msu> msus, MsuType msuType, string outputPath)
    {
        if (!msus.Any())
        {
            throw new InvalidOperationException("No MSUs were passed in");
        }
        var convertedMsu = ConvertMsus(msus, msuType).Random(_random)!;
        return SaveMsu(convertedMsu, outputPath);
    }

    public Msu CreateShuffledMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, Msu? prevMsu = null)
    {
        var convertedMsu = ConvertMsus(msus, msuType);
        var tracks = convertedMsu.SelectMany(x => x.ValidTracks).ToList();
        var selectedTracks = new List<Track>();
        foreach (var msuTypeTrack in msuType.Tracks)
        {
            if (selectedTracks.Any(x => x.Number == msuTypeTrack.Number))
            {
                continue;
            }
            var track = tracks.Where(x => x.Number == msuTypeTrack.Number).Random(_random);
            if (track == null)
            {
                continue;
            }
            selectedTracks.Add(track);

            if (msuTypeTrack.PairedTrack > 0 && selectedTracks.All(x => x.Number != msuTypeTrack.PairedTrack))
            {
                var pairedTrack = tracks.Where(x => x.Number == msuTypeTrack.PairedTrack && x.MsuPath == track.MsuPath).Random(_random);
                if (pairedTrack != null)
                {
                    selectedTracks.Add(pairedTrack);
                }
            }
        }

        return SaveMsu(new Msu()
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
            .Select(x => $"{x.Number}: {x.GetDisplayText()}");
        
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
}