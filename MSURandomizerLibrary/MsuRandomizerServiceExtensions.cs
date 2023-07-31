using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibrary;

public static class MsuRandomizerServiceExtensions
{
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
        
        return services;
    }
}