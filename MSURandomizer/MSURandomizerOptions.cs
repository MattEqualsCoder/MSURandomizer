using System.Collections.Generic;

namespace MsuRandomizerLibrary;

/// <summary>
/// Class with all options for MSU generation
/// </summary>
public class MSURandomizerOptions
{
    public string? Directory { get; set; }
    public string Name { get; set; } = "RandomizedMSU";
    public string? OutputType { get; set; }
    public MsuFilter Filter { get; set; } = MsuFilter.Compatible;
    public bool AvoidDuplicates { get; set; }
    public bool AllowContinuousReshuffle { get; set; } = true;
    public bool ContinuousReshuffle { get; set; }
    public bool OpenFolderOnCreate { get; set; }
    public List<string>? SelectedMSUs { get; set; }
    public string? CreatedMSUPath { get; set; }
    public string? RomPath { get; set; }
    public string? MsuTypeConfigPath { get; set; }
    public bool UseFolderNames;
    public string? ForcedMsuType { get; set; }
    public bool AllowMSUFolderChange { get; set; } = true;
    public bool DeleteFolder { get; set; } = true;
}
