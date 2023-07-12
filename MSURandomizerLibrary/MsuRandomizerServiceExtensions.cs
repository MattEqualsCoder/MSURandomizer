using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Services;
using MSURandomizerLibrary.UI;

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
        services.AddSingleton<IMsuLookupService, MsuLookupService>();
        services.AddSingleton<IMsuSelectorService, MsuSelectorService>();
        services.AddSingleton<IMsuUiFactory, MsuUiFactory>();
        
        services.AddTransient<MsuListViewModel>();
        services.AddTransient<MsuList>();
        services.AddTransient<MsuWindow>();
        services.AddTransient<MsuUserSettingsWindow>();
        services.AddTransient<MsuContinuousShuffleWindow>();
        services.AddTransient<MsuDetailsWindow>();
        
        services.AddSingleton<IMsuRandomizerInitializationService, MsuRandomizerInitializationService>();
        return services;
    }
}