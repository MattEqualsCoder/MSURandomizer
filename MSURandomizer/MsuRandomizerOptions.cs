using System;
using System.Collections.Generic;
using System.IO;
using MSURandomizerLibrary;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MsuRandomizer;

/// <summary>
/// Class with all options for MSU generation
/// </summary>
public class MsuRandomizerOptions
{
    public string? DefaultMsuPath { get; set; }
    public string Name { get; set; } = "RandomizedMSU";
    public string? OutputMsuType { get; set; }
    public MsuFilter Filter { get; set; } = MsuFilter.Compatible;
    public bool AvoidDuplicates { get; set; }
    public bool AllowContinuousReshuffle { get; set; } = true;
    public bool ContinuousReshuffle { get; set; }
    public bool OpenFolderOnCreate { get; set; }
    public List<string>? SelectedMsus { get; set; }
    public string? OutputRomPath { get; set; }
    public string? OutputFolderPath { get; set; }
    
    private readonly ISerializer _serializer;
    
    public MsuRandomizerOptions()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
    }

    public static MsuRandomizerOptions LoadOptions()
    {
        var optionsPath = GetConfigPath();
            
        if (!File.Exists(optionsPath))
        {
            var options = new MsuRandomizerOptions();
            options.SaveOptions();
            return options;
        }
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var yaml = File.ReadAllText(optionsPath);
        return deserializer.Deserialize<MsuRandomizerOptions>(yaml);
    }
    
    public void SaveOptions()
    {
        var yaml = _serializer.Serialize(this);
        File.WriteAllText(GetConfigPath(), yaml);
    }
    
    private static string GetConfigPath()
    {
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MSURandomizer");
        Directory.CreateDirectory(basePath);
        return Path.Combine(basePath, "options.yml");
    }
}
