using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
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
                ]
            };
            return;
        }

        _service = IControlServiceFactory.GetControlService<SettingsWindowService>();
        DataContext = _service.InitializeModel();
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
}