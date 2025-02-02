﻿using System.Text.Json.Serialization;

namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Represents a loaded MSU
/// </summary>
public class Msu
{
    /// <summary>
    /// Constructor
    /// </summary>
    public Msu()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="type">The type of MSU. Null for unknown.</param>
    /// <param name="name">The name of the MSU</param>
    /// <param name="folderName">The name of the folder the MSU is in</param>
    /// <param name="fileName">The filename of the MSU file itself</param>
    /// <param name="path">The full path to the MSU</param>
    /// <param name="tracks">List of tracks that are in the MSU</param>
    /// <param name="msuDetails">Any MSU details to pull information about the MSU from</param>
    /// <param name="prevMsu">The previous MSU this is copied from</param>
    /// <param name="isHardwareMsu">If this is an MSU that is on hardware like the FxPakPro</param>
    public Msu(MsuType? type, string name, string folderName, string fileName, string path, ICollection<Track> tracks, MsuDetails? msuDetails, Msu? prevMsu, bool isHardwareMsu = false)
    {
        MsuType = type;
        Name = name;
        FolderName = folderName;
        FileName = fileName;
        Path = path;
        Tracks = tracks;
        IsHardwareMsu = isHardwareMsu;

        Version = msuDetails?.PackVersion ?? prevMsu?.Version;
        Creator = msuDetails?.PackAuthor ?? prevMsu?.Creator;
        Artist = msuDetails?.Artist ?? prevMsu?.Artist;
        Album = msuDetails?.Album ?? prevMsu?.Album;
        Url = msuDetails?.Url ?? prevMsu?.Url;
        HasDetails = msuDetails != null || prevMsu?.HasDetails == true;

        foreach (var track in tracks)
        {
            track.Msu = this;
        }
    }
    
    /// <summary>
    /// The type of MSU. Null for unknown.
    /// </summary>
    [JsonIgnore]
    public MsuType? MsuType { get; set; }

    /// <summary>
    /// The default name of the MSU
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// The default creator of the MSU
    /// </summary>
    public string? Creator { get; set; }
    
    /// <summary>
    /// The current version number of the MSU
    /// </summary>
    public string? Version { get; set; }
    
    /// <summary>
    /// If the MSU has details from a YAML file
    /// </summary>
    public bool HasDetails { get; set; }
    
    /// <summary>
    /// The default artist of an MSU
    /// </summary>
    public string? Artist { get; set; }
    
    /// <summary>
    /// The default album of an MSU
    /// </summary>
    public string? Album { get; set; }
    
    /// <summary>
    /// The default url of an MSU
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// The name of the folder the MSU is in
    /// </summary>
    public string FolderName { get; set; } = "";
    
    /// <summary>
    /// The name of the MSU file itself
    /// </summary>
    public string FileName { get; set; } = "";
    
    /// <summary>
    /// The full path to the MSU
    /// </summary>
    public string Path { get; set; } = "";
    
    /// <summary>
    /// If this is an MSU housed on hardware like the FxPakPro
    /// </summary>
    public bool IsHardwareMsu { get; set; }

    /// <summary>
    /// The list of tracks in this MSU
    /// </summary>
    public ICollection<Track> Tracks { get; set; } = new List<Track>();

    /// <summary>
    /// The MSU Type directory this was found under
    /// </summary>
    public string ParentMsuTypeDirectory { get; set; } = "";

    /// <summary>
    /// The relative path to the MSU from its parent msu type directory
    /// </summary>
    [JsonIgnore]
    public string RelativePath => !string.IsNullOrEmpty(ParentMsuTypeDirectory)
        ? System.IO.Path.GetRelativePath(ParentMsuTypeDirectory, Path)
        : Path; 

    /// <summary>
    /// If this unknown MSU should be ignored
    /// </summary>
    [JsonIgnore]
    public bool IgnoreUnknown => Settings.IsUserUnknownMsu;
    
    /// <summary>
    /// User settings applied to the MSU
    /// </summary>
    [JsonIgnore]
    public MsuSettings Settings { get; set; } = new() { MsuPath = "" };
    
