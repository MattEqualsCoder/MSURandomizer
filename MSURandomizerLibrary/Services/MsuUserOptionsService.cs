using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibrary.Services;

internal class MsuUserOptionsService : IMsuUserOptionsService
{
    private static MsuUserOptions? _options { get; set; }
    private ILogger<MsuUserOptionsService> _logger;
    private readonly IMsuTypeService _msuTypeService;
    private string _settingsFilePath = "";
    private readonly ISerializer _serializer;
    
    public MsuUserOptionsService(IMsuTypeService msuTypeService, ILogger<MsuUserOptionsService> logger)
    {
        _msuTypeService = msuTypeService;
        _logger = logger;
        _serializer = new SerializerBuilder()
            .WithQuotingNecessaryStrings()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
    }

    public MsuUserOptions Initialize(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
        
        if (!File.Exists(settingsFilePath))
        {
            _logger.LogInformation("Creating new user settings file at {Path}", settingsFilePath);
            _options = new MsuUserOptions();
            Save();
        }
        
        _logger.LogInformation("Loading user settings file at {Path}", settingsFilePath);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var yaml = File.ReadAllText(_settingsFilePath);
        _options = deserializer.Deserialize<MsuUserOptions>(yaml);

        // If there are no MSU directories, see if there are old settings that need to be updated
        if (_options.MsuDirectories.Count == 0)
        {
            var addedPaths = new List<string>();
            if (!string.IsNullOrEmpty(_options.DefaultMsuPath) && Directory.Exists(_options.DefaultMsuPath))
            {
                var smz3Type = _msuTypeService.GetSMZ3MsuType();
                if (smz3Type != null)
                {
                    _options.MsuDirectories.Add(_options.DefaultMsuPath, smz3Type.DisplayName);
                    addedPaths.Add(_options.DefaultMsuPath);
                }
            }

            foreach (var prevSetting in _options.MsuTypeNamePaths)
            {
                if (addedPaths.Contains(prevSetting.Value))
                {
                    continue;
                }
                
                _options.MsuDirectories.Add(prevSetting.Value, prevSetting.Key);
                addedPaths.Add(prevSetting.Value);
            }

            if (addedPaths.Count > 0)
            {
                _options.DefaultMsuPath = null;
                _options.MsuTypeNamePaths = [];
                _options.MsuTypePaths = [];
                Save();
            }
            
        }
        
        return _options;
    }

    public MsuUserOptions MsuUserOptions => _options ?? throw new InvalidOperationException("User options not loaded");

    public void Save()
    {
        if (_options == null) return;
        
        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var yaml = _serializer.Serialize(_options);
        File.WriteAllText(_settingsFilePath, yaml);
    }

    public void SaveMsuSettings(Msu msu)
    {
        UpdateMsuSettings(msu);
        Save();
    }
    
    public void UpdateMsuSettings(Msu msu)
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
    }
}