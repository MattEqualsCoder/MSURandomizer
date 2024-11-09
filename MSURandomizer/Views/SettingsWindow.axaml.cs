using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Extensions;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class SettingsWindow : ScalableWindow
{
    private readonly SettingsWindowService? _service;
    
    public SettingsWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new SettingsWindowViewModel()
            {
                MsuTypeNamePathsList =
                [
                    new MsuTypePath(),
                    new MsuTypePath(),
                    new MsuTypePath(),
                    new MsuTypePath(),
                    new MsuTypePath(),
                ],
                MsuDirectoryList = 
                [
                    new MsuDirectory("/test/path")
                ]
            };
            return;
        }

        _service = this.GetControlService<SettingsWindowService>();
        DataContext = _service!.InitializeModel();
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SaveModel();
        Close();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void AddDirectoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        
        var storageItem = await CrossPlatformTools.OpenFileDialogAsync(this, FileInputControlType.Folder, null, null);
        var path = storageItem?.Path.LocalPath;
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        if (!_service.AddDirectory(path))
        {
            _ = MessageWindow.ShowErrorDialog("That directory has already been selected");
        }
    }

    private void RemoveDirectoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string directory } || _service == null)
        {
            return;
        }
        _service.RemoveDirectory(directory);
    }
}