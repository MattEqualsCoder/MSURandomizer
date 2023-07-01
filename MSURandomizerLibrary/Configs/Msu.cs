using System.Collections.Generic;

namespace MSURandomizerLibrary.Configs;

public class Msu
{
    public MsuType? MsuType { get; set; }
    public required string FolderName { get; set; }
    public required string FileName { get; set; }
    public required string Path { get; set; }
    public required Dictionary<int, List<Track>> Tracks { get; set; }
}