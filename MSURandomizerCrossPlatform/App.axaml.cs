using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using Serilog;

namespace MSURandomizerCrossPlatform;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var serviceCollection = new ServiceCollection()
                .AddMsuRandomizerServices()
                .AddLogging(logging =>
                {
                    logging.AddSerilog(dispose: true);
                })
                .AddSingleton<MainWindow>();
            var services = serviceCollection.BuildServiceProvider();

            var msuInitializationRequest = new MsuRandomizerInitializationRequest();

            #if DEBUG
            msuInitializationRequest.UserOptionsPath = "%LocalAppData%\\MSURandomizer\\msu-user-settings-debug.yml";
            #endif
            
            services.GetRequiredService<IMsuRandomizerInitializationService>().Initialize(msuInitializationRequest);
            
            desktop.MainWindow = services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}