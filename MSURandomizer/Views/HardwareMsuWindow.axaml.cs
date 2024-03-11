using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Extensions;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class HardwareMsuWindow : Window
{
    private readonly HardwareMsuWindowService? _service;
    private readonly HardwareMsuViewModel _model;
    
    public HardwareMsuWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = _model = new HardwareMsuViewModel();
            return;
        }

        _service = this.GetControlService<HardwareMsuWindowService>();
        DataContext = _model = _service!.InitializeModel();
        
        Closing += OnClosing;

        if (_service == null)
        {
            return;
        }
        
        _service.OpenMsuMonitorWindow += ServiceOnOpenMsuMonitorWindow;
    }

    private void ServiceOnOpenMsuMonitorWindow(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var window = new MsuMonitorWindow();
            window.Show(_model.SelectedMsu);
            Close();
        });
    }

    public void ShowDialog(Window parentWindow, List<MsuViewModel> msus)
    {
        _model.Msus = msus;
        _service?.Connect();
        ShowDialog(parentWindow);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_service == null) return;
        _service.OpenMsuMonitorWindow -= ServiceOnOpenMsuMonitorWindow;
        _service.Disconnect(!_model.Complete);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}