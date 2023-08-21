using System;
using System.IO;
using System.Linq;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

internal class MsuUserOptionsService : IMsuUserOptionsService
{
    private static MsuUserOptions? _options { get; set; }
    private readonly IMsuTypeService _msuTypeService;
    private string _settingsFilePath = "";
    private readonly ISerializer _serializer;
    
    public MsuUserOptionsService(IMsuTypeService msuTypeService)
    {
        _msuTypeService = msuTypeService;
        _serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
    }

    public MsuUserOptions Initialize(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
        
        if (!File.Exists(settingsFilePath))
        {
            _options = new MsuUserOptions();
            Save();
        }
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var yaml = File.ReadAllText(_settingsFilePath);
        _options = deserializer.Deserialize<MsuUserOptions>(yaml);
        
        foreach (var msuTypeDirectory in _options.MsuTypeNamePaths)
        {
            var msuType = _msuTypeService.MsuTypes.FirstOrDefault(x => x.DisplayName == msuTypeDirectory.Key);
            if (msuType != null)
            {
                _options.MsuTypePaths[msuType] = msuTypeDirectory.Value;
            }
        }

        return _options;
    }

    public MsuUserOptions MsuUserOptions => _options ?? throw new InvalidOperationException("User options not loaded");

    public void Save()
    {
        if (_options == null) return;
        var yaml = _serializer.Serialize(_options);
        File.WriteAllText(_settingsFilePath, yaml);
    }

    public void SaveMsuSettings(Msu msu)
    {
        var previousSettings = MsuUserOptions.MsuSettings.FirstOrDefault(x => x.MsuPath == msu.Path);
        
        if (previousSettings == null && msu.Settings.HasSettings)
        {
            MsuUserOptions.MsuSettings.Add(msu.Settings);
        }
        else if (previousSettings != null && !msu.Settings.HasSettings)
        {
            MsuUserOptions.MsuSettings.Remove(previousSettings);
        }
        else if (previousSettings != null && previousSettings != msu.Settings)
        {
            MsuUserOptions.MsuSettings.Remove(previousSettings);
            MsuUserOptions.MsuSettings.Add(msu.Settings);
        }

        Save();
    }
}