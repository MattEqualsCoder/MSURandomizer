using AvaloniaControls.ControlServices;
using AvaloniaControls.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;
using MSURandomizer.Services;
using MSURandomizerLibrary;

namespace MSURandomizer;

public static class MsuRandomizerAppServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerAppServices(this IServiceCollection services)
    {
        services.AddSingleton<AppInitializationService>();
        services.AddTransient<MsuWindow>();
        services.AddTransient<MsuList>();
        services.AddAvaloniaControlServices<Program>();
        services.AddMsuRandomizerServices();
        return services;
    }
}