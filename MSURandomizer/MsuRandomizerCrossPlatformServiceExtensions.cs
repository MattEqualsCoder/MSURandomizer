using AvaloniaControls.ControlServices;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;
using MSURandomizer.Services;

namespace MSURandomizer;

public static class MsuRandomizerCrossPlatformServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerCrossPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<MsuWindow>();
        services.AddTransient<MsuList>();
        return services;
    }
}