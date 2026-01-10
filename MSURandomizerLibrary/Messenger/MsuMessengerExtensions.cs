using Microsoft.Extensions.DependencyInjection;

namespace MSURandomizerLibrary.Messenger;

/// <summary>
/// Extensions for adding the MSU Messenger Receiver and Sender classes to the service collection
/// </summary>
public static class MsuMessengerExtensions
{
    /// <summary>
    /// Adds the MSU Message Receiver to the service collection
    /// </summary>
    public static IServiceCollection AddMsuMessageReceiver(this IServiceCollection services)
    {
        return services.AddTransient<IMsuMessageReceiver, MsuMessageReceiver>();
    }

    /// <summary>
    /// Adds the MSU Message Sender to the service collection
    /// </summary>
    public static IServiceCollection AddMsuMessageSender(this IServiceCollection services)
    {
        return services.AddSingleton<IMsuMessageSender, MsuMessageSender>();
    }
    
    /// <summary>
    /// Adds a mock MSU Message Sender to the service collection
    /// </summary>
    public static IServiceCollection AddMsuMessageSenderNoOp(this IServiceCollection services)
    {
        return services.AddSingleton<IMsuMessageSender, MsuMessageSenderNoOp>();
    }
}