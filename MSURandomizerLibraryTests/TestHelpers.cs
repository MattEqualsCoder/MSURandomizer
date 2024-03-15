using System.Collections;
using Microsoft.Extensions.Logging;
using Moq;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibraryTests;

public abstract class TestHelpers
{
    public static IMsuAppSettingsService CreateMsuAppSettingsService(MsuAppSettings? settings = null)
    {
        settings ??= new MsuAppSettings();

        var appSettingsService = new MsuMsuAppSettingsService();
        
        var serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(settings);
        
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(yaml);
        writer.Flush();
        stream.Position = 0;

        appSettingsService.Initialize(stream);
        return appSettingsService;
    }

    public static ILogger<T> CreateMockLogger<T>()
    {
        return Mock.Of<ILogger<T>>();
    }

    public static IMsuTypeService CreateMockMsuTypeService(List<MsuType>? msuTypes)
    {
        var mockService = new Mock<IMsuTypeService>();
        
        mockService
            .Setup(x => x.MsuTypes)
            .Returns(msuTypes ?? new List<MsuType>());
        
        mockService
            .Setup(x => x.GetMsuType(It.IsAny<string>()))
            .Returns((string key) => msuTypes?.FirstOrDefault(x => x.Name == key));
        
        return mockService.Object;
    }
    
    public static IMsuTypeService CreateMockMsuTypeService(List<(int, int)> tracks, out List<MsuType> msuTypes)
    {
        var trackNumbers = GetTracksFromRanges(tracks);

        var msuType = new MsuType()
        {
            Name = "Test MSU Type",
            DisplayName = "Test MSU Type",
            RequiredTrackNumbers = trackNumbers.ToHashSet(),
            ValidTrackNumbers = trackNumbers.ToHashSet(),
            Tracks = trackNumbers.Select(x => new MsuTypeTrack()
            {
                Number = x,
                Name = $"Track {x}"
            })
        };

        msuTypes = new List<MsuType>() { msuType };
        return CreateMockMsuTypeService(msuTypes);
    }
    
    public static IMsuTypeService CreateMockMsuTypeServiceMulti(List<List<(int, int)>> msuTypeTracks, out List<MsuType> msuTypes, Dictionary<int, List<int>>? pairs = null, List<int>? specialTracks = null)
    {
        msuTypes = new List<MsuType>();

        var index = 1;

        foreach (var tracks in msuTypeTracks)
        {
            var currentIndex = index;
            var trackNumbers = GetTracksFromRanges(tracks);
            
            msuTypes.Add(new MsuType()
            {
                Name = $"Test MSU Type {currentIndex}",
                DisplayName = $"Test MSU Type {currentIndex}",
                RequiredTrackNumbers = trackNumbers.ToHashSet(),
                ValidTrackNumbers = trackNumbers.ToHashSet(),
                Tracks = trackNumbers.Select(x => new MsuTypeTrack()
                {
                    Number = x,
                    Name = $"MSU Type {currentIndex} Track {x}",
                    PairedTracks = pairs != null && pairs.ContainsKey(x) ? pairs[x] : null,
                    IsSpecial = specialTracks?.Contains(x) == true
                })
            });

            index++;
        }
        
        return CreateMockMsuTypeService(msuTypes);
    }

    public static IMsuUserOptionsService CreateMockMsuUserOptionsService(MsuUserOptions? options)
    {
        options ??= new MsuUserOptions();
        var service = new Mock<IMsuUserOptionsService>();

        service.Setup(x => x.Initialize(It.IsAny<string>()))
            .Returns(options);
        
        service.Setup(x => x.MsuUserOptions)
            .Returns(options);

        service.Setup(x => x.Save());
        
        service.Setup(x => x.SaveMsuSettings(It.IsAny<Msu>()));

        return service.Object;
    }
    
    public static IMsuCacheService CreateMockMsuCacheService()
    {
        var service = new Mock<IMsuCacheService>();
        
        service.Setup(x => x.Initialize(It.IsAny<string>()));
        
        service.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICollection<string>>()))
            .Returns(value: null);

        service.Setup(x => x.Save());
        
        service.Setup(x => x.Put(It.IsAny<Msu>(), It.IsAny<string>(), It.IsAny<ICollection<string>>(), It.IsAny<bool>()));

        return service.Object;
    }

    public static string CreateMsu(List<int> tracks, string msuName = "test-msu", bool deleteOld = true, bool createAlts = false)
    {
        if (deleteOld && Directory.Exists(MsuTestFolder))
        {
            DeleteFolder(MsuTestFolder);
        }
        
        var folder = Path.Combine(MsuTestFolder, msuName);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var msuPath = Path.Combine(folder, $"{msuName}.msu");
        if (!File.Exists(msuPath))
        {
            using (File.Create(msuPath)) {}
        }
        
        foreach (var trackNumber in tracks)
        {
            var pcmPath = Path.Combine(folder, $"{msuName}-{trackNumber}.pcm");
            using (File.Create(pcmPath)) {}

            if (createAlts)
            {
                pcmPath = Path.Combine(folder, $"{msuName}-{trackNumber}_alt.pcm");
                using (File.Create(pcmPath)) {}
            }
        }

        return msuPath;
    }
    
    public static string CreateMsu(List<(int, int)> tracks, string msuName = "test-msu", bool deleteOld = true, bool createAlts = false)
    {
        return CreateMsu(GetTracksFromRanges(tracks), msuName, deleteOld, createAlts);
    }

    public static IMsuDetailsService CreateMockMsuDetailsService(MsuDetails? returnMsuDetails, Msu? returnMsu)
    {
        var msuDetailsService = new Mock<IMsuDetailsService>();
        
        string? outString1;
        string? outString2;
        msuDetailsService.Setup(x => x.GetMsuDetails(It.IsAny<string>(), out outString1, out outString2))
            .Returns(value: returnMsuDetails);

        msuDetailsService.Setup(x => x.ConvertToMsu(It.IsAny<MsuDetails>(),It.IsAny<MsuType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out outString1))
            .Returns(value: returnMsu);
        
        msuDetailsService.Setup(x => x.SaveMsuDetails(It.IsAny<Msu>(), It.IsAny<string>(), out outString1))
            .Returns(value: true);

        return msuDetailsService.Object;
    }

    private static List<int> GetTracksFromRanges(List<(int, int)> tracks)
    {
        var trackNumbers = new List<int>();
        
        foreach (var (min, max) in tracks)
        {
            for (var i = min; i <= max; i++)
            {
                trackNumbers.Add(i);
            }
        }

        return trackNumbers;
    }

    private static string _msuTestFolder = new DirectoryInfo(Path.Combine("msu-test", "msus")).FullName;
    public static string MsuTestFolder => _msuTestFolder;

    public static void DeleteFolder(string path)
    {
        if (!Directory.Exists(path))
            return;
        
        foreach(var filePath in Directory.EnumerateFiles(path))
        {
            File.Delete(filePath);
        }

        foreach (var subDirectory in Directory.EnumerateDirectories(path))
        {
            DeleteFolder(subDirectory);
        }

        Directory.Delete(path);
    }
}