using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Services;
using MSURandomizerUI.Controls;
using MSURandomizerUI.Models;

namespace MSURandomizerUI;

public static class MsuRandomizerUIServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerUIServices(this IServiceCollection services)
    {
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