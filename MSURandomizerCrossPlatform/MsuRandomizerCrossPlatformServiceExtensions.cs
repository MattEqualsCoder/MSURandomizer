using AvaloniaControls.ControlServices;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerCrossPlatform.Services;
using MSURandomizerCrossPlatform.Views;

namespace MSURandomizerCrossPlatform;

public static class MsuRandomizerCrossPlatformServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerCrossPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<TaskService>();
        services.AddSingleton<MsuWindow>();
        services.AddTransient<MsuList>();
        services.AddControlServices<Program>();
        return services;
    }
}