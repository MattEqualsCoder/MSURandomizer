using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

internal class MsuDetailsService : IMsuDetailsService
{
    private readonly ILogger<MsuDetailsService> _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public MsuDetailsService(ILogger<MsuDetailsService> logger)
    {
        _logger = logger;
        _serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public bool SaveMsuDetails(Msu msu, string outputPath, out string? error)
    {
        var tracks = new Dictionary<string, MsuDetailsTrack>();

        error = null;
        
        foreach (var trackGroup in msu.Tracks.GroupBy(x => x.Number))
        {
            var trackDetails = InternalConvertToTrackDetails(msu, trackGroup, out var trackError);
            if (trackError != null)
                error = trackError;
            var msuTypeTrack = msu.MsuType?.Tracks.FirstOrDefault(x => x.Number == trackDetails.TrackNumber);
            tracks[msuTypeTrack?.YamlNameSecondary ?? msuTypeTrack?.YamlName ?? trackGroup.First().TrackName] = trackDetails;
        }
        
        var msuDetails = new MsuDetails()
        {
            PackName = msu.Name,
            PackAuthor = msu.Creator,
            PackVersion = "1",
            Album = msu.Album,
            Artist = msu.Artist,
            Url = msu.Url,
            Tracks = tracks
        };

        if (msu.MsuType != null)
        {
            msuDetails.MsuType = msu.MsuType?.Name;
        }

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

    public MsuDetails? GetMsuDetails(string msuPath, out string yamlHash, out string? error)
    {
        var path = msuPath.Replace(".msu", ".yml", StringComparison.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            path = path.Replace(".yml", ".yaml");
            if (!File.Exists(path))
            {
                yamlHash = "";
                error = null;
                return GetBasicJsonDetails(msuPath);
            }
        }

        var msuDetails = InternalGetMsuDetails(path, out yamlHash, out error);
        return msuDetails;
    }

    private MsuDetailsTrack InternalConvertToTrackDetails(Msu msu, IGrouping<int, Track> trackGroup, out string? error)
    {
        error = null;

        var tracks = trackGroup.OrderBy(x => x.IsAlt);
        var track = tracks.First();
            
        var output = new MsuDetailsTrack()
        {
            Name = track.SongName,
            TrackNumber = track.Number
        };
        
        if (!string.IsNullOrEmpty(track.OriginalMsu?.DisplayCreator) && track.OriginalMsu?.DisplayCreator != msu.DisplayCreator)
        {
            output.MsuAuthor = track.OriginalMsu?.DisplayCreator;
        }
        
        if (!string.IsNullOrEmpty(track.OriginalMsu?.DisplayName) && track.OriginalMsu?.DisplayName != msu.DisplayName)
        {
            output.MsuName = track.OriginalMsu?.DisplayName;
        }
        
        if (!string.IsNullOrEmpty(track.Artist) && track.Artist != msu.Artist)
        {
            output.Artist = track.Artist;
        }

        if (!string.IsNullOrEmpty(track.Artist) && track.Artist != msu.Artist)
        {
            output.Artist = track.Artist;
        }
        
        if (!string.IsNullOrEmpty(track.Album) && track.Album != msu.Album)
        {
            output.Album = track.Album;
        }
        
        if (!string.IsNullOrEmpty(track.Url) && track.Url != msu.Url)
        {
            output.Url = track.Url;
        }

        if (tracks.Count() == 1)
        {
            return output;
        }

        var alts = new List<MsuDetailsTrack>();
        if (!output.CalculateAltInfo(msu.Path, track.Path))
        {
            _logger.LogWarning("Unable to calculate alt track info for {Path}", track.Path);
            error = $"Unable to calculate alt track info for {track.Path}";
        }
            
        foreach (var altTrack in trackGroup.Where(x => x != track))
        {
            var altOutput = new MsuDetailsTrack()
            {
                Name = altTrack.SongName
            };
            
            if (!string.IsNullOrEmpty(altTrack.OriginalMsu?.DisplayCreator) && altTrack.OriginalMsu?.DisplayCreator != msu.DisplayCreator)
            {
                altOutput.MsuAuthor = altTrack.OriginalMsu?.DisplayCreator;
            }
        
            if (!string.IsNullOrEmpty(altTrack.OriginalMsu?.DisplayName) && altTrack.OriginalMsu?.DisplayName != msu.DisplayName)
            {
                altOutput.MsuName = altTrack.OriginalMsu?.DisplayName;
            }
        
            if (!string.IsNullOrEmpty(altTrack.Artist) && altTrack.Artist != msu.Artist)
            {
                altOutput.Artist = altTrack.Artist;
            }

            if (!string.IsNullOrEmpty(altTrack.Artist) && altTrack.Artist != msu.Artist)
            {
                altOutput.Artist = altTrack.Artist;
            }
        
            if (!string.IsNullOrEmpty(altTrack.Album) && altTrack.Album != msu.Album)
            {
                altOutput.Album = altTrack.Album;
            }
        
            if (!string.IsNullOrEmpty(altTrack.Url) && altTrack.Url != msu.Url)
            {
                altOutput.Url = altTrack.Url;
            }
            
            if (!altOutput.CalculateAltInfo(msu.Path, altTrack.Path))
            {
                _logger.LogWarning("Unable to calculate alt track info for {Path}", altTrack.Path);
                error = $"Unable to calculate alt track info for {altTrack.Path}";
            }
            
            alts.Add(altOutput);
        }

        output.Alts = alts;

        return output;
    }

    private MsuDetails? InternalGetMsuDetails(string yamlPath, out string yamlHash, out string? error)
    {
        try
        {
            var yamlText = File.ReadAllText(yamlPath);
            using var sha1 = SHA1.Create();
            yamlHash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF32.GetBytes(yamlText))).Replace("-", "");
            error = null;
            return _deserializer.Deserialize<MsuDetails>(yamlText);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not parse YAML file {Path}", yamlPath);
            error = $"Could not load YAML file: {e.Message}";
            yamlHash = "";
            return null;
        }
    }

