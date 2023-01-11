using System;
using System.IO;
using System.Windows;
using MSURandomizerLibrary;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MSURandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ISerializer _serializer;
        private MainWindowViewModel _viewModel;
        
        public MainWindow()
        {
            _serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            var options = LoadOptions();
            var msuTypePath = Environment.ExpandEnvironmentVariables("%LocalAppData%\\MSURandomizer\\configs");
            options.MsuTypeConfigPath = msuTypePath;
            DataContext = _viewModel = new MainWindowViewModel() { Options = options };
            InitializeComponent();
        }
        
        private static string GetConfigPath()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MSURandomizer");
            Directory.CreateDirectory(basePath);
            return Path.Combine(basePath, "options.yml");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MsuRandomizerControl.Options = _viewModel.Options;
            MsuRandomizerControl.OnRomGenerated += (o, args) => SaveOptions(args.Options);
            MsuRandomizerControl.OnSettingsUpdated += (o, args) => SaveOptions(args.Options);
        }

        private MSURandomizerOptions LoadOptions()
        {
            var optionsPath = GetConfigPath();
            
            if (!File.Exists(optionsPath))
            {
                var options = new MSURandomizerOptions();
                SaveOptions(options);
                return options;
            }
            else
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
                var yaml = File.ReadAllText(optionsPath);
                return deserializer.Deserialize<MSURandomizerOptions>(yaml);
            }
        }

        private void SaveOptions(MSURandomizerOptions options)
        {
            var yaml = _serializer.Serialize(options);
            File.WriteAllText(GetConfigPath(), yaml);
        }
    }
}
