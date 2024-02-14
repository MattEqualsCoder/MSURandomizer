using System.Collections;
using MSURandomizerLibrary;
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
        
        Assert.That(response.Msu?.Tracks.Select(x => x.OriginalMsu).Distinct().Count(), Is.EqualTo(2));
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
    public void PairedTracksMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 2) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
            },
            out var msuTypes, out var msus, new Dictionary<int, List<int>>() { { 1, new List<int>() { 2 } }});

        for (var i = 0; i < 10; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ShuffleWithPairedTracks
            });

            Assert.That(response.Msu!.Tracks.First().MsuName == response.Msu!.Tracks.Last().MsuName);
        }
    }
    
    [Test]
    public void PairedTracksCurrentTrackSetMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 2) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
            },
            out var msuTypes, out var msus, new Dictionary<int, List<int>>() { { 1, new List<int>() { 2 } }, { 2, new List<int>() { 1 } } });

        var playedTrack = msus.Last().Tracks.Last();
        playedTrack.MsuName = playedTrack.Msu?.DisplayName;
        playedTrack.OriginalMsu = playedTrack.Msu;
        
        for (var i = 0; i < 10; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ShuffleWithPairedTracks,
                CurrentTrack = playedTrack
            });

            Assert.That(response.Msu!.Tracks.First().MsuName == response.Msu!.Tracks.Last().MsuName);
            Assert.That(response.Msu!.Tracks.First().MsuName == playedTrack.MsuName);
        }
    }
    
    [Test]
    public void NonPairedTracksMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 2) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
                new() { (1, 2) },
            },
            out var msuTypes, out var msus, new Dictionary<int, List<int>>() { { 1, new List<int>() { 2 } }});

        var anyNotMatched = false;
        
        for (var i = 0; i < 10; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
            });

            if (response.Msu!.Tracks.First().MsuName != response.Msu!.Tracks.Last().MsuName)
            {
                anyNotMatched = true;
                break;
            }
        }
        
        Assert.That(anyNotMatched, Is.True);
    }
    
    [Test]
    public void SpecialTracksMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 3) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
            },
            out var msuTypes, out var msus, new Dictionary<int, List<int>>() { { 1, new List<int>() { 2 } }}, 1);

        var anyNotMatchedTrack2 = false;
        var anyNotMatchedTrack3 = false;
        
        for (var i = 0; i < 10; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ChaosNonSpecialTracks
            });

            var tracks = response.Msu?.Tracks.OrderBy(x => x.Number).ToList() ?? new List<Track>();
            
            Assert.That(tracks[0].OriginalTrackNumber, Is.EqualTo(tracks[0].Number));

            if (tracks[1].OriginalTrackNumber != tracks[1].Number)
            {
                anyNotMatchedTrack2 = true;
            }
            if (tracks[2].OriginalTrackNumber != tracks[2].Number)
            {
                anyNotMatchedTrack3 = true;
            }
        }
        
        Assert.That(anyNotMatchedTrack2, Is.True);
        Assert.That(anyNotMatchedTrack3, Is.True);
    }
    
    [Test]
    public void ChaosTracksMsuTest()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 3) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
                new() { (1, 3) },
            },
            out var msuTypes, out var msus, new Dictionary<int, List<int>>() { { 1, new List<int>() { 2 } }}, 1);

        var anyNotMatchedTrack1 = false;
        var anyNotMatchedTrack2 = false;
        var anyNotMatchedTrack3 = false;
        
        for (var i = 0; i < 1000; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ChaosAllTracks
            });

            var tracks = response.Msu?.Tracks.OrderBy(x => x.Number).ToList() ?? new List<Track>();
            
            if (tracks[0].OriginalTrackNumber != 1)
            {
                anyNotMatchedTrack1 = true;
            }
            if (tracks[1].OriginalTrackNumber == 1)
            {
                anyNotMatchedTrack2 = true;
            }
            if (tracks[2].OriginalTrackNumber == 1)
            {
                anyNotMatchedTrack3 = true;
            }

            if (anyNotMatchedTrack1 && anyNotMatchedTrack2 && anyNotMatchedTrack3)
            {
                break;
            }
        }
        
        Assert.That(anyNotMatchedTrack1, Is.True);
        Assert.That(anyNotMatchedTrack2, Is.True);
        Assert.That(anyNotMatchedTrack3, Is.True);
    }
    
    [Test]
    public void FrequencyTestDefault()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 100) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 100) },
                new() { (1, 100) },
            },
            out var msuTypes, out var msus);

        msus.First().Settings.ShuffleFrequency = ShuffleFrequency.Default;
        msus.Last().Settings.ShuffleFrequency = ShuffleFrequency.Default;

        var firstMsuCount = 0;
        for (var i = 0; i < 100; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ShuffleWithPairedTracks
            });

            firstMsuCount += response.Msu!.Tracks.Count(x => x.OriginalMsu == msus.First());
        }
        
        var percentage = firstMsuCount / 10000.0;
        
        Assert.That(percentage is > 0.42 and < 0.58, Is.True);
    }
    
    [Test]
    public void FrequencyTestMoreFrequent()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 100) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 100) },
                new() { (1, 100) },
            },
            out var msuTypes, out var msus);

        msus.First().Settings.ShuffleFrequency = ShuffleFrequency.MoreFrequent;
        msus.Last().Settings.ShuffleFrequency = ShuffleFrequency.Default;

        var firstMsuCount = 0;
        for (var i = 0; i < 100; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ShuffleWithPairedTracks
            });

            firstMsuCount += response.Msu!.Tracks.Count(x => x.OriginalMsu == msus.First());
        }
        
        var percentage = firstMsuCount / 10000.0;
        
        Assert.That(percentage is > 0.58, Is.True);
    }
    
    [Test]
    public void FrequencyTestLessFrequent()
    {
        var msuSelectService = CreateMsuSelectorService(new List<List<(int, int)>>()
            {
                new() { (1, 100) }
            },
            new List<List<(int, int)>>()
            {
                new() { (1, 100) },
                new() { (1, 100) },
            },
            out var msuTypes, out var msus);

        msus.First().Settings.ShuffleFrequency = ShuffleFrequency.LessFrequent;
        msus.Last().Settings.ShuffleFrequency = ShuffleFrequency.Default;

        var firstMsuCount = 0;
        for (var i = 0; i < 100; i++)
        {
            var response = msuSelectService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                Msus = msus,
                OutputMsuType = msuTypes.First(),
                OutputPath = msus.First().Path.Replace(".msu", "-output.msu"),
                EmptyFolder = false,
                OpenFolder = false,
                PrevMsu = null,
                ShuffleStyle = MsuShuffleStyle.ShuffleWithPairedTracks
            });

            firstMsuCount += response.Msu!.Tracks.Count(x => x.OriginalMsu == msus.First());
        }
        
        var percentage = firstMsuCount / 10000.0;
        
        Assert.That(percentage is < 0.42, Is.True);
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


    private MsuSelectorService CreateMsuSelectorService(List<List<(int, int)>> msuTypeTracks, List<List<(int, int)>> msuTracks, out ICollection<MsuType> msuTypes, out ICollection<Msu> msus, Dictionary<int, List<int>>? pairs = null, int? specialTrack = null)
    {
        var logger = TestHelpers.CreateMockLogger<MsuSelectorService>();
        var lookupLogger = TestHelpers.CreateMockLogger<MsuLookupService>();
        var msuDetailsService = TestHelpers.CreateMockMsuDetailsService(null, null);
        var msuTypeService = TestHelpers.CreateMockMsuTypeServiceMulti(msuTypeTracks, out var generatedMsuTypes, pairs, specialTrack);
        var msuUserOptionsService = TestHelpers.CreateMockMsuUserOptionsService(null);
        var msuCacheService = TestHelpers.CreateMockMsuCacheService();

        msuTypes = generatedMsuTypes;
        
        var index = 1;
        var folder = "";
        foreach (var tracks in msuTracks)
        {
            var path = TestHelpers.CreateMsu(tracks, $"test-msu-{index}", index == 1);
            if (string.IsNullOrEmpty(folder))
            {
                folder = new FileInfo(path).Directory?.Parent?.FullName;;
            }
            index++;
        }
        var lookupService = new MsuLookupService(lookupLogger, msuTypeService, msuUserOptionsService.MsuUserOptions, msuDetailsService, new MsuAppSettings(), msuCacheService);
        lookupService.LookupMsus(folder);

        msus = lookupService.Msus.ToList();
        
        return new MsuSelectorService(logger, msuDetailsService, msuTypeService, lookupService, msuUserOptionsService);
    }
}