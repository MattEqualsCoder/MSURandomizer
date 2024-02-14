using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerUI.Models;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for GenerateOptionsWindow.xaml
/// </summary>
internal partial class MsuCreateWindow : Window
{
    private readonly MsuCreateViewModel _model;
    private readonly MsuUserOptions _userOptions;

    public MsuCreateWindow(MsuUserOptions model, MsuRandomizationStyle randomizationStyle, MsuAppSettings appSettings)
    {
        _userOptions = model;
        _userOptions.RandomizationStyle = randomizationStyle;
        InitializeComponent();
        DataContext = _model = new MsuCreateViewModel(model, appSettings);
        Title = "MSU Generation Details";
    }

    private void GenerateFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var value = OpenFolderDialog();
        if (string.IsNullOrEmpty(value)) return;
        _userOptions.OutputRomPath = null;
        _userOptions.OutputFolderPath = value;
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
        _userOptions.OutputRomPath = value;
        _userOptions.OutputFolderPath = null;
        Generate();
    }

    private void Generate()
    {
        _model.UpdateSettings(_userOptions);
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
        using var dialog = new CommonOpenFileDialog();
        dialog.EnsurePathExists = true;
        dialog.Title = "Select output folder";
        dialog.InitialDirectory = Directory.Exists(initDirectory) ? initDirectory : "";
        dialog.IsFolderPicker = true;

        return dialog.ShowDialog(this) == CommonFileDialogResult.Ok ? dialog.FileName : initDirectory;
    }
}