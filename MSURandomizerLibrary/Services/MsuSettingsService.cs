using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsuRandomizerLibrary.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MsuRandomizerLibrary.Services;

public class MsuSettingsService : IMsuSettingsService
{
    private string _path = "";
    private List<MsuSettings> _settings = new();
    private readonly ISerializer _serializer;

    public MsuSettingsService()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
    }

    public void InitializeSettingsService(string path)
    {
        _path = path;
        if (File.Exists(path))
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            var yaml = File.ReadAllText(path);
            _settings = deserializer.Deserialize<ICollection<MsuSettings>>(yaml).ToList();
        }
    }

    public void UpdateMsuSettings(Msu msu, MsuSettings settings)
    {
        settings.MsuPath = msu.Path;
        if (_settings.Any(x => settings.MsuPath == msu.Path))
        {
            _settings.Remove(_settings.First(x => settings.MsuPath == msu.Path));
        }
        _settings.Add(settings);
        var yaml = _serializer.Serialize(_settings);
        File.WriteAllText(_path, yaml);
    }

    public MsuSettings GetMsuSettings(Msu msu)
    {
        return GetMsuSettings(msu.Path);
    }
    
    public MsuSettings GetMsuSettings(string msuPath)
    {
        return _settings.FirstOrDefault(x => x.MsuPath == msuPath) ?? new MsuSettings { MsuPath = msuPath };
    }
}