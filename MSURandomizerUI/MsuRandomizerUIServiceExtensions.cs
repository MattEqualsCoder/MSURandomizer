using Microsoft.Extensions.DependencyInjection;
using MSURandomizerUI.Controls;
using MSURandomizerUI.Models;

namespace MSURandomizerUI;

/// <summary>
/// Service extensions for adding the UI elements to the service collection
/// </summary>
public static class MsuRandomizerUIServiceExtensions
{
    /// <summary>
    /// Adds the MSU Randomizer UI elements to the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMsuRandomizerUIServices(this IServiceCollection services)
    {
        services.AddSingleton<IMsuUiFactory, MsuUiFactory>();
        
        services.AddTransient<MsuListViewModel>();
        services.AddTransient<MsuList>();
        services.AddTransient<MsuWindow>();
        services.AddTransient<MsuUserSettingsWindow>();
        services.AddTransient<MsuContinuousShuffleWindow>();
        services.AddTransient<MsuDetailsWindow>();
        
        return services;
    }
}