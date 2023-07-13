using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;

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
    
    public void Initialize(MsuRandomizerInitializationRequest request)
    {
        if (request.MsuAppSettingsStream == null && string.IsNullOrWhiteSpace(request.MsuAppSettingsPath))
        {
            throw new InvalidOperationException(
                "Initialization requires either the MsuAppSettingsStream or MsuAppSettingsPath to be specified");
        }

        var appSettings = request.MsuAppSettingsStream == null
            ? _msuAppSettingsService.Initialize(request.MsuAppSettingsPath!)
            : _msuAppSettingsService.Initialize(request.MsuAppSettingsStream!);

        if (appSettings == null)
        {
            throw new InvalidOperationException("Unable to initialize MSU Randomizer App Settings");
        }

        InitializeMsuTypes(request, appSettings);
        var userOptions = InitializeUserOptions(request, appSettings);
        
        Task.Run(() =>
        {
            var msuLookupService = _serviceProvider.GetRequiredService<IMsuLookupService>();
            msuLookupService.LookupMsus(userOptions.DefaultMsuPath, userOptions.MsuTypePaths); 
        });
    }

    private void InitializeMsuTypes(MsuRandomizerInitializationRequest request, MsuAppSettings appSettings)
    {
        var msuTypeService = _serviceProvider.GetRequiredService<IMsuTypeService>();
        if (request.MsuTypeConfigStream != null)
        {
            msuTypeService.LoadMsuTypes(request.MsuTypeConfigStream);
        }
        else
        {
            var msuTypePath = string.IsNullOrWhiteSpace(request.MsuTypeConfigPath)
                ? appSettings.MsuTypeFilePath
                : request.MsuTypeConfigPath;

            if (string.IsNullOrWhiteSpace(msuTypePath))
            {
                throw new InvalidOperationException("Missing MSU Type configuration");
            }
            
            if ((msuTypePath.ToLower().EndsWith(".yaml") || msuTypePath.ToLower().EndsWith(".yml")) && File.Exists(msuTypePath))
            {
                using var yamlFile = new FileStream(msuTypePath, FileMode.Open);
                msuTypeService.LoadMsuTypes(yamlFile);
            }
            else
            {
                msuTypeService.LoadMsuTypes(msuTypePath);
            }
        }
    }

    private MsuUserOptions InitializeUserOptions(MsuRandomizerInitializationRequest request, MsuAppSettings msuAppSettings)
    {
        var basePath = string.IsNullOrWhiteSpace(request.UserOptionsPath)
            ? msuAppSettings.UserOptionsFilePath
            : request.UserOptionsPath;
        
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new InvalidOperationException("Missing User Settings Path configuration");
        }
        
        var userOptionsPath = Environment.ExpandEnvironmentVariables(basePath);
        var userOptionsService = _serviceProvider.GetRequiredService<IMsuUserOptionsService>();
        return userOptionsService.Initialize(userOptionsPath);
    }
}