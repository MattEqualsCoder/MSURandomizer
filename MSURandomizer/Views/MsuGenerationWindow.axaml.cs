using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary;

namespace MSURandomizer.Views;

public partial class MsuGenerationWindow : Window
{
    private readonly MsuGenerationWindowService? _service;
    
    public MsuGenerationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MsuGenerationViewModel();
            return;
        }

        _service = IControlServiceFactory.GetControlService<MsuGenerationWindowService>();
    }

    public bool DialogResult { get; private set; }

    public void ShowDialog(Window window, MsuRandomizationStyle style, string outputMsuType, ICollection<string> selectedMsus)
    {
        DataContext = _service?.InitializeModel(style, outputMsuType, selectedMsus);
        Owner = window;
        ShowDialog(window);
    }
    
    private void SelectRomButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ITaskService.Run(OpenFileDialog);
    }

    private void SelectFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ITaskService.Run(OpenFolderDialog);
    }

    private async void OpenFileDialog()
    {
        if (_service == null)
        {
            return;
        }
        
        var previousFolder = _service.Model.OutputRomPath ?? _service.Model.OutputFolderPath;
        var path = await CrossPlatformTools.OpenFileDialogAsync(this, FileInputControlType.OpenFile, "Rom Files:*.sfc,*.smc,*.gb,*.gbc", previousFolder);
        if (path is not IStorageFile file || file.TryGetLocalPath() == null)
        {
            return;
        }

        _service?.SaveFile(file.TryGetLocalPath()!);
        
        DialogResult = true;
        
        Dispatcher.UIThread.Invoke(Close);
    }
    
    private async void OpenFolderDialog()
    {
        if (_service == null)
        {
            return;
        }
        
        var previousFolder = _service.Model.OutputFolderPath ?? _service.Model.OutputRomPath;
        var path = await CrossPlatformTools.OpenFileDialogAsync(this, FileInputControlType.Folder, "", previousFolder);
        if (path is not IStorageFolder file || file.TryGetLocalPath() == null)
        {
            return;
        }

        _service?.SaveFolder(file.TryGetLocalPath()!);

        DialogResult = true;

        Dispatcher.UIThread.Invoke(Close);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CreateMsuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.Save();
        DialogResult = true;
        Close();
    }
}