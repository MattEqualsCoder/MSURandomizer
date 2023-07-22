using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

public class MsuDetailsService : IMsuDetailsService
{
    private readonly ILogger<MsuDetailsService> _logger;
    private readonly MsuAppSettings _msuAppSettings;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public MsuDetailsService(ILogger<MsuDetailsService> logger, MsuAppSettings msuAppSettings)
    {
        _logger = logger;
        _msuAppSettings = msuAppSettings;
        _serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public Msu? LoadMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlPath, MsuDetailsBasic? basicDetails, out MsuDetails? msuDetails, out string? error)
    {
        using var fileStream = new FileStream(yamlPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        var yamlText = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(yamlText))
        {
            _logger.LogError("Empty MSU yaml file {File}", yamlPath);
            msuDetails = null;
            error = "Empty MSU yaml file";
            return null;
        }

        var trackType = basicDetails?.Tracks?.GetType().Name;

        // The YAML can come in the form of SMZ3 specific and a generic format. Try to detect which format it's in
        // and deserialize it accordingly
        if (trackType?.StartsWith("Dictionary") == true)
        {
            return ParseSmz3MsuDetails(msuType, msuPath, msuDirectory, msuBaseName, yamlText, basicDetails!, out msuDetails, out error);
        }
        else
        {
            return ParseGenericMsuDetails(msuType, msuPath, msuDirectory, msuBaseName, yamlText, out msuDetails, out error);
        }
    }

    public bool SaveMsuDetails(Msu msu, string outputPath)
    {
        if (_msuAppSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(msu.MsuTypeName))
        {
            return SaveMsuDetailsSmz3(msu, outputPath);
        }
        else
        {
            return SaveMsuDetailsGeneric(msu, outputPath);
        }
    }

    public MsuDetailsBasic? GetBasicMsuDetails(string msuPath, out string? yamlPath, out string? error)
    {
        var path = msuPath.Replace(".msu", ".yml", StringComparison.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            path = path.Replace(".yml", ".yaml");
            if (!File.Exists(path))
            {
                yamlPath = null;
                error = null;
                return GetBasicJsonDetails(msuPath);
            }
        }

        var msuDetails = InternalGetBasicMsuDetails(path, out error);
        yamlPath = msuDetails == null ? null : path;
        return msuDetails;
    }

    private bool SaveMsuDetailsGeneric(Msu msu, string outputPath)
    {
        var msuTrackDetails = msu.Tracks.OrderBy(x => x.Number).Select(x => new MsuDetailsTrack()
        {
            TrackNumber = x.Number,
            TrackName = x.TrackName,
            Name = x.SongName,
            Artist = x.Artist,
            Album = x.Album,
            Url = x.Url,
            MsuAuthor = x.MsuCreator,
            MsuName = x.MsuName
        }).ToList();

        var msuDetails = new MsuDetailsGeneric()
        {
            PackName = msu.Name,
            PackAuthor = msu.Creator,
            PackVersion = "1",
            Tracks = msuTrackDetails
        };

        try
        {
            var yaml = _serializer.Serialize(msuDetails);
            File.WriteAllText(outputPath, yaml);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to write MSU YAML details to {Path}", outputPath);
            return false;
        }
    }
    
    private bool SaveMsuDetailsSmz3(Msu msu, string outputPath)
    {
        var metroidFirst = _msuAppSettings.MetroidFirstMsuTypes?.Contains(msu.MsuTypeName) == true;

        var msuTrackDetails = new MsuDetailsTrackListSmz3();
        
        foreach (var prop in typeof(MsuDetailsTrackListSmz3).GetProperties())
        {
            var attribute = prop.GetCustomAttributes<Smz3TrackNumberAttribute>().First();
            var trackNumber = metroidFirst ? attribute.MetroidFirst : attribute.ZeldaFirst;
            var track = msu.Tracks.FirstOrDefault(x => x.Number == trackNumber);
            
            if (track == null)
            {
                continue;
            }

            prop.SetValue(msuTrackDetails, new MsuDetailsTrack()
            {
                Name = track.SongName,
                Artist = track.Artist,
                Album = track.Album,
                Url = track.Url,
                MsuAuthor = track.MsuCreator,
                MsuName = track.MsuName
            });
        }
        
        var msuDetails = new MsuDetailsSmz3()
        {
            PackName = msu.Name,
            PackAuthor = msu.Creator,
            PackVersion = "1",
            Tracks = msuTrackDetails
        };

        try
        {
            var yaml = _serializer.Serialize(msuDetails);
            File.WriteAllText(outputPath, yaml);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to write MSU YAML details to {Path}", outputPath);
            return false;
        }
    }