    private MsuDetails? GetBasicJsonDetails(string msuPath)
    {
        var fileInfo = new FileInfo(msuPath);
        if (fileInfo.DirectoryName == null)
            return null;
        var baseName = fileInfo.Name.Replace(fileInfo.Extension, "");
        var jsonPaths = Directory.EnumerateFiles(fileInfo.DirectoryName, "*.json").OrderBy(x => !x.Contains(baseName));
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

            return new MsuDetails()
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

    private List<Track> GetTrackDetails(MsuType msuType, MsuDetails msuDetails, string directory, string baseName, out string? error)
    {
        error = null;
        
        var toReturn = new List<Track>();
        foreach (var trackInfo in msuDetails.Tracks!)
        {
            var trackName = trackInfo.Key;
            var track = trackInfo.Value;
            var msuTypeTrack = msuType.Tracks.FirstOrDefault(x =>
                x.YamlName == trackName || x.YamlNameSecondary == trackName);

            if (msuTypeTrack == null && track.TrackNumber != null)
            {
                msuTypeTrack = msuType.Tracks.FirstOrDefault(x => x.Number == track.TrackNumber);
            }

            if (msuTypeTrack == null)
            {
                error = $"Could not match track {trackName} in MSU {baseName}";
                continue;
            }
            
            var trackNumber = msuTypeTrack.Number;
            
            // If there's no alt data for the track, simply load the base PCM file as the track
            if (!track.HasAltTrackData)
            {
                var pcmFilePath = $"{directory}{Path.DirectorySeparatorChar}{baseName}-{trackNumber}.pcm";
                track.Path = pcmFilePath;

                if (!File.Exists(pcmFilePath))
                {
                    continue;
                }
                
                toReturn.Add(new Track
                (
                    trackName: msuTypeTrack.Name,
                    number: msuTypeTrack.Number,
                    songName: string.IsNullOrWhiteSpace(track.Name) ? $"Track #{trackNumber}" : track.Name,
                    path: pcmFilePath,
                    artist: track.Artist,
                    album: track.Album,
                    url: track.Url,
                    isBaseFile: true
                )
                {
                    MsuName = trackInfo.Value.MsuName,
                    MsuCreator = trackInfo.Value.MsuAuthor
                });
            }
            // If there are alt tracks, we need to determine which file is which in case they've been swapped
            // manually or via script file
            else
            {
                var basePcm = Path.Combine(directory, $"{baseName}-{trackNumber}.pcm");
                var basePcmOriginal = $"{directory}{Path.DirectorySeparatorChar}{baseName}-{trackNumber}_Original.pcm";
                var basePcmMatched = false;
                foreach (var subTrack in track.Alts!.Append(track))
                {
                    var subTrackPath = subTrack.Path?.Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);
                    var altPcmPath = Path.Combine(directory, $"{subTrackPath}");
                    if (altPcmPath == basePcm)
                    {
                        altPcmPath = basePcmOriginal;
                    }
                    var path = basePcmMatched ? altPcmPath : subTrack.DeterminePath(basePcm, altPcmPath);

                    if (path == basePcm)
                        basePcmMatched = true;

                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        continue;
                    }
                    
                    toReturn.Add(new Track
                    (
                        trackName: msuTypeTrack.Name,
                        number: trackNumber,
                        songName: string.IsNullOrWhiteSpace(subTrack.Name) ? $"Track #{trackNumber}" : subTrack.Name,
                        path: path,
                        artist: subTrack.Artist,
                        album: subTrack.Album,
                        url: subTrack.Url,
                        isAlt: subTrack != track,
                        isBaseFile: path == basePcm
                    )
                    {
                        MsuName = trackInfo.Value.MsuName,
                        MsuCreator = trackInfo.Value.MsuAuthor
                    });
                }
            }
        }

        return toReturn;
    }
    
    public Msu? ConvertToMsu(MsuDetails msuDetails, MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, out string? error)
    {
        try
        {
            var tracks = GetTrackDetails(msuType, msuDetails, msuDirectory, msuBaseName, out error);
            return new Msu(
                type: msuType, 
                name: string.IsNullOrWhiteSpace(msuDetails.PackName) ? msuBaseName : msuDetails.PackName, 
                folderName: new DirectoryInfo(msuDirectory).Name, 
                fileName: msuBaseName, 
                path: msuPath, 
                tracks: tracks, 
                msuDetails: msuDetails,
                prevMsu: null
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to parse generic MSU yaml file for {Directory}{Separator}{BaseName}.msu", msuDirectory, Path.DirectorySeparatorChar, msuBaseName);
            Console.WriteLine(e);
            error = $"Could not load YAML file: {e.Message}";
            return null;
        }
    }
}