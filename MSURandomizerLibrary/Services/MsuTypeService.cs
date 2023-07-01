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

    public MsuTypeService(ILogger<MsuTypeService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<MsuType> MsuTypes => _msuTypes;

    public void LoadMsuTypes(string directory)
    {
        _msuTypes.Clear();
        
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
        
        foreach (var config in configs)
        {
            // Copy tracks from other configs
            if (config.CanCopy)
            {
                config.ApplyCopiedTracks(configs);
            }

            var type = ConvertMSUTypeConfig(config);
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
            var msuType = _msuTypes.First(x => x.Name == config.Name);
            foreach (var copyDetails in config.Copy!)
            {
                var otherConfig = configs.First(x => x.Path == copyDetails.Msu);
                var otherMsuType = _msuTypes.First(x => x.Name == otherConfig.Name);
                msuType.Conversions[otherMsuType] = x => x + copyDetails.Modifier;
                otherMsuType.Conversions[msuType] = x => x - copyDetails.Modifier;
            }
        }

        var smz3 = _msuTypes.First(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer");
        var smz3Old = _msuTypes.First(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer Legacy");
        
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

    private MsuType ConvertMSUTypeConfig(MsuTypeConfig config)
    {
        var tracks = config.FullTrackList.Where(x => !x.IsUnused).Select(x => new MsuTypeTrack()
        {
            Name = x.Name,
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
            Tracks = tracks,
            RequiredTrackNumbers = tracks.Where(x => !x.IsIgnored).Select(x => x.Number).ToHashSet(),
            ValidTrackNumbers = tracks.Select(x => x.Number).ToHashSet()
        };
    }
}