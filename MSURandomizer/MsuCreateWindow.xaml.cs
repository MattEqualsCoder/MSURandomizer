using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MsuRandomizer;

/// <summary>
/// Interaction logic for GenerateOptionsWindow.xaml
/// </summary>
internal partial class MsuCreateWindow : Window
{
    public new readonly MsuRandomizerOptions DataContext;
    private readonly bool _shuffledMsuWindow = false;

    public MsuCreateWindow(MsuRandomizerOptions model, bool shuffledMsuWindow)
    {
        InitializeComponent();
        DataContext = model;
        _shuffledMsuWindow = shuffledMsuWindow;
        Title = shuffledMsuWindow ? "Create Shuffled MSU" : "Pick Random MSU";
        AvoidDuplicatesCheckBox.Visibility = shuffledMsuWindow ? Visibility.Visible : Visibility.Collapsed;
        ContinousReshuffleCheckBox.Visibility = shuffledMsuWindow
            ? Visibility.Visible 
            : Visibility.Collapsed;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var value = OpenFolderDialog();
        if (string.IsNullOrEmpty(value)) return;
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
        Generate();
    }

    private void Generate()
    {
        DataContext.Name = NameTextBox.Text;
        DataContext.AvoidDuplicates = !_shuffledMsuWindow && AvoidDuplicatesCheckBox.IsChecked == true;
        DataContext.ContinuousReshuffle = !_shuffledMsuWindow && ContinousReshuffleCheckBox.IsChecked == true;
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