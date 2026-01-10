using System.Security.Cryptography;
using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Represents details about a track in a MSU Details YAML file
/// </summary>
public class MsuDetailsTrack
{
    /// <summary>
    /// The number of the track
    /// </summary>
    public int? TrackNumber { get; set; }
    
    /// <summary>
    /// The name of the song
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The name of the artist(s) that created the song
    /// </summary>
    public string? Artist { get; set; }
    
    /// <summary>
    /// The name of the album the song is from
    /// </summary>
    public string? Album { get; set; }
    
    /// <summary>
    /// Url for viewing/purchasing/downloading the song elsewhere
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// The name of the MSU the track is from
    /// </summary>
    public string? MsuName { get; set; }
    
    /// <summary>
    /// The name of the creator of the MSU the track is from
    /// </summary>
    public string? MsuAuthor { get; set; }
    
    /// <summary>
    /// The length of the file for determining alt tracks
    /// </summary>
    public long? FileLength { get; set; }
    
    /// <summary>
    /// The SHA-1 hash of the file for determining alt tracks
    /// </summary>
    public string? Hash { get; set; }
    
    /// <summary>
    /// The relative path to the track for determining alt tracks
    /// </summary>
    public string? Path { get; set; }
    
    /// <summary>
    /// If the track has been tested to be copyright safe
    /// </summary>
    public bool? IsCopyrightSafe { get; set; }
    
    /// <summary>
    /// A list of all of the alt tracks. If alt tracks are added, the file length, hash, and path for all tracks
    /// (including the base track) is required
    /// </summary>
    public ICollection<MsuDetailsTrack>? Alts { get; set; }

    /// <summary>
    /// If there are any details in this track
    /// </summary>
    [YamlIgnore]
    public bool HasData => !string.IsNullOrWhiteSpace(Name) || !string.IsNullOrWhiteSpace(Artist) || !string.IsNullOrWhiteSpace(Album);

    /// <summary>
    /// If this track contains any alts
    /// </summary>
    [YamlIgnore]
    public bool HasAltTrackData =>
        !string.IsNullOrWhiteSpace(Path) && !string.IsNullOrWhiteSpace(Hash) && FileLength > 0;

    /// <summary>
    /// Determines which file of two provided files matches the track
    /// </summary>
    /// <param name="path1">The first file to check</param>
    /// <param name="path2">The second file to check</param>
    /// <returns>Which path matches the track, if any. Returns null if no match.</returns>
    public string? DeterminePath(string path1, string path2)
    {
        var path1Exists = !string.IsNullOrWhiteSpace(path1) && File.Exists(path1);
        var path2Exists = !string.IsNullOrWhiteSpace(path2) && File.Exists(path2);
        var path1Length = path1Exists ? new FileInfo(path1).Length : 0;
        var path2Length = path2Exists ? new FileInfo(path2).Length : 0;

        if (path1Length == FileLength && path2Length == FileLength)
        {
            using var sha1 = SHA1.Create();
            using var stream1 = File.OpenRead(path1);
            using var stream2 = File.OpenRead(path2);
            var hash1 = BitConverter.ToString(sha1.ComputeHash(stream1)).Replace("-", "");
            var hash2 = BitConverter.ToString(sha1.ComputeHash(stream2)).Replace("-", "");
            if (hash1 == Hash)
            {
                return path1;
            }
            else if (hash2 == Hash)
            {
                return path2;
            }
        }
        else if (path1Length == FileLength)
        {
            return path1;
        }
        else if (path2Length == FileLength)
        {
            return path2;
        }

        return null;
    }

    /// <summary>
    /// Determines and updates the data for the track for saving to the MSU details YAML file
    /// </summary>
    /// <param name="msuPath">The path to the MSU file</param>
    /// <param name="trackPath">The path to the pcm file</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool CalculateAltInfo(string msuPath, string trackPath)
    {
        if (!File.Exists(trackPath)) return false;
        var msuFolder = new FileInfo(msuPath).DirectoryName ?? msuPath;
        Path = System.IO.Path.GetRelativePath(msuFolder, trackPath);
        FileLength = new FileInfo(trackPath).Length;
        using var sha1 = SHA1.Create();
        using var stream1 = File.OpenRead(trackPath);
        Hash = BitConverter.ToString(sha1.ComputeHash(stream1)).Replace("-", "");
        return true;
    }
}