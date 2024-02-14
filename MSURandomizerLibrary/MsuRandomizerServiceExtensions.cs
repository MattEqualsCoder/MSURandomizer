using Microsoft.Extensions.DependencyInjection;
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
        
        services.AddSingleton<IMsuTypeService, MsuTypeService>();
        services.AddSingleton<IMsuDetailsService, MsuDetailsService>();
        services.AddSingleton<IMsuCacheService, MsuCacheService>();
        services.AddSingleton<IMsuLookupService, MsuLookupService>();
        services.AddSingleton<IMsuSelectorService, MsuSelectorService>();
        services.AddSingleton<IMsuRandomizerInitializationService, MsuRandomizerInitializationService>();
        services.AddSingleton<IMsuGameService, MsuGameService>();
        services.AddSingleton<IMsuMonitorService, MsuMonitorService>();

        services.AddSnesConnectorServices();
        
        return services;
    }
}