using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

public class MsuLookupServiceTests
{
    [Test]
    public void LoadMsuTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        var service = CreateMsuLookupService(tracks);
        var msuPath = TestHelpers.CreateMsu(tracks);
        var msu = service.LoadMsu(msuPath);
        
        Assert.That(msu, Is.Not.Null);
        Assert.That(msu?.Name, Is.EqualTo("test-msu"));
        Assert.That(msu?.Tracks.Count, Is.EqualTo(5));
        Assert.That(File.Exists(msu?.Tracks.First().Path), Is.True);
    }
    
    [Test]
    public void LookupMsusTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        TestHelpers.CreateMsu(tracks, "test-msu-1");
        var msuPath = TestHelpers.CreateMsu(tracks, "test-msu-2", false, true);
        var folder = new FileInfo(msuPath).Directory?.Parent?.FullName;
        
        var service = CreateMsuLookupService(tracks);
        var msus = service.LookupMsus(folder, new Dictionary<string, string>()
        {
            { TestHelpers.MsuTestFolder, TestHelpers.TestMsuTypeName }
        });
        
        Assert.That(service.Msus.Count, Is.EqualTo(2));
        Assert.That(service.Msus.Any(x => x.Name == "test-msu-1"));
        Assert.That(service.Msus.Any(x => x.Name == "test-msu-2"));
        Assert.That(service.Msus.Any(x => x.Tracks.Count == 5));
        Assert.That(service.Msus.Any(x => x.Tracks.Count == 10));
        Assert.That(File.Exists(service.Msus.First().Tracks.First().Path), Is.True);
        Assert.That(File.Exists(service.Msus.Last().Tracks.First().Path), Is.True);
    }

    [Test]
    public void RefreshMsuTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        var msuPath = TestHelpers.CreateMsu(tracks, "test-msu-1");
        var folder = new FileInfo(msuPath).Directory?.Parent?.FullName;
        
        var service = CreateMsuLookupService(tracks);
        service.LookupMsus(folder, new Dictionary<string, string>()
        {
            { TestHelpers.MsuTestFolder, TestHelpers.TestMsuTypeName }
        });
        
        Assert.That(service.Msus.Count, Is.EqualTo(1));
        Assert.That(service.Msus.First().Tracks.Count, Is.EqualTo(5));
        
        tracks = new List<(int, int)>() { (1, 6) };
        TestHelpers.CreateMsu(tracks, "test-msu-1");
        
        service.LookupMsus(folder, new Dictionary<string, string>()
        {
            { TestHelpers.MsuTestFolder, TestHelpers.TestMsuTypeName }
        });
        Assert.That(service.Msus.Count, Is.EqualTo(1));
        Assert.That(service.Msus.First().Tracks.Count, Is.EqualTo(6));
    }
    
    [Test]
    public void GetMsusByPathTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        var path1 = TestHelpers.CreateMsu(tracks, "test-msu-1");
        var path2 = TestHelpers.CreateMsu(tracks, "test-msu-2", false);
        var path3 = TestHelpers.CreateMsu(tracks, "test-msu-3", false);
        
        var folder = new FileInfo(path1).Directory?.Parent?.FullName;
        
        var service = CreateMsuLookupService(tracks);
        service.LookupMsus(folder, new Dictionary<string, string>()
        {
            { TestHelpers.MsuTestFolder, TestHelpers.TestMsuTypeName }
        });
        Assert.That(service.Msus.Count, Is.EqualTo(3));
        
        var msus = service.GetMsusByPath(new List<string>() { path1, path2 });
        Assert.That(msus.Count, Is.EqualTo(2));
        
        msus = service.GetMsusByPath(new List<string>() { path1+"test" });
        Assert.That(msus.Count, Is.EqualTo(0));
    }
    
    [Test]
    public void GetMsuByPathTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        var path1 = TestHelpers.CreateMsu(tracks, "test-msu-1");
        var path2 = TestHelpers.CreateMsu(tracks, "test-msu-2", false);
        
        var folder = new FileInfo(path1).Directory?.Parent?.FullName;
        
        var service = CreateMsuLookupService(tracks);
        service.LookupMsus(folder, new Dictionary<string, string>()
        {
            { TestHelpers.MsuTestFolder, TestHelpers.TestMsuTypeName }
        });
        Assert.That(service.Msus.Count, Is.EqualTo(2));
        
        var msu = service.GetMsuByPath(path1);
        Assert.That(msu, Is.Not.Null);
        
        msu = service.GetMsuByPath(path2+"test");
        Assert.That(msu, Is.Null);
    }

    private MsuLookupService CreateMsuLookupService(List<(int, int)> tracks)
    {
        var logger = TestHelpers.CreateMockLogger<MsuLookupService>();
        var msuTypeService = TestHelpers.CreateMockMsuTypeService(tracks, out var msuTypes);
        var msuDetailsService = TestHelpers.CreateMockMsuDetailsService(null, null);
        var msuCacheService = TestHelpers.CreateMockMsuCacheService();
        var msuUserOptionsService = TestHelpers.CreateMockMsuUserOptionsService(null);
        return new MsuLookupService(logger, msuTypeService, msuDetailsService, new MsuAppSettings(), msuCacheService, msuUserOptionsService);
    }
}