using System.Security.Cryptography;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibraryTests;

public class MsuEndtoEndTests : IDisposable
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
    public void ShuffledMsuTest()
    {
        var msuType = _msuTypeService.GetMsuType("A Link to the Past");
        Assert.That(msuType, Is.Not.Null);

        var msuCount = 3;
        var altCount = 2;
        var addSongInfo = false;
        
        TestHelpers.DeleteFolder(TestHelpers.MsuTestFolder);
        CreateMsus(msuType!, msuCount, altCount, addSongInfo);

        var msus = _msuLookupService.LookupMsus(TestHelpers.MsuTestFolder);
        VerifyMsus(msus, msuCount, altCount, addSongInfo);

        var response = _msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
        {
            Msus = msus.ToList(),
            OutputMsuType = msuType,
            OutputPath = Path.Combine(TestHelpers.MsuTestFolder, "output", "output-msu.msu")
        });

        VerifyResponse(response, msuType!, addSongInfo, true, false, true, false);
    }
    
    [Test]
    public void SingleShuffledMsuTest()
    {
        var msuType = _msuTypeService.GetMsuType("A Link to the Past");
        Assert.That(msuType, Is.Not.Null);

        var msuCount = 1;
        var altCount = 2;
        var addSongInfo = true;
        
        TestHelpers.DeleteFolder(TestHelpers.MsuTestFolder);
        CreateMsus(msuType!, msuCount, altCount, addSongInfo);

        var msus = _msuLookupService.LookupMsus(TestHelpers.MsuTestFolder);
        VerifyMsus(msus, msuCount, altCount, addSongInfo);

        var response = _msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
        {
            Msus = msus.ToList(),
            OutputMsuType = msuType,
            OutputPath = Path.Combine(TestHelpers.MsuTestFolder, "output", "output-msu.msu")
        });

        VerifyResponse(response, msuType!, addSongInfo, true, false, false, false);
    }
    
    [Test]
    public void AssignMsuTest()
    {
        var msuType = _msuTypeService.GetMsuType("A Link to the Past");
        Assert.That(msuType, Is.Not.Null);

        var msuCount = 1;
        var altCount = 2;
        var addSongInfo = true;
        
        TestHelpers.DeleteFolder(TestHelpers.MsuTestFolder);
        CreateMsus(msuType!, msuCount, altCount, addSongInfo);

        var msus = _msuLookupService.LookupMsus(TestHelpers.MsuTestFolder);
        VerifyMsus(msus, msuCount, altCount, addSongInfo);

        var response = _msuSelectorService.AssignMsu(new MsuSelectorRequest()
        {
            Msu = msus.First(),
            OutputMsuType = msuType,
            OutputPath = Path.Combine(TestHelpers.MsuTestFolder, "output", "output-msu.msu")
        });

        VerifyResponse(response, msuType!, addSongInfo, false, false, false, true);
    }
    
    [Test]
    public void RandomMsuTest()
    {
        var msuType = _msuTypeService.GetMsuType("A Link to the Past");
        Assert.That(msuType, Is.Not.Null);

        var msuCount = 2;
        var altCount = 2;
        var addSongInfo = false;
        
        TestHelpers.DeleteFolder(TestHelpers.MsuTestFolder);
        CreateMsus(msuType!, msuCount, altCount, addSongInfo);

        var msus = _msuLookupService.LookupMsus(TestHelpers.MsuTestFolder);
        VerifyMsus(msus, msuCount, altCount, addSongInfo);

        var response = _msuSelectorService.PickRandomMsu(new MsuSelectorRequest()
        {
            Msus = msus.ToList(),
            OutputMsuType = msuType,
            OutputPath = Path.Combine(TestHelpers.MsuTestFolder, "output", "output-msu.msu")
        });

        VerifyResponse(response, msuType!, addSongInfo, false, false, false, false);
    }
    
    [Test]
    public void SaveMsuTest()
    {
        var msuType = _msuTypeService.GetMsuType("A Link to the Past");
        Assert.That(msuType, Is.Not.Null);

        var msuCount = 1;
        var altCount = 2;
        var addSongInfo = false;
        
        TestHelpers.DeleteFolder(TestHelpers.MsuTestFolder);
        CreateMsus(msuType!, msuCount, altCount, addSongInfo);

        var msus = _msuLookupService.LookupMsus(TestHelpers.MsuTestFolder);
        VerifyMsus(msus, msuCount, altCount, addSongInfo);

        var response = _msuSelectorService.SaveMsu(new MsuSelectorRequest()
        {
            Msu = msus.First(),
            OutputPath = Path.Combine(TestHelpers.MsuTestFolder, "output", "output-msu.msu")
        });

        VerifyResponse(response, msuType!, addSongInfo, false, true, false, false);
    }

    private void VerifyResponse(MsuSelectorResponse response, MsuType msuType, bool addSongInfo, bool shouldhaveMsuDetails, bool shouldHaveAlts, bool isShuffled, bool allBaseTracks)
    {
        Assert.That(response.Msu, Is.Not.Null);

        FileInfo file = new FileInfo(response.Msu!.Path);
        var folder = file.DirectoryName;
        var basePath = file.FullName.Replace(file.Extension, "");
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var yamlText = File.ReadAllText(basePath + ".yml");
        var details = deserializer.Deserialize<MsuDetails>(yamlText);
        Assert.That(details, Is.Not.Null);

        if (allBaseTracks)
        {
            Assert.That(details.Tracks!.All(x => !x.Value.Name!.Contains("Alt")));
        }
        else
        {
            Assert.That(details.Tracks!.Any(x => x.Value.Name!.Contains("Alt")));    
        }
        
        if (!isShuffled && !addSongInfo)
        {
            Assert.That(!string.IsNullOrEmpty(details.Artist), $"MSU has Artist '{details.Artist}'");
            Assert.That(!string.IsNullOrEmpty(details.Album), $"MSU has Album '{details.Album}'");
            Assert.That(!string.IsNullOrEmpty(details.Url), $"MSU has Url '{details.Url}'");
        }
        else
        {
            Assert.That(string.IsNullOrEmpty(details.Artist), $"MSU has Artist '{details.Artist}'");
            Assert.That(string.IsNullOrEmpty(details.Album), $"MSU has Album '{details.Album}'");
            Assert.That(string.IsNullOrEmpty(details.Url), $"MSU has Url '{details.Url}'");
        }

        foreach (var track in msuType.Tracks)
        {
            Assert.That(details.Tracks?.ContainsKey(track.YamlNameSecondary ?? track.YamlName!) == true);
            var trackDetails = details.Tracks![track.YamlNameSecondary ?? track.YamlName!];
            Assert.That(File.Exists(basePath + $"-{track.Number}.pcm"));
            CheckTrackBasicDetails(trackDetails, addSongInfo, shouldhaveMsuDetails, shouldHaveAlts, isShuffled);
            
            if (!shouldHaveAlts)
            {
                Assert.That(!trackDetails.Alts?.Any() != true);
                Assert.That(trackDetails.FileLength, Is.Null);
                Assert.That(trackDetails.Hash, Is.Null);
                Assert.That(trackDetails.Path, Is.Null);
            }
            else
            {
                Assert.That(trackDetails.Alts?.Any() == true);
                Assert.That(File.Exists(Path.Combine(folder ?? "", trackDetails.Path ?? "")));
                GetTrackData(Path.Combine(folder ?? "", trackDetails.Path ?? ""), out long fileLength,
                    out string fileHash);
                Assert.That(trackDetails.FileLength, Is.EqualTo(fileLength));
                Assert.That(trackDetails.Hash, Is.EqualTo(fileHash));

                foreach (var alt in trackDetails.Alts!)
                {
                    Assert.That(File.Exists(Path.Combine(folder ?? "", alt.Path ?? "")));
                    CheckTrackBasicDetails(alt, addSongInfo, shouldhaveMsuDetails, shouldHaveAlts, isShuffled);
                    GetTrackData(Path.Combine(folder ?? "", alt.Path ?? ""), out fileLength,
                        out fileHash);
                    Assert.That(alt.FileLength, Is.EqualTo(fileLength));
                    Assert.That(alt.Hash, Is.EqualTo(fileHash));
                }
            }
        }
    }

    private void CheckTrackBasicDetails(MsuDetailsTrack trackDetails, bool addSongInfo, bool shouldhaveMsuDetails, bool shouldHaveAlts, bool isShuffled)
    {
        Assert.That(!string.IsNullOrEmpty(trackDetails.Name));

        if (isShuffled || addSongInfo)
        {
            Assert.That(!string.IsNullOrEmpty(trackDetails.Artist), $"Song has Artist '{trackDetails.Artist}'");
            Assert.That(!string.IsNullOrEmpty(trackDetails.Album), $"Song has Album '{trackDetails.Album}'");
            Assert.That(!string.IsNullOrEmpty(trackDetails.Url), $"Song has Url '{trackDetails.Url}'");
        }
            
        if (addSongInfo)
        {
            Assert.That(trackDetails.Artist?.Contains("Test Song Artist") == true, $"Song has Artist '{trackDetails.Artist}'");
            Assert.That(trackDetails.Album?.Contains("Test Song Album") == true, $"Song has Album '{trackDetails.Album}'");
            Assert.That(trackDetails.Url?.Contains("Test Song Url") == true, $"Song has Url '{trackDetails.Url}'");
        }
        else if(isShuffled)
        {
            Assert.That(trackDetails.Artist?.Contains("Test Artist") == true, $"Song has Artist '{trackDetails.Artist}'");
            Assert.That(trackDetails.Album?.Contains("Test Album") == true, $"Song has Album '{trackDetails.Album}'");
            Assert.That(trackDetails.Url?.Contains("Test Url") == true, $"Song has Url '{trackDetails.Url}'");
        }

        if (shouldhaveMsuDetails)
        {
            Assert.That(!string.IsNullOrEmpty(trackDetails.MsuName), $"Song has MsuName '{trackDetails.MsuName}'");
            Assert.That(!string.IsNullOrEmpty(trackDetails.MsuAuthor), $"Song has MsuAuthor '{trackDetails.MsuAuthor}'");    
        }
        else
        {
            Assert.That(string.IsNullOrEmpty(trackDetails.MsuName), $"Song has MsuName '{trackDetails.MsuName}'");
            Assert.That(string.IsNullOrEmpty(trackDetails.MsuAuthor), $"Song has MsuAuthor '{trackDetails.MsuAuthor}'");    
        }
    }
    
    

    private void VerifyMsus(IReadOnlyCollection<Msu> msus, int msuCount, int altCount, bool addSongInfo)
    {
        Assert.That(msus.Count, Is.EqualTo(msuCount));
        foreach (var msu in msus)
        {
            Assert.That(msu.Tracks.Count, Is.EqualTo(msu.MsuType?.Tracks.Count() * (1+ altCount)));

            if (addSongInfo)
            {
                Assert.That(string.IsNullOrEmpty(msu.Artist));
                Assert.That(string.IsNullOrEmpty(msu.Album));
                Assert.That(string.IsNullOrEmpty(msu.Url));
            }
            else
            {
                Assert.That(!string.IsNullOrEmpty(msu.Artist));
                Assert.That(!string.IsNullOrEmpty(msu.Album));
                Assert.That(!string.IsNullOrEmpty(msu.Url));
            }
            
            foreach (var track in msu.Tracks)
            {
                Assert.That(!string.IsNullOrEmpty(track.Path));
                Assert.That(File.Exists(track.Path));
                Assert.That(!string.IsNullOrEmpty(track.SongName));
                
                if (addSongInfo)
                {
                    Assert.That(!string.IsNullOrEmpty(track.Artist), $"Track has Artist '{track.Artist}'");
                    Assert.That(!string.IsNullOrEmpty(track.Album), $"Track has Album '{track.Album}'");
                    Assert.That(!string.IsNullOrEmpty(track.Url), $"Track has Url '{track.Url}'");
                    Assert.That(track.DisplayArtist?.Contains("Test Song Artist") == true);
                    Assert.That(track.DisplayAlbum?.Contains("Test Song Album") == true);
                    Assert.That(track.DisplayUrl?.Contains("Test Song Url") == true);
                }
                else
                {
                    Assert.That(string.IsNullOrEmpty(track.Artist), $"Track has Artist '{track.Artist}'");
                    Assert.That(string.IsNullOrEmpty(track.Album), $"Track has Album '{track.Album}'");
                    Assert.That(string.IsNullOrEmpty(track.Url), $"Track has Url '{track.Url}'");
                    Assert.That(track.DisplayArtist?.Contains("Test Artist") == true);
                    Assert.That(track.DisplayAlbum?.Contains("Test Album") == true);
                    Assert.That(track.DisplayUrl?.Contains("Test Url") == true);
                }
            }
        }
    }

    private void CreateMsus(MsuType msuType, int msuCount, int altCount, bool addSongInfo)
    {
        for (int i = 0; i < msuCount; i++)
        {
            CreateMsu(msuType, i, altCount, addSongInfo);
        }
    }

    private void CreateMsu(MsuType msuType, int testNumber, int altCount, bool addSongInfo)
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
                Artist = addSongInfo ? $"Test Song Artist {testNumber}" : null,
                Album = addSongInfo ? $"Test Song Album {testNumber}" : null,
                Url = addSongInfo ? $"Test Song Url {testNumber}" : null,
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
                    Path = $"test-{testNumber}-msu-{track.Number}_alt{i}.pcm",
                    Artist = addSongInfo ? $"Test Song Artist {testNumber}" : null,
                    Album = addSongInfo ? $"Test Song Album {testNumber}" : null,
                    Url = addSongInfo ? $"Test Song Url {testNumber}" : null,
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
            Artist = !addSongInfo ? $"Test Artist {testNumber}" : null,
            Album = !addSongInfo ? $"Test Album {testNumber}" : null,
            Url = !addSongInfo ? $"Test Url {testNumber}" : null,
            MsuType = msuType.Name,
            Tracks = trackDetailsList
        };

        var yamlText = _serializer.Serialize(msuDetails);
        File.WriteAllText($"{msuBasePath}.yml", yamlText);
    }
    
    private MsuAppSettings GetAppSettings()
    {
        var service = new MsuMsuAppSettingsService();
        return service.Initialize("");
    }
    
    private MsuTypeService GetMsuTypeService(MsuTypeConfig? typeConfig, MsuAppSettings? appSettings)
    {
        var logger = TestHelpers.CreateMockLogger<MsuTypeService>();
        var settingsService = TestHelpers.CreateMsuAppSettingsService(appSettings);
        var msuTypeService = new MsuTypeService(logger, settingsService.MsuAppSettings);
        if (typeConfig == null)
        {
            msuTypeService.LoadMsuTypes();    
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

    public void Dispose()
    {
        _sha1.Dispose();
        GC.SuppressFinalize(this);
    }
}