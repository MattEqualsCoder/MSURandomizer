using System.Globalization;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibraryTests;

[NonParallelizable]
public class MsuTypeYamlTests
{
    private MsuTypeService _msuTypeService = null!;
    private MsuAppSettings _appSettings = null!;
    private MsuLookupService _msuLookupService = null!;
    private string _outputFolder = new DirectoryInfo("GeneratedYaml").FullName;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly ISerializer _serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();
    
    [SetUp]
    public void Setup()
    {
        _appSettings = GetAppSettings();
        _msuTypeService = GetMsuTypeService(null, _appSettings);
        _msuLookupService = CreateMsuLookupService(_appSettings);
        _outputFolder = GetYamlPath();

        if (!Directory.Exists(_outputFolder))
        {
            Directory.CreateDirectory(_outputFolder);
        }
    }

    [Test]
    [Retry(5)]
    public void CreateYamlFiles()
    {
        foreach (var file in Directory.EnumerateFiles(_outputFolder))
        {
            File.Delete(file);
        }
        
        foreach (var msuType in _msuTypeService.MsuTypes)
        {
            var tracks = new Dictionary<string, MsuDetailsTrack>();

            foreach (var track in msuType.Tracks.OrderBy(x => x.Number))
            {
                tracks[track.YamlNameSecondary ?? track.YamlName!] = new MsuDetailsTrack()
                {
                    TrackNumber = track.Number,
                    Name = "",
                    Artist = "",
                    Album = "",
                    Url = ""
                };
            }

            var msuDetails = new MsuDetails()
            {
                PackName = "",
                PackAuthor = "",
                PackVersion = "",
                MsuType = msuType.Name,
                Tracks = tracks
            };

            var outputPath = GetMsuYamlPath(msuType);
            var yaml = _serializer.Serialize(msuDetails).Replace("''", "");
            File.WriteAllText(outputPath, yaml);
        }
    }

    [Test]
    [Retry(5)]
    public void TestYamlFiles()
    {
        foreach (var msuType in _msuTypeService.MsuTypes)
        {
            var testValue = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + " - " + msuType.Name;
            
            var details = GetDetails(msuType);

            Assert.That(details.Tracks, Is.Not.Null);
            
            details.PackName = $"Name {testValue}";
            details.PackAuthor = $"Author {testValue}";
            details.PackVersion = $"Version {testValue}";

            foreach (var track in details.Tracks!.Values)
            {
                var msuTypeTrack = msuType.Tracks.FirstOrDefault(x => x.Number == track.TrackNumber);
                Assert.That(msuTypeTrack, Is.Not.Null);
                
                track.Name = $"Song Name {msuTypeTrack?.Name} {testValue}";
                track.Artist = $"Artist {msuTypeTrack?.Name} {testValue}";
                track.Album = $"Album {msuTypeTrack?.Name} {testValue}";
                track.Url = $"Url {msuTypeTrack?.Name} {testValue}";
            }

            var msuPath = TestHelpers.CreateMsu(msuType.Tracks.Select(x => x.Number).ToList());

            SaveDetails(details, msuPath.Replace(".msu", ".yml"));

            var msu = _msuLookupService.LoadMsu(msuPath, msuType);
            Assert.That(msu, Is.Not.Null);
            
            Assert.Multiple(() =>
            {
                Assert.That(msu!.DisplayName, Is.EqualTo($"Name {testValue}"));
                Assert.That(msu.DisplayCreator, Is.EqualTo($"Author {testValue}"));
                Assert.That(msu.Version, Is.EqualTo($"Version {testValue}"));
            });
            
            foreach (var track in msu!.Tracks)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(track.SongName, Is.EqualTo($"Song Name {track.TrackName} {testValue}"));
                    Assert.That(track.Artist, Is.EqualTo($"Artist {track.TrackName} {testValue}"));
                    Assert.That(track.Album, Is.EqualTo($"Album {track.TrackName} {testValue}"));
                    Assert.That(track.Url, Is.EqualTo($"Url {track.TrackName} {testValue}"));
                });
            }
        }
    }

    private MsuDetails GetDetails(MsuType type)
    {
        var path = GetMsuYamlPath(type);
        var text = File.ReadAllText(path);
        return _deserializer.Deserialize<MsuDetails>(text);
    }
    
    private void SaveDetails(MsuDetails details, string path)
    {
        var value = _serializer.Serialize(details);
        File.WriteAllText(path, value);
    }

    private string GetMsuYamlPath(MsuType type)
    {
        var sanitizedName = MsuTypeService.TrackYamlNameRegex.Replace(type.Name, "").Replace("  ", " ");
        return Path.Combine(_outputFolder, $"{sanitizedName}.yml");
    }
    
    private MsuAppSettings GetAppSettings()
    {
        var service = new MsuMsuAppSettingsService();
        return service.Initialize("");
    }
    
    private string GetYamlPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        if (directory != null)
            return Path.Combine(directory.FullName, "Docs", "YamlTemplates");
        return _outputFolder;
    }

    private MsuTypeService GetMsuTypeService(MsuTypeConfig? typeConfig, MsuAppSettings? appSettings)
    {
        var logger = TestHelpers.CreateMockLogger<MsuTypeService>();
        var settingsService = TestHelpers.CreateMsuAppSettingsService(appSettings);
        var msuTypeService = new MsuTypeService(logger, settingsService);
        if (typeConfig == null)
        {
            msuTypeService.LoadMsuTypes();    
        }

        return msuTypeService;
    }
    
    private MsuLookupService CreateMsuLookupService(MsuAppSettings? appSettings)
    {
        var logger = TestHelpers.CreateMockLogger<MsuLookupService>();
        var msuOptions = TestHelpers.CreateMockMsuUserOptionsService(null);
        var detailsService = new MsuDetailsService(TestHelpers.CreateMockLogger<MsuDetailsService>(), msuOptions);
        var msuCacheService = TestHelpers.CreateMockMsuCacheService();
        var msuUserOptionsService = TestHelpers.CreateMockMsuUserOptionsService(null);
        return new MsuLookupService(logger, _msuTypeService, detailsService, appSettings ?? new MsuAppSettings(), msuCacheService, msuUserOptionsService);
    }
}