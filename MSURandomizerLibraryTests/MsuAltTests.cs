using System.Security.Cryptography;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibraryTests;

public class MsuAltTests
{
    private MsuTypeService _msuTypeService = null!;
    private MsuLookupService _msuLookupService = null!;
    private MsuAppSettings _appSettings = null!;
    private MsuSelectorService _msuSelectorService = null!;
    private MsuDetailsService _msuDetailsService = null!;
    private Random _random = new Random();
    private ISerializer _serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();
    
    [SetUp]
    public void Setup()
    {
        _appSettings = GetAppSettings();
        _msuTypeService = GetMsuTypeService(null, _appSettings);
        _msuDetailsService = CreateMsuDetailsService();
        _msuLookupService = CreateMsuLookupService(_appSettings);
        _msuSelectorService = CreateMsuSelectorService();
    }

    [Test]
    public void ExecuteTest()
    {
        var msuType = _msuTypeService.GetMsuType("A Link to the Past");
        Assert.IsNotNull(msuType);

        var msuCount = 3;
        var altCount = 2;
        
        TestHelpers.DeleteFolder(TestHelpers.MsuTestFolder);
        CreateMsus(msuType!, msuCount, altCount);

        var msus = _msuLookupService.LookupMsus(TestHelpers.MsuTestFolder);
        VerifyMsus(msus, msuCount, altCount);

        var response = _msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
        {
            Msus = msus.ToList(),
            OutputMsuType = msuType,
            OutputPath = Path.Combine(TestHelpers.MsuTestFolder, "output", "output-msu.msu")
        });