    private MsuDetailsBasic? InternalGetBasicMsuDetails(string yamlPath, out string? error)
    {
        try
        {
            var yamlText = File.ReadAllText(yamlPath);
            error = null;
            return _deserializer.Deserialize<MsuDetailsBasic>(yamlText);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not parse YAML file {Path}", yamlPath);
            error = $"Could not load YAML file: {e.Message}";
            return null;
        }
    }

    private MsuDetailsBasic? GetBasicJsonDetails(string msuPath)
    {
        var fileInfo = new FileInfo(msuPath);
        if (fileInfo.DirectoryName == null)
            return null;
        var jsonPaths = Directory.EnumerateFiles(fileInfo.DirectoryName, "*.json");
        if (!jsonPaths.Any()) 
            return null;
        var path = jsonPaths.First();
        try
        {
            var jsonText = File.ReadAllText(path);
            var json = JsonSerializer.Deserialize<TracksJson>(jsonText);
            if (json == null)
            {
                return null;
            }

            return new MsuDetailsBasic()
            {
                PackName = json.Name ?? json.Pack ?? json.PackName,
                PackAuthor = json.Creator ?? json.PackCreator ?? json.PackAuthor,
                Artist = json.Artist,
                Url = json.Url
            };
        }
        catch
        {
            return null;
        }
    }

    private List<Track> GetTrackDetails(ICollection<MsuDetailsTrack> tracks, MsuDetails msuDetails, string directory, string baseName)
    {
        if (tracks.Any(x => (x.TrackNumber ?? 0) <= 0))
        {
            throw new InvalidOperationException("Track missing number");
        }
        
        var toReturn = new List<Track>();
        foreach (var track in tracks)
        {
            var trackNumber = track.TrackNumber ?? 0;
            
            // If there's no alt data for the track, simply load the base PCM file as the track
            if (!track.HasAltTrackData)
            {
                var pcmFilePath = $"{directory}{Path.DirectorySeparatorChar}{baseName}-{trackNumber}.pcm";
                track.Path = pcmFilePath;
                toReturn.Add(new Track
                (
                    trackName: track.TrackName ?? $"Track #{trackNumber}",
                    number: trackNumber,
                    songName: string.IsNullOrWhiteSpace(track.Name) ? $"Track #{trackNumber}" : track.Name,
                    path: pcmFilePath,
                    msuPath: $"{directory}{Path.DirectorySeparatorChar}{baseName}.msu",
                    msuName: msuDetails.PackName ?? baseName,
                    msuCreator: msuDetails.PackAuthor,
                    artist: string.IsNullOrWhiteSpace(track.Artist) ? msuDetails.Artist : track.Artist,
                    album: string.IsNullOrWhiteSpace(track.Album) ? msuDetails.Album : track.Album,
                    url: string.IsNullOrWhiteSpace(track.Url) ? msuDetails.Url : track.Url
                ));
            }
            // If there are alt tracks, we need to determine which file is which in case they've been swapped
            // manually or via script file
            else
            {
                var basePcm = $"{directory}{Path.DirectorySeparatorChar}{baseName}-{trackNumber}.pcm";
                foreach (var subTrack in track.Alts!.Append(track))
                {
                    var subTrackPath = subTrack.Path?.Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);
                    var altPcmPath = $"{directory}{Path.DirectorySeparatorChar}{subTrackPath}";
                    var path = subTrack.DeterminePath(basePcm, altPcmPath);
                    toReturn.Add(new Track
                    (
                        trackName: track.TrackName ?? $"Track #{trackNumber}",
                        number: trackNumber,
                        songName: string.IsNullOrWhiteSpace(subTrack.Name) ? $"Track #{trackNumber}" : subTrack.Name,
                        path: path ?? "",
                        msuPath: $"{directory}{Path.DirectorySeparatorChar}{baseName}.msu",
                        artist: string.IsNullOrWhiteSpace(subTrack.Artist) ? msuDetails.Artist : subTrack.Artist,
                        album: string.IsNullOrWhiteSpace(subTrack.Album) ? msuDetails.Album : subTrack.Album,
                        url: string.IsNullOrWhiteSpace(track.Url) ? msuDetails.Url : track.Url,
                        msuName: msuDetails.PackName ?? baseName,
                        msuCreator: msuDetails.PackAuthor,
                        isAlt: subTrack != track
                    ));
                }
            }
        }

