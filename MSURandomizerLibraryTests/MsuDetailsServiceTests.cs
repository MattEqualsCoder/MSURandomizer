using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

public class MsuDetailsServiceTests
{
    private readonly string _msuPath = new FileInfo("unit-test.msu").FullName;
    private readonly string _yamlPath = new FileInfo("unit-test.yml").FullName;
    private bool _yamlExists => new FileInfo(_yamlPath).Exists;
    
    [Test]
    public void GetBasicMsuDetailsTest()
    {
        SaveTestYamlFile(
@"
pack_name: Test MSU Pack
pack_author: Test Author
pack_version: 100
artist: Test Artist
");
        var service = GetMsuDetailsService(null);
        var details = service.GetBasicMsuDetails(_msuPath, out var yamlPath, out var error);
        DeleteYamlFile();
        
        Assert.Multiple(() =>
        {
            Assert.That(_yamlPath, Is.EqualTo(yamlPath), "Generated yaml path is invalid");
            Assert.That(string.IsNullOrEmpty(error), Is.True, "Details load error");
            Assert.That(details, Is.Not.Null, "No basic MSU details loaded");
            Assert.That(details?.PackName, Is.EqualTo("Test MSU Pack"), "Pack name invalid");
            Assert.That(details?.PackAuthor, Is.EqualTo("Test Author"), "Pack author invalid");
            Assert.That(details?.PackVersion, Is.EqualTo(100), "Pack version invalid");
            Assert.That(details?.Artist, Is.EqualTo("Test Artist"), "Artist invalid");
        });
    }
    
    [Test]
    public void SaveMsuDetails()
    {
        var msu = new Msu()
        {
            Name = "Test MSU Pack",
            Creator = "Test Creator",
            FolderName = "Test Folder",
            FileName = "Test File",
            Path = _msuPath,
            Tracks = new List<Track>()
            {
                new Track("Test Track", 1, "Test Song", "Test Path", _msuPath, "Test MSU Pack")
            }
        };
        
        var service = GetMsuDetailsService(null);
        var success = service.SaveMsuDetails(msu, _yamlPath);

        var expectedResult =
            @"tracks:
- track_number: 1
  track_name: Test Track
  name: Test Song
  msu_name: Test MSU Pack
pack_name: Test MSU Pack
pack_author: Test Creator
pack_version: 1
";

        var output = success ? File.ReadAllText(_yamlPath) : "";
        DeleteYamlFile();
        
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True, "Save MSU Details Unsuccessful");
            Assert.That(output, Is.EqualTo(expectedResult), "Invalid YAML text");
        });
    }

    [Test]
    public void LoadMsuDetailsTest()
    {
        SaveTestYamlFile(@"pack_name: Test MSU Pack
pack_author: Test Creator
artist: Test Artist
tracks:
  light_world:
    name: Test Song 1
    album: Test Album 1
    url: Test Url 1
  samus_fanfare:
    name: Test Song 2
    album: Test Album 2
    artist: Test Artist 2");

        var msuType = new MsuType()
        {
            Name = "Test MSU Type",
            DisplayName = "Test MSU Type",
            RequiredTrackNumbers = new HashSet<int>() { 2, 101 },
            ValidTrackNumbers = new HashSet<int>() { 2, 101 },
            Tracks = new List<MsuTypeTrack>()
            {
                new()
                {
                    Number = 2,
                    Name = "Track 2"
                },
                new()
                {
                    Number = 101,
                    Name = "Track 101"
                }
            }
        };
        
        var service = GetMsuDetailsService(new MsuAppSettings()
        {
            Smz3MsuTypes = new List<string> { "Test MSU Type" }
        });

        var basicDetails = service.GetBasicMsuDetails(_msuPath, out _, out var basicError);
        Assert.That(basicDetails, Is.Not.Null);

        var msu = service.LoadMsuDetails(msuType, _msuPath, new FileInfo(_msuPath).DirectoryName!, "unit-test",
            _yamlPath, basicDetails, out var msuDetails, out var error);
        Assert.Multiple(() =>
        {
            Assert.That(msu, Is.Not.Null);
            Assert.That(msuDetails, Is.Not.Null);
            Assert.That(string.IsNullOrEmpty(error), Is.True);
            Assert.That(msu?.Name, Is.EqualTo("Test MSU Pack"));
            Assert.That(msu?.Creator, Is.EqualTo("Test Creator"));
            Assert.That(msu?.Tracks.Count, Is.EqualTo(2));
        });

        var track1 = msu?.Tracks.First();
        Assert.Multiple(() =>
        {
            Assert.That(track1?.SongName, Is.EqualTo("Test Song 1"));
            Assert.That(track1?.Album, Is.EqualTo("Test Album 1"));
            Assert.That(track1?.Artist, Is.EqualTo("Test Artist"));
            Assert.That(track1?.Url, Is.EqualTo("Test Url 1"));
            Assert.That(track1?.TrackName, Is.EqualTo("LightWorld"));
            Assert.That(track1?.Number, Is.EqualTo(2));
        });
        
        var track2 = msu?.Tracks.Last();
        Assert.Multiple(() =>
        {
            Assert.That(track2?.SongName, Is.EqualTo("Test Song 2"));
            Assert.That(track2?.Album, Is.EqualTo("Test Album 2"));
            Assert.That(track2?.Artist, Is.EqualTo("Test Artist 2"));
            Assert.That(track2?.TrackName, Is.EqualTo("SamusFanfare"));
            Assert.That(track2?.Number, Is.EqualTo(101));
        });
    }

    private void SaveTestYamlFile(string text)
    {
        DeleteYamlFile();
        File.WriteAllText(_yamlPath, text);
        Assert.That(_yamlExists, Is.True);
    }
    
    private MsuDetailsService GetMsuDetailsService(MsuAppSettings? appSettings)
    {
        var logger = TestHelpers.CreateMockLogger<MsuDetailsService>();
        var settingsService = TestHelpers.CreateMsuAppSettingsService(appSettings);
        return new MsuDetailsService(logger, settingsService.MsuAppSettings);
    }

    private void DeleteYamlFile()
    {
        if (_yamlExists)
        {
            File.Delete(_yamlPath);
        }
    }
}