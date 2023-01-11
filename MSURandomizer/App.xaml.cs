using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MSURandomizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
                        options.FileSizeLimitBytes = 1048576;
                        options.MaxRollingFiles = 5;
                    });
                })
                .Start();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();
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
    }
}
