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
}