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
    
    public void Initialize(string randomizerSettingsPath)
    {
        var randomizerSettings = _msuAppSettingsService.Initialize(randomizerSettingsPath);
        InitializeInternal(randomizerSettings);
    }

    public void Initialize(Stream randomizerSettingsStream)
    {
        var randomizerSettings = _msuAppSettingsService.Initialize(randomizerSettingsStream);
        InitializeInternal(randomizerSettings);
    }

    private void InitializeInternal(MsuAppSettings msuAppSettings)
    {
        var msuTypePath = Environment.ExpandEnvironmentVariables(msuAppSettings.MsuTypeFilePath);
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
        
        var userOptionsPath = Environment.ExpandEnvironmentVariables(msuAppSettings.UserSettingsFilePath);
        var userOptionsService = _serviceProvider.GetRequiredService<IMsuUserOptionsService>();
        var userOptions = userOptionsService.Initialize(userOptionsPath);
        
        Task.Run(() =>
        {
            var msuLookupService = _serviceProvider.GetRequiredService<IMsuLookupService>();
            msuLookupService.LookupMsus(userOptions.DefaultMsuPath, userOptions.MsuTypePaths); 
        });
        
    }
}