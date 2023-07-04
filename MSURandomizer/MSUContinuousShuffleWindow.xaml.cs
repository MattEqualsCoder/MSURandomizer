using System;
using System.Timers;
using System.Windows;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace MsuRandomizerLibrary
{
    /// <summary>
    /// Interaction logic for MSUContinuousShuffleWindow.xaml
    /// </summary>
    public partial class MSUContinuousShuffleWindow : Window
    {
        private const int Duration = 1 * 60 * 1000;
        private readonly MSURandomizerOptions _options;
        private readonly Timer _reshuffleTimer;
        private readonly string? _romPath;

        public MSUContinuousShuffleWindow(MSURandomizerOptions options, string? romPath)
        {
            InitializeComponent();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            _options = deserializer.Deserialize<MSURandomizerOptions>(serializer.Serialize(options));
            _options.OpenFolderOnCreate = false;
            _options.DeleteFolder = false;
            _romPath = romPath;
            _reshuffleTimer = new Timer(Duration);
            _reshuffleTimer.Elapsed += OnTimedEvent;
            _reshuffleTimer.Interval = Duration;
            _reshuffleTimer.Start();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            UpdateText("Reshuffling...");

            _options.RomPath = _romPath;

            /*UpdateText(!MSURandomizerService.ShuffleMSU(_options, out var error)
                ? "Error while reshuffling"
                : $"Last Reshuffle: {DateTime.Now:h\\:mm tt}");*/
        }

        private void UpdateText(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageTextBlock.Text = text;
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _reshuffleTimer.Stop();
        }

        private void MSUContinuousShuffleWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateText($"Last Shuffle: {DateTime.Now:h\\:mm tt}");
        }
    }
}
