using System.Text;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

[NonParallelizable]
public class MsuTypeServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void LoadMsuTypesTest()
    {
        var msuTypeService = GetMsuTypeService();
        Assert.That(msuTypeService.MsuTypes, Is.Not.Empty, "No MSU types loaded");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "The Legend of Zelda: A Link to the Past"), Is.True, "MSU type list missing A Link to the Past");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "Super Metroid"), Is.True, "MSU type list missing Super Metroid");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer"), Is.True, "MSU type list missing SMZ3");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer Legacy"), Is.True, "MSU type list missing SMZ3 Legacy");

        var quadRando = msuTypeService.GetMsuType("Quad rando");
        if (quadRando != null)
        {
            var expectedTracks = Enumerable.Range(1, 61)
                .Append(99)
                .Concat(Enumerable.Range(101, 41))
                .Concat(Enumerable.Range(201, 18))
                .Concat(Enumerable.Range(301, 15));
            Assert.That(quadRando.ValidTrackNumbers, Is.EquivalentTo(expectedTracks), "Quad rando copied tracks are incomplete");
        }
    }
    
    [Test]
    public void GetMsuTypeTest()
    {
        var msuTypeService = GetMsuTypeService(new MsuAppSettings()
        {
            MsuTypeNameOverrides = new Dictionary<string, string>()
            {
                {"The Legend of Zelda: A Link to the Past", "LTTP"},
            }
        });
        
        Assert.That(msuTypeService.GetMsuType("The Legend of Zelda: A Link to the Past"), Is.Not.Null, "Could not find MSU type by \"The Legend of Zelda: A Link to the Past\"");
        Assert.That(msuTypeService.GetMsuType("LTTP"), Is.Not.Null, "Could not find MSU type by \"LTTP\"");
    }
    
    [Test]
    public void GetMsuTypeNameTest()
    {
        var msuTypeService = GetMsuTypeService(new MsuAppSettings()
        {
            MsuTypeNameOverrides = new Dictionary<string, string>()
            {
                {"The Legend of Zelda: A Link to the Past", "LTTP"},
            }
        });

        var lttpType = msuTypeService.GetMsuType("LTTP");
        Assert.That(msuTypeService.GetMsuTypeName(lttpType), Is.EqualTo("LTTP"));
        Assert.That(msuTypeService.GetMsuTypeName(null), Is.EqualTo("Unknown"));
    }

    [Test]
    public void LoadMsuTypesResolvesNestedCopiesBeforeConsumers()
    {
        using var stream = JsonStream("""
        [
          {
            "meta": { "name": "Leaf", "path": "leaf" },
            "tracks": { "basic": [{ "num": 1, "title": "Leaf" }] },
            "copy": [{ "msu": "middle", "modifier": 200 }]
          },
          {
            "meta": { "name": "Middle", "path": "middle" },
            "tracks": { "basic": [{ "num": 1, "title": "Middle" }] },
            "copy": [{ "msu": "base", "modifier": 100 }]
          },
          {
            "meta": { "name": "Base", "path": "base" },
            "tracks": { "basic": [{ "num": 1, "title": "Base" }] }
          }
        ]
        """);

        var leaf = GetMsuTypeService(stream: stream).GetMsuType("Leaf");

        Assert.That(leaf, Is.Not.Null);
        Assert.That(leaf!.ValidTrackNumbers, Is.EquivalentTo(new[] { 1, 201, 301 }));
    }

    [Test]
    public void LoadMsuTypesRejectsMissingCopyTarget()
    {
        using var stream = JsonStream("""
        [{
          "meta": { "name": "Leaf", "path": "leaf" },
          "tracks": { "basic": [{ "num": 1, "title": "Leaf" }] },
          "copy": [{ "msu": "missing" }]
        }]
        """);

        var exception = Assert.Throws<InvalidOperationException>(() => GetMsuTypeService(stream: stream));

        Assert.That(exception!.Message, Does.Contain("Leaf references missing MSU type missing"));
    }

    [Test]
    public void LoadMsuTypesRejectsCopyCycles()
    {
        using var stream = JsonStream("""
        [
          {
            "meta": { "name": "First", "path": "first" },
            "tracks": { "basic": [{ "num": 1, "title": "First" }] },
            "copy": [{ "msu": "second" }]
          },
          {
            "meta": { "name": "Second", "path": "second" },
            "tracks": { "basic": [{ "num": 1, "title": "Second" }] },
            "copy": [{ "msu": "first" }]
          }
        ]
        """);

        var exception = Assert.Throws<InvalidOperationException>(() => GetMsuTypeService(stream: stream));

        Assert.That(exception!.Message, Does.StartWith("Circular MSU type copy detected"));
    }
    
    private MsuTypeService GetMsuTypeService(MsuAppSettings? appSettings = null, Stream? stream = null)
    {
        var logger = TestHelpers.CreateMockLogger<MsuTypeService>();
        var settingsService = TestHelpers.CreateMsuAppSettingsService(appSettings);
        var msuTypeService = new MsuTypeService(logger, settingsService);
        if (stream == null) msuTypeService.LoadMsuTypes();
        else msuTypeService.LoadMsuTypes(stream);

        return msuTypeService;
    }

    private static MemoryStream JsonStream(string json) => new(Encoding.UTF8.GetBytes(json));
}
