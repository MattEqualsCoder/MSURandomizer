using AvaloniaControls.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;
using MSURandomizer.Services;
using MSURandomizerLibrary;

namespace MSURandomizer;

public static class MsuRandomizerAppServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerUiServices(this IServiceCollection services)
    {
        services.AddSingleton<AppInitializationService>();
        services.AddTransient<MsuWindow>();
        services.AddTransient<MsuList>();
        services.AddMsuRandomizerServices();
        services.AddAutoMapper(x => x.AddProfile(new ViewModelMapperConfig<Program>()));
        return services;
    }
    
    public static IServiceCollection AddMsuRandomizerAppServices(this IServiceCollection services)
    {
        services.AddSingleton<AppInitializationService>();
        services.AddTransient<MsuWindow>();
        services.AddTransient<MsuList>();
        services.AddAvaloniaControlServices<Program>();
        services.AddMsuRandomizerServices();
        services.AddAutoMapper(x => x.AddProfile(new ViewModelMapperConfig<Program>()));
        return services;
    }
}