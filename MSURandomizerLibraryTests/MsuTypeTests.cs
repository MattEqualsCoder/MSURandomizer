using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

public class MsuTypeTests
{
    private MsuTypeService _msuTypeService = null!;
    private MsuLookupService _msuLookupService = null!;
    private MsuAppSettings _appSettings = null!;
    
    [SetUp]
    public void Setup()
    {
        _appSettings = GetAppSettings();
        _msuTypeService = GetMsuTypeService(null, _appSettings);
        _msuLookupService = CreateMsuLookupService(_appSettings);
    }

    [Test]
    public void TestSmz3()
    {
        var path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 61),
            (99, 99),
            (101, 140)
        });
        var msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Combo Randomizer"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 61),
            (99, 99),
            (101, 130)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Combo Randomizer"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 34),
            (99, 99),
            (101, 130)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Combo Randomizer"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 34),
            (99, 99),
            (101, 140)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Combo Randomizer"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 46),
            (99, 99),
            (101, 130)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Combo Randomizer"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 46),
            (99, 99),
            (101, 140)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Combo Randomizer"));
    }
    
    [Test]
    public void TestSmz3Legacy()
    {
        // Note: SMZ3 Legacy with extended Metroid tracks but no extended Zelda tracks does not get detected properly,
        // but it's a very unlikely combo so it's probably fine
        
        var path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 40),
            (99, 99),
            (101, 161)
        });
        var msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Classic (Metroid First)"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 30),
            (99, 99),
            (101, 161)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Classic (Metroid First)"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 30),
            (99, 99),
            (101, 134)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Classic (Metroid First)"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 40),
            (99, 99),
            (101, 146)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Classic (Metroid First)"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 30),
            (99, 99),
            (101, 146)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("SMZ3 Classic (Metroid First)"));
    }
    
    [Test]
    public void TestZelda()
    {
        var path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 34)
        });
        var msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("A Link to the Past"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 46)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("A Link to the Past"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 61)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("A Link to the Past"));
    }
    
    [Test]
    public void TestSuperMetroid()
    {
        var path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 30)
        });
        var msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("Super Metroid"));
        
        path = TestHelpers.CreateMsu(new List<(int, int)>()
        {
            (1, 40)
        });
        msu = _msuLookupService.LoadMsu(path);
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu!.MsuTypeName, Is.EqualTo("Super Metroid"));
    }

    [Test]
    public void TestOther()
    {
        var msuTypes = _msuTypeService.MsuTypes.Where(x => !_appSettings.ZeldaSuperMetroidSmz3MsuTypes.Contains(x.DisplayName))
            .ToList();

        foreach (var msuType in msuTypes)
        {
            var path = TestHelpers.CreateMsu(msuType.ValidTrackNumbers.ToList());
            var msu = _msuLookupService.LoadMsu(path, msuType);
            Assert.That(msu!.Tracks.Count, Is.EqualTo(msuType.ValidTrackNumbers.Count), $"MSU of type {msuType.DisplayName} failed to load");
        }
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
        var msuDetailsService = TestHelpers.CreateMockMsuDetailsService(null, null);
        var msuCacheService = TestHelpers.CreateMockMsuCacheService();
        return new MsuLookupService(logger, _msuTypeService, new MsuUserOptions(), msuDetailsService, appSettings ?? new MsuAppSettings(), msuCacheService);
    }
    
}