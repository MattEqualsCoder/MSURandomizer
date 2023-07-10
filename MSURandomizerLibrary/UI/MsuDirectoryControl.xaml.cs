using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.UI;

public partial class MsuDirectoryControl : UserControl
{
    private MsuUserSettingsWindow? _parentWindow;
    
    public MsuDirectoryControl(MsuUserSettingsWindow parentWindow, MsuType? msuType, string? msuDirectory)
    {
        _parentWindow = parentWindow;
        InitializeComponent();
        MsuType = msuType;
        MsuDirectory = msuDirectory;
        MsuNameTextBlock.Text = msuType?.Name == null ? "Default MSU Directory" : $"{msuType.Name} MSU Directory";
        OutputFolderTextBox.Text = msuDirectory ?? "";
        ClearFolderButton.IsEnabled = !string.IsNullOrWhiteSpace(msuDirectory);
    }
    
    public MsuType? MsuType { get; set; }
    
    public string? MsuDirectory { get; set; }
    
    public bool HasModified { get; set; }

    private void OutputFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog($"Select {MsuNameTextBlock.Text}", MsuDirectory ?? "");
    }
    
    private void OpenFolderDialog(string title, string initDirectory = "")
    {
            
        using var dialog = new CommonOpenFileDialog
        {
            EnsurePathExists = true,
            Title = title,
            InitialDirectory = Directory.Exists(initDirectory) ? initDirectory : "",
            IsFolderPicker = true
        };

        if (_parentWindow != null && dialog.ShowDialog(_parentWindow) == CommonFileDialogResult.Ok && dialog.FileName != MsuDirectory)
        {
            HasModified = true;
            MsuDirectory = dialog.FileName;
            OutputFolderTextBox.Text = dialog.FileName;
            ClearFolderButton.IsEnabled = !string.IsNullOrWhiteSpace(dialog.FileName);
        }
    }

    private void ClearFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        MsuDirectory = "";
        OutputFolderTextBox.Text = "";
        ClearFolderButton.IsEnabled = false;
        HasModified = true;
    }
}