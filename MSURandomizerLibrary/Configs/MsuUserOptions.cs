using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Configs;

/// <summary>
/// Class with all options for MSU generation
/// </summary>
public class MsuUserOptions
{
    public string? DefaultMsuPath { get; set; }
    public string Name { get; set; } = "RandomizedMSU";
    public string? OutputMsuType { get; set; }
    public MsuFilter Filter { get; set; } = MsuFilter.Compatible;
    public bool AvoidDuplicates { get; set; }
    public bool AllowContinuousReshuffle { get; set; } = true;
    public bool OpenFolderOnCreate { get; set; }
    public ICollection<string>? SelectedMsus { get; set; }
    public string? OutputRomPath { get; set; }
    public string? OutputFolderPath { get; set; }
    public bool PromptOnUpdate { get; set; } = true;
    public bool PromptOnPreRelease { get; set; }
    public Dictionary<string, string> MsuTypeNamePaths { get; set;  } = new();
    public ICollection<MsuSettings> MsuSettings { get; set; } = new List<MsuSettings>();
    
    public MsuRandomizationStyle RandomizationStyle { get; set; }
    
    [YamlIgnore] public Dictionary<MsuType, string> MsuTypePaths { get; set; } = new();
    
    public MsuSettings GetMsuSettings(string path)
    {
        return MsuSettings.FirstOrDefault(x => x.MsuPath == path) ?? new MsuSettings(path);
    }

    public bool AnyMsuPaths => !string.IsNullOrEmpty(DefaultMsuPath) ||
                               MsuTypeNamePaths.Values.Any(x => !string.IsNullOrEmpty(x));
}
