using Microsoft.Extensions.Logging;
using Moq;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizerLibraryTests;

public class TestHelpers
{
    public static IMsuAppSettingsService CreateMsuAppSettingsService(MsuAppSettings? settings = null)
    {
        if (settings == null)
        {
            settings = new MsuAppSettings();
        }

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
    
    public static IMsuTypeService CreateMockMsuTypeService(List<(int, int)> tracks)
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

        return CreateMockMsuTypeService(new List<MsuType>() { msuType });
    }

    public static string CreateMsu(List<int> tracks, string msuName = "test-msu", bool deleteOld = true, bool createAlts = false)
    {
        var folder = new DirectoryInfo("msu-test");
        if (!folder.Exists)
        {
            folder.Create();
        }

        var path = folder.FullName;

        if (deleteOld)
        {
            foreach(var filePath in Directory.EnumerateFiles(path, "*.msu").Concat(Directory.EnumerateFiles(path, "*.pcm")))
            {
                File.Delete(filePath);
            }    
        }

        var msuPath = Path.Combine(folder.FullName, $"{msuName}.msu");
        if (!File.Exists(msuPath))
        {
            using (File.Create(msuPath)) {}
        }
        
        foreach (var trackNumber in tracks)
        {
            var pcmPath = Path.Combine(folder.FullName, $"{msuName}-{trackNumber}.pcm");
            using (File.Create(pcmPath)) {}

            if (createAlts)
            {
                pcmPath = Path.Combine(folder.FullName, $"{msuName}-{trackNumber}_alt.pcm");
                using (File.Create(pcmPath)) {}
            }
        }

        return msuPath;
    }
    
    public static string CreateMsu(List<(int, int)> tracks, string msuName = "test-msu", bool deleteOld = true, bool createAlts = false)
    {
        return CreateMsu(GetTracksFromRanges(tracks), msuName, deleteOld, createAlts);
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
}