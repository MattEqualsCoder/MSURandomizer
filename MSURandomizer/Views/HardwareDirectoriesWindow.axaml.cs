using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Extensions;
using AvaloniaControls.Models;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.Views;

public partial class HardwareDirectoriesWindow : ScalableWindow
{
    private HardwareDirectoriesWindowService? _service;
    private HardwareDirectoriesWindowViewModel? _model;
    
    public List<Msu>? HardwareMsus { get; set; }
    
    public HardwareDirectoriesWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new HardwareDirectoriesWindowViewModel()
            {
                IsLoadingData = false,
                TreeNodes =
                [
                    new HardwareItem("Folder1", "", "", true)
                    {
                        Directories =
                        [
                            new HardwareItem("Folder2", "", "", true)
                            {
                                Directories =
                                [
                                    new HardwareItem("File1", "", "", false),
                                ]
                            },
                            new HardwareItem("File2", "", "", false),
                            new HardwareItem("File3", "", "", false),
                        ]
                    },
                    new HardwareItem("File4", "", "", false),
                ]
            };
        }
        else
        {
            _service = this.GetControlService<HardwareDirectoriesWindowService>();
            
            if (_service == null)
            {
                return;
            }

            DataContext = _model = _service.InitializeModel(this);
        }
    }

    public async Task<bool?> ShowDialog(Window window, string? msuToUpload)
    {
        if (_model != null)
        {
            _model.MsuToUpload = msuToUpload;
            _service?.LoadData();
        }
        
        return await ShowDialog<bool?>(window);
    }

    private async void CreateDirectoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var messageWindow = new MessageWindow(new MessageWindowRequest()
        {
            Message = "Enter directory name",
            Title = "Enter Directory Name",
            DisplayTextBox = true
        });

        await messageWindow.ShowDialog(this);
        var result = messageWindow.DialogResult;

        if (result?.PressedAcceptButton != true || string.IsNullOrWhiteSpace(result.ResponseText))
        {
            return;
        }

        _service?.CreateNewDirectory(result.ResponseText.Trim());
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_model?.DidUpdate == true && _service != null)
        {
            e.Cancel = true;
            _ = ReloadAndClose();
            return;
        }

        if (_model is { IsLoadingData: true, IsLoadingIndeterminate: false })
        {
            e.Cancel = true;
            _ = ConfirmCancel();
            return;
        }
        
        _service?.Disconnect();
    }

    private async Task ConfirmCancel()
    {
        var response = await MessageWindow.ShowYesNoDialog(
            "Cancelling a transfer can cause issues with some hardware connectors. You may need to restart your device and SNI/QUsb2Snes after cancelling the download. Do you want to continue?",
            "Continue?", this);
        if (response)
        {
            _model!.IsLoadingIndeterminate = true;
            Close();
        }
    }

    private async Task ReloadAndClose()
    {
        if (_service != null)
        {
            HardwareMsus = await _service.ReloadHardwareMsus();
        }

        _model!.DidUpdate = false;
        Close();
    }

    private async void DeleteDirectoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_service == null || _model?.SelectedTreeNode == null)
            {
                return;
            }
            
            var message = _model.SelectedTreeNode.IsFolder
                ? "Are you sure you want to delete this folder and all contents within it?"
                : "Are you sure you want to delete this file?";
            
            var title = _model.SelectedTreeNode.IsFolder
                ? "Delete Folder"
                : "Delete File";
        
            var result = await MessageWindow.ShowYesNoDialog(message, title, parentWindow: this);
            if (result)
            {
                await _service.DeleteSelectedItem();
            }
        }
        catch
        {
            await MessageWindow.ShowErrorDialog("Unable to delete file or folder", "Error", parentWindow: this);
        }
    }

    private async void UploadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_service == null)
            {
                return;
            }

            if (await _service.UploadMsu())
            {
                await MessageWindow.ShowInfoDialog("Upload completed successfully.", "Success", parentWindow: this);
                Close(true);
            }
            else
            {
                await MessageWindow.ShowErrorDialog("Upload did not complete successfully.", "Error", parentWindow: this);
            }
        }
        catch
        {
            // Do nothing
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}