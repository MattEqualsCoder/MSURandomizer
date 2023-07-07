using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MsuRandomizer;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Services;

namespace MSURandomizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        private ILogger<App>? _logger;
        private bool _initialized = false;
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFile($"%LocalAppData%\\MSURandomizer\\msu-{DateTime.UtcNow:yyyyMMdd}.log", options =>
                    {
                        options.Append = true;
                        options.FileSizeLimitBytes = 1048576;
                        options.MaxRollingFiles = 5;
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<MsuRandomizerOptionsFactory>();
                    services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<MsuRandomizerOptionsFactory>().GetOptions());
                    services.AddMsuRandomizerServices();
                    services.AddSingleton<MsuRandomizerService>();
                    services.AddScoped<MainWindow>();
                    services.AddScoped<MsuOptionsWindow>();
                })
                .Start();
            
            _logger = _host.Services.GetRequiredService<ILogger<App>>();
            _host.Services.GetRequiredService<MsuRandomizerService>();
            _host.Services.GetRequiredService<IMsuTypeService>().LoadMsuTypesFromDirectory(@"G:\Source\Randomizers\ALttPMSUShuffler\resources");
            _host.Services.GetRequiredService<IMsuSettingsService>().InitializeSettingsService(Environment.ExpandEnvironmentVariables("%LocalAppData%\\MSURandomizer\\msu-settings.yml"));
            
            
            Task.Run(() =>
            {
                _host.Services.GetRequiredService<IMsuLookupService>().LookupMsus(@"D:\Games\SMZ3\SMZ3MSUs");
            });
            
            _host.Services.GetRequiredService<MainWindow>().ShowDialog();
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            
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

        private void App_OnActivated(object? sender, EventArgs e)
        {
            /*if (_initialized) return;
            _initialized = true;
            
            var app = sender as App;
            if (app?.MainWindow is not MainWindow mainWindow || _host == null) return;
            

            var start = DateTime.Now;
            var msuType = _host.Services.GetRequiredService<IMsuTypeService>().MsuTypes.First(x => x.Name == "Super Metroid");
            var end = DateTime.Now;
            var typeLookupDuration = (end - start).TotalSeconds;

            var msuList = app._host!.Services.GetRequiredService<IMsuUiFactory>().CreateMsuList(msuType, MsuFilter.Compatible, SelectionMode.Multiple);
            mainWindow.MsuList = msuList;
            mainWindow.MainGrid.Children.Add(msuList);
            */
            
        }
    }
}
