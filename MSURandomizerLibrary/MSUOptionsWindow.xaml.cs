using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = System.Windows.Shapes.Path;

namespace MSURandomizerLibrary
{
    /// <summary>
    /// Interaction logic for MSUOptionsWindow.xaml
    /// </summary>
    internal partial class MSUOptionsWindow : Window
    {
        private readonly MSUOptionsViewModel _viewModel;
        
        public MSUOptionsWindow(MSURandomizerOptions options)
        {
            InitializeComponent();
            DataContext = _viewModel = new MSUOptionsViewModel(options);
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OutputFolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Directory = OpenFolderDialog("Select Output Directory", _viewModel.Directory ?? "");
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
