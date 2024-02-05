using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MSURandomizerLibrary.Configs;

internal class MsuTypeConfig
{
    [JsonPropertyName("meta"), JsonRequired]
    public MsuTypeConfigMeta Meta { get; set; } = null!;
    
    [JsonPropertyName("tracks"), JsonRequired]
    public MsuTypeConfigTracks Tracks { get; set; } = null!;
    
    [JsonPropertyName("copy")]
    public ICollection<MsuTypeConfigCopy>? Copy { get; set; }

    [JsonIgnore] public string Name => Meta.Name;
    
    public string? Path { get; set; }

    [JsonIgnore] public bool CanCopy => Copy?.Any() == true;

    [JsonIgnore] public List<MsuTypeConfigTrack> FullTrackList { get; set; } = new List<MsuTypeConfigTrack>();

    public IEnumerable<MsuTypeConfigTrack> DetermineBasicTracksNumbers()
    {
        FullTrackList.AddRange(Tracks.Basic);
        if (Tracks.Extended != null)
        {
            foreach (var track in Tracks.Extended)
            {
                if (track.Remap != null)
                {
                    track.Fallback = track.Remap.Value;
                }
                track.IsExtended = true;
                track.IsIgnored = true;
                FullTrackList.Add(track);
            }
        }

        var currentTrackNumber = 1;
        foreach (var track in FullTrackList)
        {
            if (track.Num != null)
            {
                currentTrackNumber = track.Num.Value + 1;
            }
            else
            {
                track.Num = currentTrackNumber;
                currentTrackNumber++;
            }
        }

        return FullTrackList;
    }

    public IEnumerable<MsuTypeConfigTrack> ApplyCopiedTracks(IEnumerable<MsuTypeConfig> configs)
    {
        if (Copy?.Any() != true) return FullTrackList;
        foreach (var copy in Copy)
        {
            var configToCopyFrom = configs.FirstOrDefault(x => x.Path == copy.Msu || x.Name == copy.Msu);
            if (configToCopyFrom == null) continue;
            foreach (var track in configToCopyFrom.FullTrackList)
            {
                FullTrackList.Add(new MsuTypeConfigTrack()
                {
                    Num = track.Num + copy.Modifier,
                    Title = track.Title,
                    Name = track.Name,
                    NonLooping = track.NonLooping,
                    Fallback = track.Fallback + copy.Modifier,
                    IsUnused = track.IsUnused,
                    IsExtended = track.IsExtended,
                    IsSpecial = track.IsSpecial,
                    PairedTracks = track.PairedTracks?.Select(x => x + copy.Modifier).ToList(),
                    IsIgnored = track.IsIgnored || copy.IgnoredTracks.Contains(track.Num!.Value + copy.Modifier) || track.IsExtended,
                    Description = track.Description
                });
            }
        }

        return FullTrackList;
    }
}

internal class MsuTypeConfigMeta
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("selectable")]
    public bool? Selectable { get; set; } = true;
    
    [JsonPropertyName("exactmatches")]
    public ICollection<string> ExactMatches { get; set; } = new List<string>();
}

internal class MsuTypeConfigTracks
{
    [JsonPropertyName("basic"), JsonRequired]
    public ICollection<MsuTypeConfigTrack> Basic { get; set; } = null!;
    
    [JsonPropertyName("extended")]
    public ICollection<MsuTypeConfigTrack>? Extended { get; set; }
}

internal class MsuTypeConfigTrack
{
    [JsonPropertyName("num")]
    public int? Num { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("nonlooping")]
    public bool NonLooping { get; set; }
    
    [JsonPropertyName("fallback")]
    public int Fallback { get; set; }
    
    [JsonPropertyName("remap")]
    public int? Remap { get; set; }
    
    [JsonPropertyName("unused")]
    public bool IsUnused { get; set; }
    
    [JsonPropertyName("ignore")]
    public bool IsIgnored { get; set; }
    
    [JsonPropertyName("special_track")]
    public bool IsSpecial { get; set; }
    
    [JsonPropertyName("paired_tracks")]
    public List<int>? PairedTracks { get; set; }
    
    [JsonIgnore]
    public bool IsExtended { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal class MsuTypeConfigCopy
{
    [JsonPropertyName("msu"), JsonRequired]
    public string Msu { get; set; } = "";
    
    [JsonPropertyName("modifier")]
    public int Modifier { get; set; }

    [JsonPropertyName("ignore")] 
    public ICollection<int> IgnoredTracks { get; set; } = new List<int>();
}