using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public class MsuRandomizerInitializationService : IMsuRandomizerInitializationService
{
    private readonly IMsuAppSettingsService _msuAppSettingsService;
    private readonly IServiceProvider _serviceProvider;
    
    public MsuRandomizerInitializationService(IMsuAppSettingsService msuAppSettingsService, IServiceProvider serviceProvider)
    {
        _msuAppSettingsService = msuAppSettingsService;
        _serviceProvider = serviceProvider;
    }
    
    public void Initialize(string randomizerSettingsPath, string msuTypeFilePathOverride = "", string userSettingsFilePathOverride = "")
    {
        var randomizerSettings = _msuAppSettingsService.Initialize(randomizerSettingsPath);
        InitializeInternal(randomizerSettings, msuTypeFilePathOverride, userSettingsFilePathOverride);
    }

    public void Initialize(Stream randomizerSettingsStream, string msuTypeFilePathOverride = "", string userSettingsFilePathOverride = "")
    {
        var randomizerSettings = _msuAppSettingsService.Initialize(randomizerSettingsStream);
        InitializeInternal(randomizerSettings, msuTypeFilePathOverride, userSettingsFilePathOverride);
    }

    private void InitializeInternal(MsuAppSettings msuAppSettings, string msuTypeFilePathOverride, string userSettingsFilePathOverride)
    {
        var msuTypePath = string.IsNullOrWhiteSpace(msuTypeFilePathOverride)
            ? Environment.ExpandEnvironmentVariables(msuAppSettings.MsuTypeFilePath)
            : Environment.ExpandEnvironmentVariables(msuTypeFilePathOverride);
        var msuTypeService = _serviceProvider.GetRequiredService<IMsuTypeService>();
        if ((msuTypePath.ToLower().EndsWith(".yaml") || msuTypePath.ToLower().EndsWith(".yml")) && File.Exists(msuTypePath))
        {
            using var yamlFile = new FileStream(msuTypePath, FileMode.Open);
            msuTypeService.LoadMsuTypes(yamlFile);
        }
        else
        {
            msuTypeService.LoadMsuTypes(msuTypePath);
        }

        var userOptionsPath = string.IsNullOrWhiteSpace(userSettingsFilePathOverride)
            ? Environment.ExpandEnvironmentVariables(msuAppSettings.UserSettingsFilePath)
            : Environment.ExpandEnvironmentVariables(userSettingsFilePathOverride);
        var userOptionsService = _serviceProvider.GetRequiredService<IMsuUserOptionsService>();
        var userOptions = userOptionsService.Initialize(userOptionsPath);
        
        Task.Run(() =>
        {
            var msuLookupService = _serviceProvider.GetRequiredService<IMsuLookupService>();
            msuLookupService.LookupMsus(userOptions.DefaultMsuPath, userOptions.MsuTypePaths); 
        });
        
    }
}