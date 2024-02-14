using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using MSURandomizerUI.Models;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for MSUOptionsWindow.xaml
/// </summary>
internal partial class MsuUserSettingsWindow : Window
{
    private readonly MsuUserOptionsViewModel _model;
    private readonly MsuUserOptions _msuUserOptions;
    private readonly List<MsuDirectoryControl> _directoryControls = new();
    private readonly string _defaultMsuOutputTextFile;
        
    public MsuUserSettingsWindow(MsuUserOptions msuUserOptions, IMsuTypeService msuTypeService, MsuAppSettings msuAppSettings)
    {
        InitializeComponent();
        _msuUserOptions = msuUserOptions;
        _defaultMsuOutputTextFile = msuAppSettings.DefaultMsuCurrentSongOutputFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), "current_song.txt");
        DataContext = _model = new MsuUserOptionsViewModel(msuUserOptions);
        if (string.IsNullOrWhiteSpace(_model.MsuCurrentSongOutputFilePath))
        {
            _model.MsuCurrentSongOutputFilePath = _defaultMsuOutputTextFile;
        }
        AddDirectoryControl(null, msuUserOptions.DefaultMsuPath);
        foreach(var msuType in msuTypeService.MsuTypes.Where(x => x.Selectable).OrderBy(x => x.DisplayName))
        {
            msuUserOptions.MsuTypePaths.TryGetValue(msuType, out var typePath);
            AddDirectoryControl(msuType, typePath);
        }
    }

    public void AddDirectoryControl(MsuType? msuType, string? path)
    {
        var control = new MsuDirectoryControl(this, msuType, path);
        _directoryControls.Add(control);
        if (msuType == null)
        {
            BaseSettings.Children.Add(control);
        }
        else
        {
            MsuTypeSettings.Children.Add(control);
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_model.HasBeenModified && !_directoryControls.Any(x => x.Model.HasBeenModified))
        {
            Close();
            return;
        }
        
        _model.UpdateSettings(_msuUserOptions);
        foreach (var control in _directoryControls)
        {
            control.Model.UpdateSettings(_msuUserOptions);
        }
            
        DialogResult = true;
            
        Close();
    }

    private void SelectFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var filePath = string.IsNullOrWhiteSpace(_model.MsuCurrentSongOutputFilePath)
            ? _defaultMsuOutputTextFile
            : _model.MsuCurrentSongOutputFilePath;
        var file = new FileInfo(filePath);
        using var dialog = new CommonOpenFileDialog();
        dialog.Title = "Select Current Song File";
        dialog.InitialDirectory = file.DirectoryName;

        if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok && dialog.FileName != _model.MsuCurrentSongOutputFilePath)
        {
            _model.MsuCurrentSongOutputFilePath = dialog.FileName;
            OutputFolderTextBox.Text = dialog.FileName;
            ClearFolderButton.IsEnabled = !string.IsNullOrWhiteSpace(dialog.FileName);
        }
    }

    private void ClearFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        _model.MsuCurrentSongOutputFilePath = _defaultMsuOutputTextFile;
    }
}