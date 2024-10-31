using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Messenger;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizerLibrary;

/// <summary>
/// ServiceCollection extension for adding the MSU Randomizer services
/// </summary>
public static class MsuRandomizerServiceExtensions
{
    /// <summary>
    /// Adds all required services for the MSU randomizer to the service collection
    /// </summary>
    /// <param name="services">The service collection object</param>
    /// <returns>The service collection object</returns>
    public static IServiceCollection AddMsuRandomizerServices(this IServiceCollection services)
    {
        services.AddSingleton<IMsuUserOptionsService, MsuUserOptionsService>();
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMsuUserOptionsService>().MsuUserOptions);
        
        services.AddSingleton<IMsuAppSettingsService, MsuMsuAppSettingsService>();
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMsuAppSettingsService>().MsuAppSettings);
        
        services.AddScoped<IMsuMessageReceiver, MsuMessageReceiver>();
        services.AddSingleton<IMsuMessageSender>(serviceProvider =>
        {
            var appSettings = serviceProvider.GetRequiredService<IMsuAppSettingsService>().MsuAppSettings;
            if (appSettings.DisableMessageSender)
            {
                return new MsuMessageSenderNoOp();
            }
            else
            {
                var logger = serviceProvider.GetRequiredService<ILogger<MsuMessageSender>>();
                return new MsuMessageSender(logger);
            }
        });
        
        services.AddSingleton<IMsuTypeService, MsuTypeService>();
        services.AddSingleton<IMsuDetailsService, MsuDetailsService>();
        services.AddSingleton<IMsuCacheService, MsuCacheService>();
        services.AddSingleton<IMsuLookupService, MsuLookupService>();
        services.AddSingleton<IMsuSelectorService, MsuSelectorService>();
        services.AddSingleton<IMsuRandomizerInitializationService, MsuRandomizerInitializationService>();
        services.AddSingleton<IMsuGameService, MsuGameService>();
        services.AddSingleton<IMsuMonitorService, MsuMonitorService>();
        services.AddSingleton<IRomLauncherService, RomLauncherService>();
        services.AddSingleton<IMsuHardwareService, MsuHardwareService>();
        services.AddSingleton<IRomCopyService, RomCopyService>();
        
        services.AddSnesConnectorServices();
        
        return services;
    }
}