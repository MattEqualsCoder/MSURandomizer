using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSURandomizerLibrary.Configs;
using MSURandomizerUI.Models;

namespace MSURandomizerUI.Controls;

internal partial class MsuDirectoryControl : UserControl
{
    private MsuUserSettingsWindow? _parentWindow;

    public MsuPathViewModel Model { get; init; }
    
    public MsuDirectoryControl(MsuUserSettingsWindow parentWindow, MsuType? msuType, string? msuDirectory)
    {
        _parentWindow = parentWindow;
        InitializeComponent();
        DataContext = Model = new MsuPathViewModel(msuType, msuDirectory);
        NameLabeledControl.Text = msuType?.DisplayName == null ? "Default MSU Directory" : $"{msuType.DisplayName}";
        OutputFolderTextBox.Text = msuDirectory ?? "";
        ClearFolderButton.IsEnabled = !string.IsNullOrWhiteSpace(msuDirectory);
    }
    
    private void OutputFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog($"Select {NameLabeledControl.Text}", Model.MsuPath ?? "");
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

        if (_parentWindow != null && dialog.ShowDialog(_parentWindow) == CommonFileDialogResult.Ok && dialog.FileName != Model.MsuPath)
        {
            Model.MsuPath = dialog.FileName;
            OutputFolderTextBox.Text = dialog.FileName;
            ClearFolderButton.IsEnabled = !string.IsNullOrWhiteSpace(dialog.FileName);
        }
    }

    private void ClearFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.MsuPath = "";
        OutputFolderTextBox.Text = "";
        ClearFolderButton.IsEnabled = false;
    }
}