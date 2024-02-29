using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class SettingsWindow : ScalableWindow
{
    private SettingsWindowService? _service;
    
    public SettingsWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new SettingsWindowViewModel()
            {
                MsuTypeNamePathsList = new List<MsuTypePath>()
                {
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                }
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