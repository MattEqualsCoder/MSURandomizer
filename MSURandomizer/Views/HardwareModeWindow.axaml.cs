using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Extensions;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.Views;

public partial class HardwareModeWindow : ScalableWindow
{
    private readonly HardwareModeWindowService? _service;
    private readonly HardwareModeWindowViewModel _model;
    
    public HardwareModeWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
        
        if (Design.IsDesignMode)
        {
            DataContext = _model = new HardwareModeWindowViewModel();
            return;
        }

        _service = this.GetControlService<HardwareModeWindowService>();
        DataContext = _model = _service!.InitializeModel();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _service?.Disconnect();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(new List<Msu>());
    }

    private void StartHardwareModeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.Save();
        Close(_model.Msus);
    }

    private void SnesConnectorTypeComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _service?.ConnectToSnes();
    }
}