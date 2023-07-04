using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MsuRandomizerLibrary
{
    /// <summary>
    /// Interaction logic for GenerateOptionsWindow.xaml
    /// </summary>
    internal partial class MSUCreateWindow : Window
    {
        public bool Generate = false;
        public MSURandomizerOptions Options;

        public MSUCreateWindow(MSURandomizerOptions options, bool shuffledMsuWindow)
        {
            InitializeComponent();
            DataContext = Options = options;
            Title = shuffledMsuWindow ? "Create Shuffled MSU" : "Pick Random MSU";
            AvoidDuplicatesCheckBox.Visibility = shuffledMsuWindow ? Visibility.Visible : Visibility.Collapsed;
            GenerateRomMSUButton.Visibility = string.IsNullOrEmpty(Options.ForcedMsuType)
                ? Visibility.Visible
                : Visibility.Collapsed;
            ContinousReshuffleCheckBox.Visibility = shuffledMsuWindow && options.AllowContinuousReshuffle
                ? Visibility.Visible 
                : Visibility.Collapsed;
            options.ContinuousReshuffle = options.ContinuousReshuffle && shuffledMsuWindow;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Generate = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void GenerateRomMSUButton_Click(object sender, RoutedEventArgs e)
        {
            var value = OpenFileDialog();
            if (string.IsNullOrEmpty(value)) return;
            Options.RomPath = value;
            Generate = true;
            Close();
        }
        
        private string OpenFileDialog(string initDirectory = "")
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Select rom file",
                InitialDirectory = Directory.Exists(initDirectory) ? initDirectory : "",
                Filter = "Roms (*.sfc,*.gb,*.gbc)|*.sfc;*.gb;*.gbc|All files (*.*)|*.*"
            };

            return dialog.ShowDialog(this) != true ? "" : dialog.FileName;
        }
    }
}
