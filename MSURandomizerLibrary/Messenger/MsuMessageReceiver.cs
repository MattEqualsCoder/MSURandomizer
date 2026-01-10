using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Messenger;

internal class MsuMessageReceiver(ILogger<MsuMessageReceiver> logger) : IMsuMessageReceiver, IDisposable
{
    public event EventHandler<MsuTrackChangedEventArgs>? TrackChanged;
    public event EventHandler<MsuGeneratedEventArgs>? MsuGenerated;
    private WebApplication? _webApplication;
    private CancellationTokenSource _cts = new();
    
    public IMsuMessageReceiver Initialize()
    {
        var builder = WebApplication.CreateBuilder();
            
        builder.Services.AddLogging(
            logging =>
            {
                logging.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });
        
        builder.Services.AddGrpc();
        builder.Services.AddSingleton<MsuMessageReceiverProxy>();
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, 0, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
        
        builder.Host.ConfigureHostOptions(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(3); // Increase or decrease as needed
        });
        
        _webApplication = builder.Build();

        var proxy = _webApplication.Services.GetRequiredService<MsuMessageReceiverProxy>();
        proxy.TrackChanged += (_, args) =>
        {
            TrackChanged?.Invoke(this, args);
        };
        
        proxy.MsuGenerated += (_, args) =>
        {
            MsuGenerated?.Invoke(this, args);
        };
        
        _webApplication.MapGrpcService<MsuMessageReceiverGrpcService>();
        _webApplication.StartAsync(_cts.Token);

        var url = _webApplication.Urls.FirstOrDefault();

        if (!string.IsNullOrEmpty(url))
        {
            var directory = Path.GetDirectoryName(Shared.GrpcUrlFile)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(Shared.GrpcUrlFile, url);
            logger.LogInformation("Writing Grpc url {Url} to {Path}", url, Shared.GrpcUrlFile);
        }

        return this;
    }

    public void Dispose()
    {
        _cts.Dispose();
        
        Task.Run(async () =>
        {
            if (_webApplication != null)
            {
                await _webApplication.StopAsync();
                await _webApplication.DisposeAsync();
            }
        });
        
        GC.SuppressFinalize(this);
    }
}