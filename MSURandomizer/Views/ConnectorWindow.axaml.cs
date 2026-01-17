using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Extensions;
using AvaloniaControls.Models;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class ConnectorWindow : Window
{
    private ConnectorWindowViewModel _model;
    private ConnectorWindowService? _service;
    
    public ConnectorWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = _model = new ConnectorWindowViewModel();
        }
        else
        {
            _service = this.GetControlService<ConnectorWindowService>();
            DataContext = _model = _service?.InitializeModel() ??  new ConnectorWindowViewModel();
        }
    }

    private void AcceptButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void ConnectorTypeEnumComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _service?.ConnectToSnes();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _service?.OnClose();
    }
}