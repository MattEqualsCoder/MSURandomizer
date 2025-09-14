using Moq;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibraryTests;

[NonParallelizable]
public class MsuUserOptionsServiceTests
{
    private const string TestMsuTypeName = "TestMSUType";
    private const string TestMsuTypeValue = "TestMSUValue";
    private const string TestOptionName = "TestName";
    private const string TestMsuName = "TestMSU";
    private const string TestMsuSettingsName = "TestMSUSettings";
    
    [Test]
    public void InitializeTest()
    {
        var defaultOptions = new MsuUserOptions();
        var msuUserOptionsService = GetMsuUserOptionsService(null);
        var file = new FileInfo("unit-test.yml");
        if (file.Exists)
        {
            file.Delete();
        }
        var options = msuUserOptionsService.Initialize(file.FullName);
        Assert.That(options.Name, Is.EqualTo(defaultOptions.Name), "Spawned default options are invalid");
    }

    [Test]
    public void SaveTest()
    {
        var testMsuType = new MsuType()
        {
            Name = TestMsuTypeName,
            DisplayName = TestMsuTypeName,
            RequiredTrackNumbers = new HashSet<int>(),
            ValidTrackNumbers = new HashSet<int>(),
            Tracks = new List<MsuTypeTrack>()
        };
        
        var msuUserOptionsService = GetMsuUserOptionsService(new List<MsuType>()
        {
            testMsuType
        });
        
        var file = new FileInfo("unit-test.yml");
        if (file.Exists)
        {
            file.Delete();
        }
        
        var options = msuUserOptionsService.Initialize(file.FullName);
        Assert.That(options.Name, Is.Not.EqualTo(TestOptionName));
        
        options.Name = TestOptionName;
        options.MsuDirectories = new Dictionary<string, string>()
        {
            { TestMsuTypeValue, testMsuType.Name }
        };
        msuUserOptionsService.Save();
        
        file = new FileInfo("unit-test.yml");
        Assert.That(file.Exists, Is.True, $"{file.FullName} was not created by MsuUserOptionsService");
        
        msuUserOptionsService.Initialize(file.FullName);
        Assert.That(msuUserOptionsService.MsuUserOptions.Name, Is.EqualTo(TestOptionName), "Reloaded MSU User Options name is invalid");
        Assert.That(msuUserOptionsService.MsuUserOptions.MsuDirectories.Keys.Contains(TestMsuTypeValue), Is.True);
        Assert.That(msuUserOptionsService.MsuUserOptions.MsuDirectories[TestMsuTypeValue], Is.EqualTo(testMsuType.Name ));
        file.Delete();
    }
    
    [Test]
    public void SaveMsuTest()
    {
        var msu = new Msu(
            type: null, 
            name: TestMsuName,
            folderName: TestMsuName,
            fileName: TestMsuName,
            path: TestMsuName,
            tracks: new List<Track>(),
            msuDetails: null,
            prevMsu: null
        )
        {
            Settings = new MsuSettings(TestMsuName)
            {
                Name = TestMsuSettingsName
            }
        };
        
        var msuUserOptionsService = GetMsuUserOptionsService(null);
        var file = new FileInfo("unit-test.yml");
        if (file.Exists)
        {
            file.Delete();
        }
        
        msuUserOptionsService.Initialize(file.FullName);
        msuUserOptionsService.SaveMsuSettings(msu);
        
        file = new FileInfo("unit-test.yml");
        Assert.That(file.Exists, Is.True, $"{file.FullName} was not created by MsuUserOptionsService");
        
        msuUserOptionsService.Initialize(file.FullName);
        Assert.That(msuUserOptionsService.MsuUserOptions.MsuSettings.FirstOrDefault(x => x.MsuPath == TestMsuName)?.Name, Is.EqualTo(TestMsuSettingsName), "Reloaded MSU settings is invalid");
        file.Delete();

        msu.Settings = new MsuSettings(TestMsuName);
        msuUserOptionsService.SaveMsuSettings(msu);
        
        file = new FileInfo("unit-test.yml");
        Assert.That(file.Exists, Is.True, $"{file.FullName} was not created by MsuUserOptionsService");
        
        msuUserOptionsService.Initialize(file.FullName);
        Assert.That(msuUserOptionsService.MsuUserOptions.MsuSettings.FirstOrDefault(x => x.MsuPath == TestMsuName)?.Name, Is.Null, "Empty MSU settings is invalid");
        file.Delete();
    }

    private IMsuUserOptionsService GetMsuUserOptionsService(List<MsuType>? msuTypes)
    {
        var msuTypeService = CreateMockMsuTypeService(msuTypes);
        return new MsuUserOptionsService(msuTypeService, TestHelpers.CreateMockLogger<MsuUserOptionsService>());
    }
    
    private IMsuTypeService CreateMockMsuTypeService(List<MsuType>? msuTypes)
    {
        var mockService = new Mock<IMsuTypeService>();
        mockService
            .Setup(x => x.MsuTypes)
            .Returns(msuTypes ?? new List<MsuType>());
        return mockService.Object;
    }
}