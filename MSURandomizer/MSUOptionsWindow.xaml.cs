using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MsuRandomizerLibrary
{
    /// <summary>
    /// Interaction logic for MSUOptionsWindow.xaml
    /// </summary>
    internal partial class MSUOptionsWindow : Window
    {
        //private readonly MSUOptionsViewModel _viewModel;
        
        public MSUOptionsWindow(MSURandomizerOptions options)
        {
            InitializeComponent();
            //DataContext = _viewModel = new MSUOptionsViewModel(options);
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OutputFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            //_viewModel.Directory = OpenFolderDialog("Select Output Directory", _viewModel.Directory ?? "");
        }
        
        private string OpenFolderDialog(string title, string initDirectory = "")
        {
            
            using var dialog = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                Title = title,
                InitialDirectory = Directory.Exists(initDirectory) ? initDirectory : "",
                IsFolderPicker = true
            };
            
            return dialog.ShowDialog(this) == CommonFileDialogResult.Ok ? dialog.FileName : initDirectory;
        }
    }
}