    /// <summary>
    /// Displays the user override name if found or the default name otherwise
    /// </summary>
    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Settings.Name) ? Name : Settings.Name;
    
    /// <summary>
    /// Displays the user override creator if found or the default creator otherwise
    /// </summary>
    [JsonIgnore]
    public string DisplayCreator => string.IsNullOrWhiteSpace(Settings.Creator) ? Creator ?? "" : Settings.Creator;
    
    /// <summary>
    /// The user override MSUType if found or the default MsuType otherwise
    /// </summary>
    [JsonIgnore]
    public MsuType? SelectedMsuType => Settings.MsuType ?? MsuType;
    
    /// <summary>
    /// The MSU's name and creator, if applicable
    /// </summary>
    [JsonIgnore]
    public string FullName => DisplayName + (string.IsNullOrWhiteSpace(DisplayCreator) ? "" : $" by {DisplayCreator}");
    
    /// <summary>
    /// The name of the selected MSU, or unknown if one is not found
    /// </summary>
    [JsonIgnore]
    public string MsuTypeName => string.IsNullOrWhiteSpace(SelectedMsuType?.DisplayName) ? "Unknown" : SelectedMsuType.DisplayName;
    
    /// <summary>
    /// The tracks that are available to use based on the user's alt track settings
    /// </summary>
    [JsonIgnore]
    public ICollection<Track> ValidTracks => Tracks.Where(x => x.MatchesAltOption(Settings.AltOption)).ToList();
    
    /// <summary>
    /// The number of unique tracks found as part of the MSU
    /// </summary>
    [JsonIgnore]
    public int NumUniqueTracks => Tracks.Select(x => x.Path).Distinct().Count();
    
    /// <summary>
    /// The parent folder and msu file
    /// </summary>
    [JsonIgnore]
    public string AbbreviatedPath => GetAbbreviatedPath();
    
    /// <summary>
    /// If all tracks are marked as copyright safe
    /// </summary>
    [JsonIgnore]
    public bool AreAllTracksCopyrightSafe { get; set; }

    /// <summary>
    /// If the MSU matches filter settings
    /// </summary>
    /// <param name="filter">How closely this MSU needs to match the MSU type</param>
    /// <param name="type">The MSU type being looked for</param>
    /// <param name="compatibleMsuTypeNames">Optional list of MSU type names to use for filtering, unless the FilterType All is selected</param>
    /// <returns>True if matches, false otherwise</returns>
    public bool MatchesFilter(MsuFilter filter, MsuType type, List<string>? compatibleMsuTypeNames = null)
    {
        if (MatchesFilterType(filter, type) && Tracks.Count >= 1)
        {
            if (compatibleMsuTypeNames == null || filter == MsuFilter.All)
            {
                return true;
            }

            return compatibleMsuTypeNames.Contains(MsuTypeName);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// If the MSU has enough to tracks to be displayed (>= 20% of required tracks or > 10 tracks)
    /// </summary>
    [JsonIgnore]
    public bool HasSufficientTracks => NumUniqueTracks > MsuType?.RequiredTrackNumbers.Count / 5 || NumUniqueTracks > 10;

    private bool MatchesFilterType(MsuFilter filter, MsuType type)
    {
        return filter == MsuFilter.All ||
               (filter == MsuFilter.Favorite && Settings.IsFavorite) ||
               (filter == MsuFilter.Compatible && SelectedMsuType?.IsCompatibleWith(type) == true) ||
               (filter == MsuFilter.Exact && SelectedMsuType?.IsExactMatchWith(type) == true) ||
               (filter == MsuFilter.CopyrightSafe && AreAllTracksCopyrightSafe);
    }

    /// <summary>
    /// Returns the track that will play for the track number. Returns the valid fallback track if applicable.
    /// </summary>
    /// <param name="trackNumber">The track number to look up</param>
    /// <returns>The first track for the requested track number, if one is found</returns>
    public Track? GetTrackFor(int trackNumber)
    {
        return GetTracksFor(trackNumber).FirstOrDefault();
    }

    /// <summary>
    /// Returns the tracks that would play for the provided track number. Returns the valid fallback tracks if applicable.
    /// </summary>
    /// <param name="trackNumber">The track number to look up</param>
    /// <returns>The set of tracks for the requested track number</returns>
    public IEnumerable<Track> GetTracksFor(int trackNumber)
    {
        if (SelectedMsuType == null)
        {
            return Tracks.Where(x => x.Number == trackNumber);
        }

        var typeTrack = SelectedMsuType.Tracks.FirstOrDefault(x => x.Number == trackNumber);
        while (typeTrack != null && Tracks.All(x => x.Number != typeTrack.Number))
        {
            typeTrack = SelectedMsuType.Tracks.FirstOrDefault(x => x.Number == typeTrack.Fallback);
        }

        return Tracks.Where(x => x.Number == typeTrack?.Number);
    }
    
    private string GetAbbreviatedPath()
    {
        var fileInfo = new FileInfo(Path);
        var directory = fileInfo.Directory;
        return $"{directory?.Name}{System.IO.Path.DirectorySeparatorChar}{fileInfo.Name}";
    }
}