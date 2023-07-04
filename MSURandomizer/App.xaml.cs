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
using MsuRandomizerLibrary;
using MsuRandomizerLibrary.Services;

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
                    services.AddMsuRandomizerServices();
                })
                .Start();
            
            _logger = _host.Services.GetRequiredService<ILogger<App>>();
            //_host.Services.GetRequiredService<IMsuTypeService>().LoadMsuTypesFromDirectory(@"D:\Source\ALttPMSUShuffler\resources");
            _host.Services.GetRequiredService<IMsuTypeService>().LoadMsuTypesFromStream( new FileStream(@"D:\Desktop\smz3_tracks.json", FileMode.Open));
            _host.Services.GetRequiredService<IMsuSettingsService>().InitializeSettingsService(Environment.ExpandEnvironmentVariables("%LocalAppData%\\MSURandomizer\\msu-settings.yml"));
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
            if (_initialized) return;
            _initialized = true;

            var app = sender as App;
            if (app?.MainWindow is not MainWindow mainWindow) return;
            
            var msuType = _host.Services.GetRequiredService<IMsuTypeService>().MsuTypes.First(x => x.Name == "Super Metroid / A Link to the Past Combination Randomizer Legacy");

            var msuList = app._host!.Services.GetRequiredService<MsuList>();
            msuList.SelectionMode = SelectionMode.Multiple;
            msuList.TargetMsuType = msuType;
            msuList.MsuFilter = MsuFilter.Compatible;
            msuList.SelectedMsuPaths = new List<string>()
            {
                @"E:\SMZ3\MSUs\Sonic\SMZ3_Sonic.msu",
                @"E:\SMZ3\MSUs\RetroPC\RPC-LTTP-MSU.msu"
            };
            mainWindow.MsuList = msuList;
            mainWindow.MainGrid.Children.Add(msuList);
            
            app._host.Services.GetRequiredService<IMsuLookupService>().LookupMsus(@"E:\SMZ3\MSUs");

            var msu = _host.Services.GetRequiredService<IMsuLookupService>().Msus.FirstOrDefault();
            var availableMsus = _host.Services.GetRequiredService<IMsuLookupService>().Msus.ToList();
            var convertedMsu = _host.Services.GetRequiredService<IMsuSelectorService>().CreateShuffledMsu(availableMsus, msuType, @"E:\SMZ3\MSUs\Test\");
            
            app._host.Services.GetRequiredService<IMsuSettingsService>().UpdateMsuSettings(msu, new() { Name = "Test", Creator = "Vivelin", MsuPath = msu.Path});
        }
    }
}
