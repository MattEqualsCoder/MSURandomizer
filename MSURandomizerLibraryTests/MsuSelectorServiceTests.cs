using System.Collections;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

public class MsuSelectorServiceTests
{
    [Test]
    public void AssignMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 5) },
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 5) },
                new() { (1, 5) },
            },
            out var msuTypes, out var msus);

        var msu = msus.First();
        
        var response = msuSelectService.AssignMsu(new MsuSelectorRequest()
        {
            Msu = msu,
            OutputMsuType = msu.MsuType,
            OutputPath = msu.Path.Replace(".msu", "-output.msu"),
            EmptyFolder = false,
            OpenFolder = false,
            PrevMsu = null
        });
        
        Assert.That(response.Successful, Is.True);
        Assert.That(string.IsNullOrEmpty(response.Message));
        Assert.That(response.Msu, Is.Not.Null);
        Assert.That(File.Exists(response.Msu?.Path));
        Assert.That(response.Msu?.Tracks.Count, Is.EqualTo(5));

        var txtFile = Path.Combine(new FileInfo(response.Msu!.Path).DirectoryName!, "msu-randomizer-output.txt");
        Assert.That(File.Exists(txtFile));

        foreach (var track in response.Msu!.Tracks)
        {
            Assert.That(File.Exists(track.Path));
        }
    }
    
    [Test]
    public void PickRandomMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 50) },
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 50) },
                new() { (1, 50) },
            },
            out var msuTypes, out var msus);

        var response = msuSelectService.PickRandomMsu(new MsuSelectorRequest()
        {
            Msus = msus,
            OutputMsuType = msus.First().MsuType,
            OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
            EmptyFolder = false,
            OpenFolder = false,
            PrevMsu = null
        });
        
        Assert.That(response.Successful, Is.True);
        Assert.That(string.IsNullOrEmpty(response.Message));
        Assert.That(response.Msu, Is.Not.Null);
        Assert.That(File.Exists(response.Msu?.Path));
        Assert.That(response.Msu?.Tracks.Count, Is.EqualTo(50));

        var txtFile = Path.Combine(new FileInfo(response.Msu!.Path).DirectoryName!, "msu-randomizer-output.txt");
        Assert.That(File.Exists(txtFile));

        foreach (var track in response.Msu!.Tracks)
        {
            Assert.That(File.Exists(track.Path));
        }
        
        Assert.That(response.Msu?.Tracks.Select(x => x.MsuPath).Distinct().Count(), Is.EqualTo(1));
    }
    
    [Test]
    public void CreateShuffledMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 100) },
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 100) },
                new() { (1, 100) },
            },
            out var msuTypes, out var msus);

        var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
        {
            Msus = msus,
            OutputMsuType = msus.First().MsuType,
            OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
            EmptyFolder = false,
            OpenFolder = false,
            PrevMsu = null
        });
        
        Assert.That(response.Successful, Is.True);
        Assert.That(string.IsNullOrEmpty(response.Message));
        Assert.That(response.Msu, Is.Not.Null);
        Assert.That(File.Exists(response.Msu?.Path));
        Assert.That(response.Msu?.Tracks.Count, Is.EqualTo(100));

        var txtFile = Path.Combine(new FileInfo(response.Msu!.Path).DirectoryName!, "msu-randomizer-output.txt");
        Assert.That(File.Exists(txtFile));

        foreach (var track in response.Msu!.Tracks)
        {
            Assert.That(File.Exists(track.Path));
        }
        
        Assert.That(response.Msu?.Tracks.Select(x => x.MsuPath).Distinct().Count(), Is.EqualTo(2));
    }
    
    [Test]
    public void ConvertMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 10) },
                new() { (51, 60) },
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 10) },
            },
            out var msuTypes, out var msus);

        msuTypes.First().Conversions[msuTypes.Last()] = (x) => x - 50;
        msuTypes.Last().Conversions[msuTypes.First()] = (x) => x + 50;

        var response = msuSelectService.ConvertMsu(new MsuSelectorRequest()
        {
            Msu = msus.First(),
            OutputMsuType = msuTypes.Last(),
        });
        
        Assert.That(response.Successful, Is.True);
        Assert.That(string.IsNullOrEmpty(response.Message));
        Assert.That(response.Msu, Is.Not.Null);
        Assert.That(response.Msu?.Tracks.Count, Is.EqualTo(10));
        Assert.That(response.Msu?.Tracks.All(x => x.Number >= 51) == true);
        Assert.That(response.Msu?.Tracks.All(x => x.Number <= 60) == true);
    }
    
    [Test]
    public void ConvertMsusTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 10) },
                new() { (51, 60) },
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 10) },
                new() { (1, 10) },
            },
            out var msuTypes, out var msus);

        msuTypes.First().Conversions[msuTypes.Last()] = (x) => x - 50;
        msuTypes.Last().Conversions[msuTypes.First()] = (x) => x + 50;

        var response = msuSelectService.ConvertMsus(new MsuSelectorRequest()
        {
            Msus = msus,
            OutputMsuType = msuTypes.Last(),
        });
        
        Assert.That(response.Successful, Is.True);
        Assert.That(string.IsNullOrEmpty(response.Message));
        Assert.That(response.Msus?.Count, Is.EqualTo(2));

        foreach (var msu in response.Msus!)
        {
            Assert.That(msu.Tracks.Count, Is.EqualTo(10));
            Assert.That(msu.Tracks.All(x => x.Number >= 51) == true);
            Assert.That(msu.Tracks.All(x => x.Number <= 60) == true);
        }
    }
    
    [Test]
    public void SaveMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 10) },
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 10) },
            },
            out var msuTypes, out var msus);

        msuTypes.First().Conversions[msuTypes.Last()] = (x) => x - 50;
        msuTypes.Last().Conversions[msuTypes.First()] = (x) => x + 50;

        var response = msuSelectService.SaveMsu(new MsuSelectorRequest()
        {
            Msu = msus.First(),
            OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
            EmptyFolder = false,
            OpenFolder = false,
            PrevMsu = null
        });
        
        Assert.That(string.IsNullOrEmpty(response.Message));
        Assert.That(response.Successful, Is.True);
        Assert.That(response.Msu, Is.Not.Null);
        Assert.That(File.Exists(response.Msu?.Path));
        Assert.That(response.Msu?.Tracks.Count, Is.EqualTo(10));

        var txtFile = Path.Combine(new FileInfo(response.Msu!.Path).DirectoryName!, "msu-randomizer-output.txt");
        Assert.That(File.Exists(txtFile));

        foreach (var track in response.Msu!.Tracks)
        {
            Assert.That(File.Exists(track.Path));
        }
        
        Assert.That(response.Msu?.Tracks.Select(x => x.MsuPath).Distinct().Count(), Is.EqualTo(1));
    }

    private MsuSelectorService CreateMsuSelectorService(List<List<(int, int)>> msuTypeTracks, List<List<(int, int)>> msuTracks, out ICollection<MsuType> msuTypes, out ICollection<Msu> msus)
    {
        var logger = TestHelpers.CreateMockLogger<MsuSelectorService>();
        var lookupLogger = TestHelpers.CreateMockLogger<MsuLookupService>();
        var msuDetailsService = TestHelpers.CreateMockMsuDetailsService(null, null);
        var msuTypeService = TestHelpers.CreateMockMsuTypeServiceMulti(msuTypeTracks, out var generatedMsuTypes);
        var msuUserOptionsService = TestHelpers.CreateMockMsuUserOptionsService(null);

        msuTypes = generatedMsuTypes;
        
        var index = 1;
        var folder = "";
        foreach (var tracks in msuTracks)
        {
            var path = TestHelpers.CreateMsu(tracks, $"test-msu-{index}", index == 1);
            if (string.IsNullOrEmpty(folder))
            {
                folder = new FileInfo(path).DirectoryName;
            }
            index++;
        }
        var lookupService = new MsuLookupService(lookupLogger, msuTypeService, msuUserOptionsService.MsuUserOptions, msuDetailsService, new MsuAppSettings());
        lookupService.LookupMsus(folder);

        msus = lookupService.Msus.ToList();
        
        return new MsuSelectorService(logger, msuDetailsService, msuTypeService, lookupService, msuUserOptionsService);
    }
}