namespace MSURandomizerLibrary.Configs;

public class Track
{
    public required int Number { get; set; }
    public required string Name { get; set; }
    public required string Path { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; } 
}