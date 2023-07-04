using Microsoft.Extensions.DependencyInjection;

namespace MsuRandomizerLibrary.Services;

public static class MsuRandomizerServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerServices(this IServiceCollection services)
    {
        services.AddSingleton<IMsuSettingsService, MsuSettingsService>();
        services.AddSingleton<IMsuTypeService, MsuTypeService>();
        services.AddSingleton<IMsuDetailsService, MsuDetailsService>();
        services.AddSingleton<IMsuLookupService, MsuLookupService>();
        services.AddSingleton<IMsuSelectorService, MsuSelectorService>();
        services.AddScoped<MsuListViewModel>();
        services.AddScoped<MsuList>();
        return services;
    }
}