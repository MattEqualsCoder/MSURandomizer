using Moq;
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
        Assert.That(msu?.Name, Is.EqualTo("msu-test"));
        Assert.That(msu?.Tracks.Count, Is.EqualTo(5));
        Assert.That(File.Exists(msu?.Tracks.First().Path), Is.True);
    }
    
    [Test]
    public void LookupMsusTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        TestHelpers.CreateMsu(tracks, "test-msu-1", true);
        var msuPath = TestHelpers.CreateMsu(tracks, "test-msu-2", false, true);
        var folder = new FileInfo(msuPath).DirectoryName;
        
        var service = CreateMsuLookupService(tracks);
        var msus = service.LookupMsus(folder);
        
        Assert.That(service.Msus.Count, Is.EqualTo(2));
        Assert.That(service.Msus.First().Name, Is.EqualTo("msu-test"));
        Assert.That(service.Msus.Last().Name, Is.EqualTo("msu-test"));
        Assert.That(service.Msus.First().Tracks.Count, Is.EqualTo(5));
        Assert.That(service.Msus.Last().Tracks.Count, Is.EqualTo(10));
        Assert.That(File.Exists(service.Msus.First().Tracks.First().Path), Is.True);
        Assert.That(File.Exists(service.Msus.Last().Tracks.First().Path), Is.True);
    }

    [Test]
    public void RefreshMsuTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        var msuPath = TestHelpers.CreateMsu(tracks, "test-msu-1", true);
        var folder = new FileInfo(msuPath).DirectoryName;
        
        var service = CreateMsuLookupService(tracks);
        service.LookupMsus(folder);
        
        Assert.That(service.Msus.Count, Is.EqualTo(1));
        Assert.That(service.Msus.First().Tracks.Count, Is.EqualTo(5));
        
        tracks = new List<(int, int)>() { (1, 6) };
        msuPath = TestHelpers.CreateMsu(tracks, "test-msu-1", true);
        
        service.LookupMsus(folder);
        Assert.That(service.Msus.Count, Is.EqualTo(1));
        Assert.That(service.Msus.First().Tracks.Count, Is.EqualTo(6));
    }
    
    [Test]
    public void GetMsusByPathTest()
    {
        var tracks = new List<(int, int)>() { (1, 5) };
        var path1 = TestHelpers.CreateMsu(tracks, "test-msu-1", true);
        var path2 = TestHelpers.CreateMsu(tracks, "test-msu-2", false);
        var path3 = TestHelpers.CreateMsu(tracks, "test-msu-3", false);
        
        var folder = new FileInfo(path1).DirectoryName;
        
        var service = CreateMsuLookupService(tracks);
        service.LookupMsus(folder);
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
        var path1 = TestHelpers.CreateMsu(tracks, "test-msu-1", true);
        var path2 = TestHelpers.CreateMsu(tracks, "test-msu-2", false);
        
        var folder = new FileInfo(path1).DirectoryName;
        
        var service = CreateMsuLookupService(tracks);
        service.LookupMsus(folder);
        Assert.That(service.Msus.Count, Is.EqualTo(2));
        
        var msu = service.GetMsuByPath(path1);
        Assert.That(msu, Is.Not.Null);
        
        msu = service.GetMsuByPath(path2+"test");
        Assert.That(msu, Is.Null);
    }

    private MsuLookupService CreateMsuLookupService(List<(int, int)> tracks)
    {
        var logger = TestHelpers.CreateMockLogger<MsuLookupService>();
        var msuTypeService = TestHelpers.CreateMockMsuTypeService(tracks);
        var msuDetailsService = new Mock<IMsuDetailsService>();
        
        string? outString1;
        string? outString2;
        msuDetailsService.Setup(x => x.GetBasicMsuDetails(It.IsAny<string>(), out outString1, out outString2))
            .Returns(value: null);

        MsuDetails? outDetails;
        msuDetailsService.Setup(x => x.LoadMsuDetails(It.IsAny<MsuType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out outDetails, out outString1))
            .Returns(value: null);
        
        return new MsuLookupService(logger, msuTypeService, new MsuUserOptions(), msuDetailsService.Object, new MsuAppSettings());
    }
}