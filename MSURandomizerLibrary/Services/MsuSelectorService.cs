using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;

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
    
    #region Public Methods
    public MsuSelectorResponse AssignMsu(MsuSelectorRequest request)
    {
        var validateResponse = ValidateRequest(request, validateSingleMsu: true, validateMsuType: true, validateOutputPath: true);
        if (validateResponse != null) return validateResponse;
        var convertedMsu = InternalConvertMsu(request.Msu!, request.OutputMsuType!);
        return SaveMsuInternal(convertedMsu, request.OutputPath!, null, request.EmptyFolder, request.OpenFolder!.Value);
    }

    public MsuSelectorResponse PickRandomMsu(MsuSelectorRequest request)
    {
        var validateResponse = ValidateRequest(request, validateMultipleMsus: true, validateMsuType: true, validateOutputPath: true);
        if (validateResponse != null) return validateResponse;

        var convertedMsu = InternalConvertMsus(request.Msus!, request.OutputMsuType!).Random(_random);
        
        if (convertedMsu == null)
        {
            return new MsuSelectorResponse()
            {
                Successful = false,
                Message = "Could not pick random MSU"
            };
        }
        
        return SaveMsuInternal(convertedMsu, request.OutputPath!, null, request.EmptyFolder, request.OpenFolder);
    }

    public MsuSelectorResponse CreateShuffledMsu(MsuSelectorRequest request)
    {
        var validateResponse = ValidateRequest(request, validateMultipleMsus: true, validateMsuType: true, validateOutputPath: true);
        if (validateResponse != null) return validateResponse;
        
        ICollection<Msu> msus = request.Msus!;
        MsuType msuType = request.OutputMsuType!;
        string outputPath = request.OutputPath!;
        
        var convertedMsu = InternalConvertMsus(msus, msuType);
        var tracks = convertedMsu.SelectMany(x => x.ValidTracks).ToList();
        var selectedTracks = new List<Track>();
        var selectedPaths = new List<string>();
        foreach (var msuTypeTrack in msuType.Tracks.OrderBy(x => x.Number))
        {
            if (selectedTracks.Any(x => x.Number == msuTypeTrack.Number))
            {
                continue;
            }
            var possibleTracks = tracks.Where(x => x.Number == msuTypeTrack.Number);
            if (request.AvoidDuplicates == true && possibleTracks.Any(x => !selectedPaths.Contains(x.Path)))
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
        
        var outputMsu = new Msu
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
        };
        
        return SaveMsuInternal(outputMsu, outputPath, request.PrevMsu, request.EmptyFolder, request.OpenFolder);
    }

    public MsuSelectorResponse SaveMsu(MsuSelectorRequest request)
    {
        var validateResponse = ValidateRequest(request, validateSingleMsu: true, validateOutputPath: true);
        if (validateResponse != null) return validateResponse;
        return SaveMsuInternal(request.Msu!, request.OutputPath!, request.PrevMsu, request.EmptyFolder, request.OpenFolder);
    }
    
    public MsuSelectorResponse ConvertMsus(MsuSelectorRequest request)
    {
        var validateResponse = ValidateRequest(request, validateMultipleMsus: true, validateMsuType: true);
        if (validateResponse != null) return validateResponse;
        return new MsuSelectorResponse()
        {
            Successful = true,
            Msus = InternalConvertMsus(request.Msus!, request.OutputMsuType!)
        };
    }
    
    public MsuSelectorResponse ConvertMsu(MsuSelectorRequest request)
    {
        var validateResponse = ValidateRequest(request, validateSingleMsu: true, validateMsuType: true);
        if (validateResponse != null) return validateResponse;
        return new MsuSelectorResponse()
        {
            Successful = true,
            Msu = InternalConvertMsu(request.Msu!, request.OutputMsuType!)
        };
    }
    #endregion Public methods

    #region Private Methods
    private MsuSelectorResponse SaveMsuInternal(Msu msu, string outputPath, Msu? prevMsu, bool emptyFolder, bool? openFolder)
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

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        
        if (emptyFolder)
        {
            EmptyFolder(outputDirectory);
        }

        var msuPath = outputDirectory + Path.DirectorySeparatorChar + outputFilename + ".msu";
        if (!File.Exists(msuPath))
        {
            using (File.Create(msuPath)) {}
        }

        var selectedPaths = new Dictionary<int, string>();
        var tracks = new List<Track>();
        var allowAltTracks = msu.Settings.AllowAltTracks;
        string? warningMessage = null;

        foreach (var trackNumber in msu.Tracks.Select(x => x.Number).Order())
        {
            var msuTypeTrack = msu.SelectedMsuType?.Tracks.FirstOrDefault(x => x.Number == trackNumber);
            var track = allowAltTracks ? msu.Tracks.Where(x => x.Number == trackNumber).Random(_random)! : msu.Tracks.FirstOrDefault(x => x.Number == trackNumber && !x.IsAlt);
            track ??= msu.Tracks.First(x => x.Number == trackNumber);
            var source = track.Path;
            var destination = $"{outputDirectory}{Path.DirectorySeparatorChar}{outputFilename}-{trackNumber}.pcm";
            var trackName = msuTypeTrack?.Name ?? track.TrackName;

            // If the fallback to the track is the same file, we can skip it
            if (msuTypeTrack?.Fallback >= 0 && selectedPaths.TryGetValue(msuTypeTrack.Fallback, out var fallbackPath) && fallbackPath == source)
            {
                selectedPaths[trackNumber] = source;
                continue;
            }
            
            try
            {
                if (CreatePcmFile(source, destination))
                {
                    _logger.LogInformation("Created PCM {Source} => {Destination}", source, destination);
                    tracks.Add(new Track(track, number: trackNumber, path: destination, trackName: trackName));
                    selectedPaths[trackNumber] = source;
                }
                else
                {
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to save PCM file {Source} to {Destination}", source, destination);
                warningMessage = "One or more PCM files failed to save. Verify space and permissions of destination.";
            }
        }

        var outputMsu = new Msu
        {
            MsuType = msu.SelectedMsuType,
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
        };

        if (!WriteTrackFile(tracks, $"{outputDirectory}{Path.DirectorySeparatorChar}msu-randomizer-output.txt"))
        {
            warningMessage ??= "MSU generated successfully but unable to save msu-randomizer-output.txt file";
        }

        if (!_msuDetailsService.SaveMsuDetails(outputMsu, msuPath.Replace(".msu", ".yml"), out var error))
        {
            warningMessage ??= "MSU generated successfully but unable to save MSU details YAML file";
        }
        
        var response = ValidateMsu(outputMsu);

        if (response.Successful && openFolder == true)
        {
            OpenMsuDirectory(outputMsu);
        }

        if (warningMessage != null && string.IsNullOrWhiteSpace(response.Message))
        {
            response.Message = warningMessage;
        }

        return response;
    }

    private ICollection<Msu> InternalConvertMsus(ICollection<Msu> msus, MsuType msuType)
    {
        return msus.Select(x => InternalConvertMsu(x, msuType)).ToList();
    }

    private Msu InternalConvertMsu(Msu msu, MsuType msuType)
    {
        if (msu.MsuTypeName == msuType.DisplayName && !msu.Settings.AllowAltTracks)
        {
            return msu;
        }

        var conversion = msu.SelectedMsuType != null && msuType.Conversions.ContainsKey(msu.SelectedMsuType!)
            ? msuType.Conversions[msu.SelectedMsuType]
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

    private bool WriteTrackFile(ICollection<Track> tracks, string path)
    {
        try
        {
            var output = tracks
                .OrderBy(x => x.Number)
                .Select(x => $"{x.Number}: {x.OriginalPath}");
        
            File.WriteAllLines(path, output);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to write msu-randomizer.output.txt file to {Path}", path);
            return false;
        }
        
    }
    
    private bool CreatePcmFile(string source, string destination)
    {
        if (File.Exists(destination))
        {
            try
            {
                File.Delete(destination);
            }
            catch
            {
                _logger.LogInformation("File {Destination} could not be deleted", destination);
                return false;
            }
        }
        
        if (destination.StartsWith(source.Substring(0,1)))
            NativeMethods.CreateHardLink(destination, source, IntPtr.Zero);
        else
            File.Copy(source, destination);

        return true;
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

    private MsuSelectorResponse ValidateMsu(Msu msu)
    {
        if (msu.MsuType == null)
        {
            return new MsuSelectorResponse()
            {
                Successful = true,
                Msu = msu
            };
        }

        var message = "";
        
        var matchingRequiredTracks = msu.Tracks.Select(x => x.Number).Intersect(msu.MsuType.RequiredTrackNumbers).Count();
        var percentRequiredTracks = 1.0f * matchingRequiredTracks / msu.MsuType.RequiredTrackNumbers.Count;
        if (percentRequiredTracks < .9)
        {
            message = "MSU generated successfully but multiple required tracks are missing.";
        }

        return new MsuSelectorResponse()
        {
            Successful = true,
            Message = message,
            Msu = msu
        };
    }

    private MsuSelectorResponse? ValidateRequest(MsuSelectorRequest request, bool validateOutputPath = false, 
        bool validateMsuType = false, bool validateSingleMsu = false, bool validateMultipleMsus = false)
    {
        var userOptions = _msuUserOptionsService.MsuUserOptions;
        
        if (validateOutputPath)
        {
            request.OutputPath ??= userOptions.OutputRomPath ??
                                   $"{userOptions.OutputFolderPath}{Path.DirectorySeparatorChar}{userOptions.Name}.msu";
                
            if (string.IsNullOrWhiteSpace(request.OutputPath))
            {
                return new MsuSelectorResponse()
                {
                    Successful = false,
                    Message = "Invalid output path"
                };
            }
        }

        if (validateMsuType)
        {
            request.OutputMsuType ??= _msuTypeService.GetMsuType(request.OutputMsuTypeName ?? userOptions.OutputMsuType);
            if (request.OutputMsuType == null)
            {
                return new MsuSelectorResponse()
                {
                    Successful = false,
                    Message = "No valid MSU type passed in"
                };
            }
        }
        
        if (validateSingleMsu)
        {
            request.Msu ??= _msuLookupService.GetMsuByPath(request.MsuPath ?? userOptions.SelectedMsus?.FirstOrDefault());
            if (request.Msu == null)
            {
                return new MsuSelectorResponse()
                {
                    Successful = false,
                    Message = "No valid MSU passed in"
                };
            }
        }

        if (validateMultipleMsus)
        {
            request.Msus ??= _msuLookupService.GetMsusByPath(request.MsuPaths ?? userOptions.SelectedMsus);
            if (request.Msus?.Any() != true)
            {
                return new MsuSelectorResponse()
                {
                    Successful = false,
                    Message = "No valid MSUs passed in"
                };
            }
        }

        request.OpenFolder ??= userOptions.OpenFolderOnCreate;
        request.AvoidDuplicates ??= userOptions.AvoidDuplicates;

        return null;
    }
    #endregion Private Methods
}