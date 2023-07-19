using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

internal class MsuTypeService : IMsuTypeService
{
    private readonly ILogger<MsuTypeService> _logger;
    private readonly List<MsuType> _msuTypes = new();
    private readonly MsuAppSettings _msuAppSettings;

    public MsuTypeService(ILogger<MsuTypeService> logger, MsuAppSettings randomizerMsuAppSettings)
    {
        _logger = logger;
        _msuAppSettings = randomizerMsuAppSettings;
    }

    public void LoadMsuTypes(Stream stream)
    {
        _msuTypes.Clear();
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        var contents = reader.ReadToEnd();
        
        var configs = JsonSerializer.Deserialize<List<MsuTypeConfig>>(contents);
        
        if (configs == null)
        {
            throw new InvalidOperationException("Unable to parse MSU types from stream");
        }

        foreach (var data in configs)
        {
            data.DetermineBasicTracksNumbers();
        }
        
        FinalizeConfigs(configs);
    }

    public IReadOnlyCollection<MsuType> MsuTypes => _msuTypes;

    public void LoadMsuTypes(string directory)
    {
        _msuTypes.Clear();
        
        if (!Directory.Exists(directory))
        {
            throw new FileNotFoundException($"MSU type directory {directory} not found");
        }
        
        var configs = new List<MsuTypeConfig>();
        foreach (var file in Directory.EnumerateFiles(directory, "tracks.json", SearchOption.AllDirectories))
        {
            // Get path to this config for purposes of copying
            var folderPath = Path.GetRelativePath(directory, file.Replace("tracks.json", "").Replace("manifests" + Path.DirectorySeparatorChar, ""));
            if (folderPath.EndsWith(Path.DirectorySeparatorChar))
            {
                folderPath = folderPath.Substring(0, folderPath.Length - 1);
            }
            folderPath = folderPath.Replace("\\", "/");
            
            // Parse JSON file
            var contents = File.ReadAllText(file);
            var data = JsonSerializer.Deserialize<MsuTypeConfig>(contents);
            if (data == null)
            {
                _logger.LogError("Unable to parse config file {File}", file);
                continue;
            }

            data.Path = folderPath;
            data.DetermineBasicTracksNumbers();
            configs.Add(data);
        }

        FinalizeConfigs(configs);
    }

    public string GetMsuTypeName(MsuType? msuType)
    {
        return msuType?.DisplayName ?? "Unknown";
    }
    
    public MsuType? GetMsuType(string? name)
    {
        return MsuTypes.FirstOrDefault(x => x.DisplayName == name || x.Name == name);
    }

    private void FinalizeConfigs(IEnumerable<MsuTypeConfig> configs)
    {
        foreach (var config in configs)
        {
            // Copy tracks from other configs
            if (config.CanCopy)
            {
                config.ApplyCopiedTracks(configs);
            }

            var type = ConvertMsuTypeConfig(config);
            _msuTypes.Add(type);
            _logger.LogInformation("MSU type {ConfigName} found with {TrackCount} tracks", config.Meta.Name, config.FullTrackList.Count);
        }
        
        SetupConversions(configs);
    }

    private void SetupConversions(IEnumerable<MsuTypeConfig> configs)
    {
        // Create conversions from the copy part of the configs
        foreach (var config in configs.Where(x => x.CanCopy))
        {
            var msuName = _msuAppSettings.GetMsuName(config.Name);
            var msuType = _msuTypes.First(x => x.DisplayName == msuName);
            foreach (var copyDetails in config.Copy!)
            {
                var otherConfig = configs.First(x => x.Path == copyDetails.Msu || x.Name == copyDetails.Msu);
                var otherMsuType = _msuTypes.First(x => x.DisplayName == _msuAppSettings.GetMsuName(otherConfig.Name));
                msuType.Conversions[otherMsuType] = x => x + copyDetails.Modifier;
                otherMsuType.Conversions[msuType] = x => x - copyDetails.Modifier;
            }

            foreach (var exactMatch in config.Meta.ExactMatches)
            {
                var otherConfig = configs.First(x => x.Path == exactMatch || x.Name == exactMatch);
                var otherMsuType = _msuTypes.First(x => x.DisplayName == _msuAppSettings.GetMsuName(otherConfig.Name));
                msuType.ExactMatches.Add(otherMsuType);
                otherMsuType.ExactMatches.Add(msuType);
            }
        }

        var smz3 = _msuTypes.FirstOrDefault(x => x.DisplayName == _msuAppSettings.Smz3MsuTypeName);
        var smz3Old = _msuTypes.FirstOrDefault(x => x.DisplayName == _msuAppSettings.Smz3LegacyMsuTypeName);

        if (smz3 != null && smz3Old != null)
        {
            smz3.Conversions[smz3Old] = x =>
            {
                return x switch
                {
                    < 99 => x + 100,
                    > 99 => x - 100,
                    _ => x
                };
            };
        
            smz3Old.Conversions[smz3] = x =>
            {
                return x switch
                {
                    < 99 => x + 100,
                    > 99 => x - 100,
                    _ => x
                };
            };
        }
    }

    private MsuType ConvertMsuTypeConfig(MsuTypeConfig config)
    {
        var tracks = config.FullTrackList.Where(x => !x.IsUnused).Select(x => new MsuTypeTrack()
        {
            Name = string.IsNullOrWhiteSpace(x.Title) ? x.Name : x.Title,
            Number = x.Num,
            Fallback = x.Fallback,
            PairedTrack = x.PairedTrack,
            IsExtended = x.IsExtended,
            NonLooping = x.NonLooping,
            IsIgnored = x.IsIgnored || x.IsExtended
        });

        return new MsuType()
        {
            Name = config.Name,
            DisplayName = _msuAppSettings.GetMsuName(config.Name),
            Selectable = config.Meta.Selectable != false,
            Tracks = tracks,
            RequiredTrackNumbers = tracks.Where(x => !x.IsIgnored).Select(x => x.Number).ToHashSet(),
            ValidTrackNumbers = tracks.Select(x => x.Number).ToHashSet()
        };
    }
}