        VerifyResponse(response, msuType!);
    }

    private void VerifyResponse(MsuSelectorResponse response, MsuType msuType)
    {
        Assert.That(response.Msu, Is.Not.Null);

        FileInfo file = new FileInfo(response.Msu!.Path);
        var basePath = file.FullName.Replace(file.Extension, "");
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var yamlText = File.ReadAllText(basePath + ".yml");
        var details = deserializer.Deserialize<MsuDetails>(yamlText);
        Assert.That(details, Is.Not.Null);

        foreach (var track in msuType.Tracks)
        {
            Assert.That(details.Tracks?.ContainsKey(track.YamlNameSecondary ?? track.YamlName!) == true);
            Assert.That(File.Exists(basePath + $"-{track.Number}.pcm"));
        }
    }

    private void VerifyMsus(IReadOnlyCollection<Msu> msus, int msuCount, int altCount)
    {
        Assert.That(msus.Count, Is.EqualTo(msuCount));
        foreach (var msu in msus)
        {
            Assert.That(msu.Tracks.Count, Is.EqualTo(msu.MsuType?.Tracks.Count() * (1+ altCount)));
            foreach (var track in msu.Tracks)
            {
                Assert.That(File.Exists(track.Path));
                Assert.That(!string.IsNullOrEmpty(track.SongName));
            }
        }
    }

    private void CreateMsus(MsuType msuType, int msuCount, int altCount)
    {
        for (int i = 0; i < msuCount; i++)
        {
            CreateMsu(msuType, i, altCount);
        }
    }

    private void CreateMsu(MsuType msuType, int testNumber, int altCount)
    {
        var msuFolder = Path.Combine(TestHelpers.MsuTestFolder, $"msu-{testNumber}");
        Directory.CreateDirectory(msuFolder);
        var msuBasePath = Path.Combine(msuFolder, $"test-{testNumber}-msu");
        var msuPath = $"{msuBasePath}.msu";
        using (File.Create(msuPath)) {}

        var trackDetailsList = new Dictionary<string, MsuDetailsTrack>();

        foreach (var track in msuType.Tracks)
        {
            var text = RandomString(_random.Next(10, 100));
            File.WriteAllText($"{msuBasePath}-{track.Number}.pcm", text);
            GetTrackData($"{msuBasePath}-{track.Number}.pcm", out var fileLength, out var fileHash);

            var trackDetails = new MsuDetailsTrack()
            {
                Name = $"MSU {testNumber} Track {track.Number}",
                Path = $"test-{testNumber}-msu-{track.Number}.pcm",
                FileLength = fileLength,
                Hash = fileHash,
                Alts = new List<MsuDetailsTrack>()
            };

            for (int i = 1; i <= altCount; i++)
            {
                text = RandomString(_random.Next(10, 100));
                File.WriteAllText($"{msuBasePath}-{track.Number}_alt{i}.pcm", text);
                GetTrackData($"{msuBasePath}-{track.Number}_alt{i}.pcm", out fileLength, out fileHash);
                
                trackDetails.Alts.Add(new MsuDetailsTrack()
                {
                    Name = $"MSU {testNumber} Track {track.Number} Alt {i}",
                    Path = $"test-{testNumber}-msu-{track.Number}_Alt{i}.pcm",
                    FileLength = fileLength,
                    Hash = fileHash
                });
            }

            trackDetailsList[track.YamlNameSecondary ?? track.YamlName!] = trackDetails;
        }

        var msuDetails = new MsuDetails()
        {
            PackName = $"Test MSU Pack {testNumber}",
            PackAuthor = $"Test Author {testNumber}",
            PackVersion = $"Test {testNumber}",
            MsuType = msuType.Name,
            Tracks = trackDetailsList
        };

        var yamlText = _serializer.Serialize(msuDetails);
        File.WriteAllText($"{msuBasePath}.yml", yamlText);
    }
    
    private string GetConfigDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory != null ? Path.Combine(directory.FullName, "ConfigRepo", "resources") : "";
    }
    
    private MsuAppSettings GetAppSettings()
    {
        var service = new MsuMsuAppSettingsService();
        var yamlPath = GetAppSettingsPath();
        return service.Initialize(yamlPath);
    }
    
    private string GetAppSettingsPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory != null ? Path.Combine(directory.FullName, "MSURandomizer", "settings.yaml") : "";
    }

    private MsuTypeService GetMsuTypeService(MsuTypeConfig? typeConfig, MsuAppSettings? appSettings)
    {
        var logger = TestHelpers.CreateMockLogger<MsuTypeService>();
        var settingsService = TestHelpers.CreateMsuAppSettingsService(appSettings);
        var msuTypeService = new MsuTypeService(logger, settingsService.MsuAppSettings);
        if (typeConfig == null)
        {
            msuTypeService.LoadMsuTypes(GetConfigDirectory());    
        }

        return msuTypeService;
    }
    
    private MsuLookupService CreateMsuLookupService(MsuAppSettings? appSettings)
    {
        var logger = TestHelpers.CreateMockLogger<MsuLookupService>();
        var msuCacheService = TestHelpers.CreateMockMsuCacheService();
        return new MsuLookupService(logger, _msuTypeService, new MsuUserOptions(), _msuDetailsService, appSettings ?? new MsuAppSettings(), msuCacheService);
    }

    private MsuSelectorService CreateMsuSelectorService()
    {
        var logger = TestHelpers.CreateMockLogger<MsuSelectorService>();
        var msuUserOptionsService = TestHelpers.CreateMockMsuUserOptionsService(null);
        return new MsuSelectorService(logger, _msuDetailsService, _msuTypeService, _msuLookupService, msuUserOptionsService);
    }

    private MsuDetailsService CreateMsuDetailsService()
    {
        var logger = TestHelpers.CreateMockLogger<MsuDetailsService>();
        return new MsuDetailsService(logger);
    }
    
    private string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
    
    private SHA1 _sha1 = SHA1.Create();

    private void GetTrackData(string filename, out long fileLength, out string fileHash)
    {
        fileLength = new FileInfo(filename).Length;
        using var stream1 = File.OpenRead(filename);
        fileHash = BitConverter.ToString(_sha1.ComputeHash(stream1)).Replace("-", "");
    }
    
}