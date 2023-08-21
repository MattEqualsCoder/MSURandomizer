using System;
using System.IO;
using System.Reflection;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

internal class MsuMsuAppSettingsService : IMsuAppSettingsService
{
    private MsuAppSettings _settings { get; set; } = new();
    
    public MsuAppSettings Initialize(Stream stream)
    {
        if (stream == null)
        {
            throw new InvalidOperationException("No MsuRandomizerSettings stream provided");
        }
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        var yamlText = reader.ReadToEnd();
        LoadSettings(yamlText);
        return _settings;
    }

    public MsuAppSettings Initialize(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            LoadSettings(null);
            return _settings;
        }
        
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Could not find MsuRandomizerSettings file {File}", path);
        }
        var yamlText = File.ReadAllText(path);
        LoadSettings(yamlText);
        return _settings;
    }

    private void LoadSettings(string? overrideYaml)
    {
        var stream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream("MSURandomizerLibrary.settings.yaml");
        var settings = new MsuAppSettings();
        if (stream != null)
        {
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            var defaultYaml = reader.ReadToEnd();
            settings = Parse(defaultYaml);
        }

        if (!string.IsNullOrEmpty(overrideYaml))
        {
            var overrideSettings = Parse(overrideYaml);
        
            // Pull in override settings if they are not null
            foreach (var prop in typeof(MsuAppSettings).GetProperties())
            {
                var value = prop.GetValue(overrideSettings);
                if (value != null && prop.CanWrite)
                {
                    prop.SetValue(settings, value);
                }
            }
        }

        if (string.IsNullOrEmpty(settings.UserOptionsFilePath))
        {
            throw new InvalidOperationException("UserOptionsFilePath not specified in app settings file");
        }

        _settings = settings;
    }

    private MsuAppSettings Parse(string yamlText)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        return deserializer.Deserialize<MsuAppSettings>(yamlText);
    }

    public MsuAppSettings MsuAppSettings => _settings;
}