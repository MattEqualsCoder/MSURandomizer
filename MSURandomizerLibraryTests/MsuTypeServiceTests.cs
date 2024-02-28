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
        Assert.That(msuTypeService.MsuTypes, Is.Not.Empty, "No MSU types loaded");
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
        
        Assert.That(msuTypeService.GetMsuType("The Legend of Zelda: A Link to the Past"), Is.Not.Null, "Could not find MSU type by \"The Legend of Zelda: A Link to the Past\"");
        Assert.That(msuTypeService.GetMsuType("LTTP"), Is.Not.Null, "Could not find MSU type by \"LTTP\"");
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
}