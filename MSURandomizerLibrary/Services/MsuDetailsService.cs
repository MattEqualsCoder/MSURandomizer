using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            .Build();
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
        foreach (var prop in typeof(MsuDetailsTrackList).GetProperties())
        {
            if (prop.GetValue(msuDetailsSmz3.Tracks) is not MsuDetailsTrack track)
            {
                track = new MsuDetailsTrack();
            }

            var attribute = prop.GetCustomAttributes<Smz3TrackNumberAttribute>().First();
            var trackNumber = metroidFirst ? attribute.MetroidFirst : attribute.ZeldaFirst;

            track.TrackNumber = trackNumber;
            track.TrackName = prop.Name;
            tracks.Add(track);
        }

        return GetTrackDetails(tracks, msuDetailsSmz3, directory, baseName);
    }

    public Msu? LoadMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlPath)
    {
        using var fileStream = new FileStream(yamlPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        var yamlText = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(yamlText))
        {
            _logger.LogError("Empty MSU yaml file {File}", yamlPath);
            return null;
        }

        // The YAML can come in the form of SMZ3 specific and a generic format. Try to detect which format it's in
        // and deserialize it accordingly
        if ((msuType.Name == _msuAppSettings.Smz3MsuTypeName || msuType.Name == _msuAppSettings.Smz3LegacyMsuTypeName) && yamlText.Contains("light_world") && yamlText.Contains("samus_fanfare"))
        {
            return ParseSmz3MsuDetails(msuType, msuPath, msuDirectory, msuBaseName, yamlText);
        }
        else
        {
            return ParseGenericMsuDetails(msuType, msuPath, msuDirectory, msuBaseName, yamlText);
        }
    }

    public void SaveMsuDetails(Msu msu, string outputPath)
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
            PackVersion = 1,
            Tracks = msuTrackDetails
        };
        
        var yaml = _serializer.Serialize(msuDetails);
        File.WriteAllText(outputPath, yaml);
    }

    private Msu? ParseSmz3MsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlText)
    {
        try
        {
            var msuDetails = _deserializer.Deserialize<MsuDetailsSmz3>(yamlText);
            if (msuDetails.Tracks == null)
            {
                return null;
            }
            var tracks = GetSmz3TrackDetails(msuDetails, msuDirectory, msuBaseName, msuType.Name.Contains("Legacy"));
            return new Msu()
            {
                FolderName = new DirectoryInfo(msuDirectory).Name,
                FileName = msuBaseName,
                Path = msuPath,
                Tracks = tracks,
                MsuType = msuType,
                Name = string.IsNullOrWhiteSpace(msuDetails.PackName) ? msuBaseName : msuDetails.PackName,
                Creator = msuDetails.PackAuthor ?? ""
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to parse SMZ3 MSU yaml file for {Directory}{Separator}{BaseName}.msu", msuDirectory, Path.DirectorySeparatorChar, msuBaseName);
            Console.WriteLine(e);
            return null;
        }
    }

    private Msu? ParseGenericMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlText)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var msuDetails = deserializer.Deserialize<MsuDetailsGeneric>(yamlText);
            if (msuDetails.Tracks?.Any() != true || msuDetails.Tracks?.Any(x => x.TrackNumber <= 0) == true)
            {
                _logger.LogInformation("YAML file for MSU {Path} missing track details", msuPath);
                return null;
            }
            var tracks = GetTrackDetails(msuDetails.Tracks!, msuDetails, msuDirectory, msuBaseName);
            return new Msu()
            {
                FolderName = new DirectoryInfo(msuDirectory).Name,
                FileName = msuBaseName,
                Path = msuPath,
                Tracks = tracks,
                MsuType = msuType,
                Name = string.IsNullOrWhiteSpace(msuDetails.PackName) ? msuBaseName : msuDetails.PackName,
                Creator = msuDetails.PackAuthor ?? ""
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to parse generic MSU yaml file for {Directory}{Separator}{BaseName}.msu", msuDirectory, Path.DirectorySeparatorChar, msuBaseName);
            Console.WriteLine(e);
            return null;
        }
    }
}