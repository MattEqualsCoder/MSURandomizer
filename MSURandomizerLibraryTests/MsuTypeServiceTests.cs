using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

public class MsuTypeServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void LoadMsuTypesTest()
    {
        var msuTypeService = GetMsuTypeService(null, null);
        Assert.IsNotEmpty(msuTypeService.MsuTypes, "No MSU types loaded");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "The Legend of Zelda: A Link to the Past"), Is.True, "MSU type list missing A Link to the Past");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "Super Metroid"), Is.True, "MSU type list missing Super Metroid");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer"), Is.True, "MSU type list missing SMZ3");
        Assert.That(msuTypeService.MsuTypes.Any(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer Legacy"), Is.True, "MSU type list missing SMZ3 Legacy");
    }
    
    [Test]
    public void GetMsuTypeTest()
    {
        var msuTypeService = GetMsuTypeService(null, new MsuAppSettings()
        {
            MsuTypeNameOverrides = new Dictionary<string, string>()
            {
                {"The Legend of Zelda: A Link to the Past", "LTTP"},
            }
        });
        
        Assert.IsNotNull(msuTypeService.GetMsuType("The Legend of Zelda: A Link to the Past"), "Could not find MSU type by \"The Legend of Zelda: A Link to the Past\"");
        Assert.IsNotNull(msuTypeService.GetMsuType("LTTP"), "Could not find MSU type by \"LTTP\"");
    }
    
    [Test]
    public void GetMsuTypeNameTest()
    {
        var msuTypeService = GetMsuTypeService(null, new MsuAppSettings()
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
    
    private string GetConfigDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory != null ? Path.Combine(directory.FullName, "ConfigRepo", "resources") : "";
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
}