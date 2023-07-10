using System;
using System.IO;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

public class MsuMsuAppSettingsService : IMsuAppSettingsService
{
    private MsuAppSettings _settings { get; set; } = new();
    
    public MsuAppSettings Initialize(Stream? stream)
    {
        if (stream == null)
        {
            throw new InvalidOperationException("No MsuRandomizerSettings stream provided");
        }
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        var yamlText = reader.ReadToEnd();
        Parse(yamlText);
        return _settings;
    }

    public MsuAppSettings Initialize(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Could not find MsuRandomizerSettings file {File}", path);
        }
        var yamlText = File.ReadAllText(path);
        Parse(yamlText);
        return _settings;
    }

    private void Parse(string yamlText)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        _settings = deserializer.Deserialize<MsuAppSettings>(yamlText);
    }

    public MsuAppSettings MsuAppSettings => _settings;
}