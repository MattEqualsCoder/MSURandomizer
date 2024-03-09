using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.Views;

public partial class SnesConnectorSelectionWindow : ScalableWindow
{
    private readonly SnesConnectorSelectionService? _service;
    private readonly SnesConnectorSelectionViewModel _model;
    
    public SnesConnectorSelectionWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
        
        if (Design.IsDesignMode)
        {
            DataContext = _model = new SnesConnectorSelectionViewModel();
            return;
        }

        _service = IControlServiceFactory.GetControlService<SnesConnectorSelectionService>();
        DataContext = _model = _service.InitializeModel();
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
        Close(_model.Msus);
    }

    private void SnesConnectorTypeComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _service?.ConnectToSnes();
    }
}