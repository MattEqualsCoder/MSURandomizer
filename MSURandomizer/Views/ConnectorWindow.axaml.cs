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
    
    public bool ClickedPrimaryButton { get; private set; }
    
    public bool ClickedSecondaryButton { get; private set; }
    
    public bool DisconnectOnClose { get; private set; }
    
    public void UpdateButtons(string primaryButtonText, string? secondaryButtonText = null)
    {
        _model.PrimaryButtonText = primaryButtonText;
        _model.SecondaryButtonText = secondaryButtonText ?? "";
        _model.DisplaySecondaryButton = !string.IsNullOrEmpty(secondaryButtonText);
    }

    private void AcceptButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ClickedPrimaryButton = true;
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
        if (DisconnectOnClose)
        {
            _service?.Disconnect();
        }
        _service?.OnClose();
    }

    private void AcceptSecondaryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ClickedSecondaryButton = true;
        Close(true);
    }
}