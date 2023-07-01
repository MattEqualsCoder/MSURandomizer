using Microsoft.Extensions.DependencyInjection;

namespace MSURandomizerLibrary.Services;

public static class MsuRandomizerServiceExtensions
{
    public static IServiceCollection AddMsuRandomizerServices(this IServiceCollection services)
    {
        services.AddSingleton<IMsuTypeService, MsuTypeService>();
        services.AddSingleton<IMsuLookupService, MsuLookupService>();
        return services;
    }
}