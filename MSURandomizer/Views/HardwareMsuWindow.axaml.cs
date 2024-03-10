using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class HardwareMsuWindow : Window
{
    private HardwareMsuWindowService? _service;
    private HardwareMsuViewModel _model;
    
    public HardwareMsuWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = _model = new HardwareMsuViewModel();
            return;
        }

        _service = IControlServiceFactory.GetControlService<HardwareMsuWindowService>();
        DataContext = _model = _service.InitializeModel();
        
        Closing += OnClosing;
    }

    public void ShowDialog(Window parentWindow, List<MsuViewModel> msus)
    {
        _model.Msus = msus;
        _service?.UploadMsuRom();
        ShowDialog(parentWindow);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _service?.Disconnect();
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.Disconnect();
    }
}