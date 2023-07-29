using System.Collections.Concurrent;

namespace MSURandomizerLibrary.Configs;

public class MsuLookupCache
{
    public ConcurrentDictionary<string, MsuLookupCacheEntry> Data { get; set; } = new();
}

public class MsuLookupCacheEntry
{
    public string? YamlHash { get; set; }
    public string? PcmFileList { get; set; }
    public required Msu MsuData { get; set; }
    public required string MsuTypeName { get; set; }
}