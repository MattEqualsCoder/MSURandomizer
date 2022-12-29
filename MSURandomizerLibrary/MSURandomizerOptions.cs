using System.Collections.Generic;

namespace MSURandomizerLibrary;

/// <summary>
/// Class with all options for MSU generation
/// </summary>
public class MSURandomizerOptions
{
    public string? Directory { get; set; }
    public string Name { get; set; } = "RandomizedMSU";
    public string? OutputType { get; set; }
    public MSUFilter Filter { get; set; } = MSUFilter.Compatible;
    public bool AvoidDuplicates { get; set; }
    public bool OpenFolderOnCreate { get; set; }
    public List<string>? SelectedMSUs { get; set; }
    public string? CreatedMSUPath { get; set; }
    public string? RomPath { get; set; }
    public bool UseFolderNames;
}
