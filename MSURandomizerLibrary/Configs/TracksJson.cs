using System.Text.Json.Serialization;

namespace MSURandomizerLibrary.Configs;

internal class TracksJson
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("pack")]
    public string? Pack { get; set; }
    
    [JsonPropertyName("pack_name")]
    public string? PackName { get; set; }
    
    [JsonPropertyName("pack_creator")]
    public string? PackCreator { get; set; }
    
    [JsonPropertyName("pack_author")]
    public string? PackAuthor { get; set; }
    
    [JsonPropertyName("creator")]
    public string? Creator { get; set; }
    
    [JsonPropertyName("artist")]
    public string? Artist { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}