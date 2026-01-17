using Avalonia;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSURandomizer.Services;
using MSURandomizer.Views;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using Serilog;

namespace MSURandomizer;

sealed class Program
{
    internal static IHost? MainHost { get; private set; }
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var loggerConfiguration = new LoggerConfiguration();
        
#if DEBUG
        if (args.Contains("-d"))
        {
            loggerConfiguration = loggerConfiguration.MinimumLevel.Verbose();
        }
        else
        {
            loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();
        }
#else
        if (args.Contains("-d"))
        {
            loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();
        }
        else
        {
            loggerConfiguration = loggerConfiguration.MinimumLevel.Information();
        }
#endif
        
        Log.Logger = loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.File(Directories.LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
#if DEBUG
            .WriteTo.Debug()
            .WriteTo.Console()
#endif
            .CreateLogger();
        
#if DEBUG
        CheckReactiveProperties();
#endif

        MainHost = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureLogging(logging =>
            {
                logging.AddSerilog(dispose: true);
            })
            .ConfigureServices(services =>
            {
                services.AddMsuRandomizerAppServices();
                services.AddGitHubReleaseCheckerServices();
            })
            .Build();
        
        MainHost.Services.GetRequiredService<ITaskService>();
        MainHost.Services.GetRequiredService<IControlServiceFactory>();
        MainHost.Services.GetRequiredService<AppInitializationService>().Initialize(args);
        
        ExceptionWindow.GitHubUrl = "https://github.com/MattEqualsCoder/MSURandomizer/issues";
        ExceptionWindow.LogPath = Directories.LogFolder;
        
        using var source = new CancellationTokenSource();
        
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            ShowExceptionPopup(e).ContinueWith(_ => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
            Dispatcher.UIThread.MainLoop(source.Token);
        }
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions() { UseDBusFilePicker = false, RenderingMode = [ X11RenderingMode.Software ] })
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    
    private static async Task ShowExceptionPopup(Exception e)
    {
        Log.Error(e, "[CRASH] Uncaught {Name}: ", e.GetType().Name);
        var window = new ExceptionWindow();
        window.Show();
        await Dispatcher.UIThread.Invoke(async () =>
        {
            while (window.IsVisible)
            {
                await Task.Delay(500);
            }
        });
    }
    
    private static void CheckReactiveProperties()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var asm in assemblies)
        {
            foreach (var type in asm.GetTypes())
            {
                if (!InheritsFromType<ReactiveObject>(type))
                {
                    continue;
                }
                
                var props = type.GetProperties(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(x => (Property: x, Attributes: x.GetCustomAttributes(true)))
                    .Where(x => x.Attributes.Any(a => a is ReactiveAttribute) && !x.Attributes.Any(a => a is GeneratedCodeAttribute))
                    .ToList();
                
                foreach (var prop in props)
                {
                    Log.Logger.Warning("Class {Class} property {Property} has ReactiveAttribute but is missing partial", type.FullName, prop.Property.Name);
                }
            }
        }
    }
    
    static bool InheritsFromType<T>(Type type)
    {
        var checkType = type;
        while (checkType != null && checkType != typeof(object))
        {
            if (checkType == typeof(T))
                return true;
            checkType = checkType.BaseType;
        }
        return false;
    }
}