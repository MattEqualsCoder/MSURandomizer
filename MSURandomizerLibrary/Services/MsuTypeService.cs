using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

internal class MsuTypeService : IMsuTypeService
{
    public static readonly Regex TrackYamlNameRegex = new("[^a-zA-Z0-9 ]");
    
    private readonly ILogger<MsuTypeService> _logger;
    private readonly List<MsuType> _msuTypes = new();
    private readonly MsuAppSettings _msuAppSettings;
    private Dictionary<string, Dictionary<int, string>> _msuTypeTrackYamlRewrites = new();

    public MsuTypeService(ILogger<MsuTypeService> logger, MsuAppSettings randomizerMsuAppSettings)
    {
        _logger = logger;
        _msuAppSettings = randomizerMsuAppSettings;
        LoadYamlTrackRewrites();
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
            data.Path = data.Meta.Path;
        }
        
        FinalizeConfigs(configs);
    }

    public IReadOnlyCollection<MsuType> MsuTypes => _msuTypes;

    public void LoadMsuTypes()
    {
        var stream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream("MSURandomizerLibrary.msu_types.json");
        LoadMsuTypes(stream!);
    }
    
    public void LoadMsuTypes(string directory)
    {
        _msuTypes.Clear();
        
        if (!Directory.Exists(directory))
        {
            throw new FileNotFoundException($"MSU type directory {directory} not found");
        }
        
        _logger.LogInformation("Loading MSU types from {directory}", directory);
        
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

    public MsuType? GetSMZ3MsuType() => MsuTypes.FirstOrDefault(x =>
        (_msuAppSettings.Smz3MsuTypes?.Contains(x.Name) == true ||
         _msuAppSettings.Smz3MsuTypes?.Contains(x.DisplayName) == true) && x.Selectable);

    public MsuType? GetSMZ3LegacyMSUType() => MsuTypes.FirstOrDefault(x =>
        (_msuAppSettings.Smz3MsuTypes?.Contains(x.Name) == true ||
         _msuAppSettings.Smz3MsuTypes?.Contains(x.DisplayName) == true) && !x.Selectable);

    private void LoadYamlTrackRewrites()
    {
        var stream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream("MSURandomizerLibrary.yaml_tracks_rewrites.yml");

        if (stream != null)
        {
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            var text = reader.ReadToEnd();
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            _msuTypeTrackYamlRewrites = deserializer.Deserialize<Dictionary<string, Dictionary<int, string>>>(text);
        }
        
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
            var msuName = _msuAppSettings.GetMsuTypeName(config.Name);
            var msuType = _msuTypes.First(x => x.DisplayName == msuName);
            foreach (var copyDetails in config.Copy!)
            {
                var otherConfig = configs.First(x => x.Path == copyDetails.Msu || x.Name == copyDetails.Msu);
                var otherMsuType = _msuTypes.First(x => x.DisplayName == _msuAppSettings.GetMsuTypeName(otherConfig.Name));
                msuType.Conversions[otherMsuType] = x => x + copyDetails.Modifier;
                otherMsuType.Conversions[msuType] = x => x - copyDetails.Modifier;
            }

            foreach (var exactMatch in config.Meta.ExactMatches)
            {
                var otherConfig = configs.First(x => x.Path == exactMatch || x.Name == exactMatch);
                var otherMsuType = _msuTypes.First(x => x.DisplayName == _msuAppSettings.GetMsuTypeName(otherConfig.Name));
                msuType.ExactMatches.Add(otherMsuType);
                otherMsuType.ExactMatches.Add(msuType);
            }
        }

        var smz3Types = _msuTypes.Where(x => _msuAppSettings.Smz3MsuTypes?.Contains(x.DisplayName) == true);
        var smz3One = smz3Types.FirstOrDefault();
        var smz3Two = smz3Types.LastOrDefault();

        if (smz3One != smz3Two && smz3One != null && smz3Two != null)
        {
            smz3One.Conversions[smz3Two] = x =>
            {
                return x switch
                {
                    < 99 => x + 100,
                    > 99 => x - 100,
                    _ => x
                };
            };
        
            smz3Two.Conversions[smz3One] = x =>
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
        _msuTypeTrackYamlRewrites.TryGetValue(config.Name, out var rewrite);
        
        var tracks = config.FullTrackList.Where(x => !x.IsUnused).Select(x => new MsuTypeTrack()
        {
            Name = string.IsNullOrWhiteSpace(x.Title) ? x.Name : x.Title,
            Number = x.Num!.Value,
            Fallback = x.Fallback,
            PairedTracks = x.PairedTracks,
            IsExtended = x.IsExtended,
            NonLooping = x.NonLooping,
            IsIgnored = x.IsIgnored || x.IsExtended,
            IsSpecial = x.IsSpecial,
            YamlName = TrackYamlNameRegex.Replace(string.IsNullOrWhiteSpace(x.Title) ? x.Name : x.Title, "").Replace(" ", "_").Replace("__", "_").ToLower(),
            YamlNameSecondary = rewrite?[x.Num!.Value],
            Description = x.Description
        });

        return new MsuType()
        {
            Name = config.Name,
            DisplayName = _msuAppSettings.GetMsuTypeName(config.Name),
            Selectable = config.Meta.Selectable != false,
            Tracks = tracks,
            RequiredTrackNumbers = tracks.Where(x => !x.IsIgnored).Select(x => x.Number).ToHashSet(),
            ValidTrackNumbers = tracks.Select(x => x.Number).ToHashSet()
        };
    }
}