        return toReturn;
    }
    
    private List<Track> GetSmz3TrackDetails(MsuDetailsSmz3 msuDetailsSmz3, string directory, string baseName, bool metroidFirst)
    {
        var tracks = new List<MsuDetailsTrack>();
        
        // For SMZ3 specific YAML files, we need to get the tracks by property and get the track number from the 
        // property attributes based on if it's Metroid or Zelda first
        foreach (var prop in typeof(MsuDetailsTrackListSmz3).GetProperties())
        {
            if (prop.GetValue(msuDetailsSmz3.Tracks) is not MsuDetailsTrack track)
            {
                continue;
            }

            var attribute = prop.GetCustomAttributes<Smz3TrackNumberAttribute>().First();
            var trackNumber = metroidFirst ? attribute.MetroidFirst : attribute.ZeldaFirst;

            track.TrackNumber = trackNumber;
            track.TrackName = prop.Name;
            tracks.Add(track);
        }

        return GetTrackDetails(tracks, msuDetailsSmz3, directory, baseName);
    }

    private Msu? ParseSmz3MsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlText, MsuDetailsBasic basicDetails, out MsuDetails? msuDetails, out string? error)
    {
        try
        {
            var smz3Details = _deserializer.Deserialize<MsuDetailsSmz3>(yamlText);
            msuDetails = smz3Details;
            if (smz3Details.Tracks == null)
            {
                error = null;
                return new Msu()
                {
                    FolderName = new DirectoryInfo(msuDirectory).Name,
                    FileName = msuBaseName,
                    Path = msuPath,
                    Tracks = new List<Track>(),
                    MsuType = msuType,
                    Name = string.IsNullOrWhiteSpace(smz3Details.PackName) ? msuBaseName : smz3Details.PackName,
                    Creator = smz3Details.PackAuthor ?? ""
                };
            }
            var tracks = GetSmz3TrackDetails(smz3Details, msuDirectory, msuBaseName, _msuAppSettings.MetroidFirstMsuTypes?.Contains(msuType.DisplayName) == true);
            error = null;
            return new Msu()
            {
                FolderName = new DirectoryInfo(msuDirectory).Name,
                FileName = msuBaseName,
                Path = msuPath,
                Tracks = tracks,
                MsuType = msuType,
                Name = string.IsNullOrWhiteSpace(smz3Details.PackName) ? msuBaseName : smz3Details.PackName,
                Creator = smz3Details.PackAuthor ?? ""
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to parse SMZ3 MSU yaml file for {Directory}{Separator}{BaseName}.msu", msuDirectory, Path.DirectorySeparatorChar, msuBaseName);
            Console.WriteLine(e);
            msuDetails = null;
            error = $"Could not load YAML file: {e.Message}";
            return null;
        }
    }

    private Msu? ParseGenericMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlText, out MsuDetails? msuDetails, out string? error)
    {
        try
        {
            var genericDetails = _deserializer.Deserialize<MsuDetailsGeneric>(yamlText);
            msuDetails = genericDetails;
            if (genericDetails.Tracks?.Any() != true || genericDetails.Tracks?.Any(x => x.TrackNumber <= 0) == true)
            {
                _logger.LogInformation("YAML file for MSU {Path} missing track details", msuPath);
                error = null;
                return new Msu()
                {
                    FolderName = new DirectoryInfo(msuDirectory).Name,
                    FileName = msuBaseName,
                    Path = msuPath,
                    Tracks = new List<Track>(),
                    MsuType = msuType,
                    Name = string.IsNullOrWhiteSpace(genericDetails.PackName) ? msuBaseName : genericDetails.PackName,
                    Creator = genericDetails.PackAuthor ?? ""
                };
            }
            var tracks = GetTrackDetails(genericDetails.Tracks!, genericDetails, msuDirectory, msuBaseName);
            error = null;
            return new Msu()
            {
                FolderName = new DirectoryInfo(msuDirectory).Name,
                FileName = msuBaseName,
                Path = msuPath,
                Tracks = tracks,
                MsuType = msuType,
                Name = string.IsNullOrWhiteSpace(genericDetails.PackName) ? msuBaseName : genericDetails.PackName,
                Creator = genericDetails.PackAuthor ?? ""
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to parse generic MSU yaml file for {Directory}{Separator}{BaseName}.msu", msuDirectory, Path.DirectorySeparatorChar, msuBaseName);
            Console.WriteLine(e);
            msuDetails = null;
            error = $"Could not load YAML file: {e.Message}";
            return null;
        }
    }
}