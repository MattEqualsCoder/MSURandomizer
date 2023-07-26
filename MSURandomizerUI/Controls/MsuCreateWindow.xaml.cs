using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for GenerateOptionsWindow.xaml
/// </summary>
internal partial class MsuCreateWindow : Window
{
    public new readonly MsuUserOptions DataContext;
    private MsuRandomizationStyle _randomizationStyle;

    public MsuCreateWindow(MsuUserOptions model, MsuRandomizationStyle randomizationStyle)
    {
        _randomizationStyle = randomizationStyle;
        InitializeComponent();
        DataContext = model;
        Title = "MSU Generation Details";
        OpenFolderOnCreateCheckBox.IsEnabled = randomizationStyle != MsuRandomizationStyle.Continuous;
        AvoidDuplicatesCheckBox.IsEnabled = randomizationStyle != MsuRandomizationStyle.Single;
        OpenFolderOnCreateCheckBox.IsChecked = model.OpenFolderOnCreate;
        AvoidDuplicatesCheckBox.IsChecked = model.AvoidDuplicates;
    }

    private void GenerateFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var value = OpenFolderDialog();
        if (string.IsNullOrEmpty(value)) return;
        DataContext.OutputRomPath = null;
        DataContext.OutputFolderPath = value;
        Generate();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
        
    private void GenerateRomMsuButton_Click(object sender, RoutedEventArgs e)
    {
        var value = OpenFileDialog();
        if (string.IsNullOrEmpty(value)) return;
        DataContext.OutputRomPath = value;
        DataContext.OutputFolderPath = null;
        Generate();
    }

    private void Generate()
    {
        DataContext.RandomizationStyle = _randomizationStyle;
        DataContext.Name = NameTextBox.Text;
        DataContext.OpenFolderOnCreate = OpenFolderOnCreateCheckBox.IsChecked == true;
        DataContext.AvoidDuplicates = AvoidDuplicatesCheckBox.IsChecked == true;
        DialogResult = true;
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

    private string OpenFolderDialog(string initDirectory = "")
    {
        using var dialog = new CommonOpenFileDialog
        {
            EnsurePathExists = true,
            Title = "Select output folder",
            InitialDirectory = Directory.Exists(initDirectory) ? initDirectory : "",
            IsFolderPicker = true
        };
            
        return dialog.ShowDialog(this) == CommonFileDialogResult.Ok ? dialog.FileName : initDirectory;
    }
}