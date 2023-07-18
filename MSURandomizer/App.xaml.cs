using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using MSURandomizerLibrary.UI;

namespace MSURandomizer
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
            _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFile($"%LocalAppData%\\MSURandomizer\\msu-{DateTime.UtcNow:yyyyMMdd}.log", options =>
                    {
                        options.Append = true;
                        options.FileSizeLimitBytes = 52428800;
                        options.MaxRollingFiles = 5;
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddMsuRandomizerServices();
                })
                .Start();
            
            _logger = _host.Services.GetRequiredService<ILogger<App>>();
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            _logger.LogInformation("Starting MSU Randomizer {Version}", version.ProductVersion ?? "");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var settingsStream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("MSURandomizer.settings.yaml");
            if (settingsStream == null)
            {
                throw new InvalidOperationException("Missing RandomizerSettings stream");
            }

            var msuInitializationRequest = new MsuRandomizerInitializationRequest
            {
                MsuAppSettingsStream = settingsStream
            };

            #if DEBUG
            msuInitializationRequest.MsuTypeConfigPath = GetConfigDirectory();
            msuInitializationRequest.UserOptionsPath = "%LocalAppData%\\MSURandomizer\\msu-user-settings-debug.yml";
            #endif
            _host.Services.GetRequiredService<IMsuRandomizerInitializationService>().Initialize(msuInitializationRequest);
            _host.Services.GetRequiredService<MsuWindow>().Show();
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
            var logFileLocation = Environment.ExpandEnvironmentVariables("%LocalAppData%\\MSURandomizer");
            var startInfo = new ProcessStartInfo
            {
                Arguments = logFileLocation, 
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }
        
        #if DEBUG
        public string GetConfigDirectory()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory != null ? Path.Combine(directory.FullName, "ConfigRepo", "resources") : "";
        }
        #endif
    }
}
