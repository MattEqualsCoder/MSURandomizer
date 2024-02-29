using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using MSURandomizerUI;
using Serilog;

namespace MSURandomizerOld
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IHost? _host;
        private ILogger<App>? _logger;
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
#if DEBUG
                .WriteTo.Console()
                .WriteTo.Debug()
#endif
                .CreateLogger();
            
            _host = Host.CreateDefaultBuilder(e.Args)
                .UseSerilog()
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureServices(services =>
                {
                    services.AddMsuRandomizerServices();
                    services.AddMsuRandomizerUIServices();
                    services.AddGitHubReleaseCheckerServices();
                })
                .Start();
            
            _logger = _host.Services.GetRequiredService<ILogger<App>>();
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            _logger.LogInformation("Starting MSU Randomizer {Version}", version.ProductVersion ?? "");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var msuInitializationRequest = new MsuRandomizerInitializationRequest();

            #if DEBUG
            msuInitializationRequest.UserOptionsPath = Path.Combine(AppDataFolder, "msu-user-settings-debug.yml");
            #endif
            
            _host.Services.GetRequiredService<IMsuRandomizerInitializationService>().Initialize(msuInitializationRequest);

            var userOptions = _host.Services.GetRequiredService<MsuUserOptions>();
            if (userOptions.PromptOnUpdate)
            {
                _ = CheckVersion(version.ProductVersion ?? "", userOptions.PromptOnPreRelease);
            }
            
            _host.Services.GetRequiredService<IMsuGameService>().InstallLuaScripts();

            _host.Services.GetRequiredService<IMsuUiFactory>().OpenMsuWindow(SelectionMode.Multiple, false, out _);
        }

        private async Task CheckVersion(string version, bool promptOnPreRelease)
        {
            var newerHubRelease = await _host!.Services.GetRequiredService<IGitHubReleaseCheckerService>()
                .GetGitHubReleaseToUpdateToAsync("MattEqualsCoder", "MSURandomizer", version, promptOnPreRelease);

            if (newerHubRelease != null)
            {
                var response = MessageBox.Show("A new version of the MSU Randomizer is now available!\n" +
                                               "Do you want to open up the GitHub release page for the update?\n" +
                                               "\n" +
                                               "You can disable this check in the options window.", "MSU Randomizer Update",
                    MessageBoxButton.YesNo);

                if (response == MessageBoxResult.Yes)
                {
                    var url = newerHubRelease.Url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                _logger?.LogCritical(ex, "[CRASH] Uncaught {ExceptionType}: ", ex.GetType().Name);
            else
                _logger?.LogCritical("Unhandled exception in current domain but exception object is not an exception ({Obj})", e.ExceptionObject);
            
            var response = MessageBox.Show("A critical error has occurred. Please open an issue at\n" + 
                            "https://github.com/MattEqualsCoder/MSURandomizer/issues.\n" +
                            "Press Yes to open the log directory.",
                "MSU Randomizer",
                MessageBoxButton.YesNo);
            
            if (response != MessageBoxResult.Yes) return;
            var startInfo = new ProcessStartInfo
            {
                Arguments = AppDataFolder, 
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }
        
        private static string AppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSURandomizer");
#if DEBUG
        private static string LogPath => Path.Combine(AppDataFolder, "msu-randomizer-debug_.log");
#else
        private static string LogPath => Path.Combine(AppDataFolder, "msu-randomizer.log");
#endif
        
    }
}
