using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

internal class MsuRandomizerInitializationService : IMsuRandomizerInitializationService
{
    private readonly IMsuAppSettingsService _msuAppSettingsService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MsuRandomizerInitializationService> _logger;
    
    public MsuRandomizerInitializationService(IMsuAppSettingsService msuAppSettingsService, IServiceProvider serviceProvider, ILogger<MsuRandomizerInitializationService> logger)
    {
        _msuAppSettingsService = msuAppSettingsService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public void Initialize(MsuRandomizerInitializationRequest request)
    {
        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        _logger.LogInformation("Initializing MSU Randomizer Library {Version}", version.ProductVersion ?? "");
        
        var appSettings = request.MsuAppSettingsStream == null
            ? _msuAppSettingsService.Initialize(request.MsuAppSettingsPath)
            : _msuAppSettingsService.Initialize(request.MsuAppSettingsStream);

        if (appSettings == null)
        {
            throw new InvalidOperationException("Unable to initialize MSU Randomizer App Settings");
        }

        InitializeMsuTypes(request, appSettings);
        var userOptions = InitializeUserOptions(request, appSettings);
        
        InitializeCache(request, appSettings);

        if (request.LookupMsus)
        {
            Task.Run(() =>
            {
                var msuLookupService = _serviceProvider.GetRequiredService<IMsuLookupService>();
                msuLookupService.LookupMsus(userOptions.DefaultMsuPath, userOptions.MsuTypePaths); 
            });
        }
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
                var stream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("MSURandomizerLibrary.msu_types.json");
                msuTypeService.LoadMsuTypes(stream!);
                return;
            }
            
            msuTypePath = msuTypePath.ExpandSpecialFolders();
            if ((msuTypePath.ToLower().EndsWith(".json")) && File.Exists(msuTypePath))
            {
                using var jsonStream = new FileStream(msuTypePath, FileMode.Open);
                msuTypeService.LoadMsuTypes(jsonStream);
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

        var userOptionsPath = basePath.ExpandSpecialFolders();
        var userOptionsService = _serviceProvider.GetRequiredService<IMsuUserOptionsService>();
        return userOptionsService.Initialize(userOptionsPath);
    }

    private void InitializeCache(MsuRandomizerInitializationRequest request, MsuAppSettings msuAppSettings)
    {
        var cachePath = string.IsNullOrWhiteSpace(request.MsuCachePath)
            ? msuAppSettings.MsuCachePath
            : request.MsuCachePath;
        
        if (string.IsNullOrEmpty(cachePath)) return;
        
        var msuCacheService = _serviceProvider.GetRequiredService<IMsuCacheService>();
        msuCacheService.Initialize(cachePath.ExpandSpecialFolders());
    }

    private string ConvertPathSeparators(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return path;
        }
        else
        {
            return path.Replace("\\", "/");
        }
    }
}