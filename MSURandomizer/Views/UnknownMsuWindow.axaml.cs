using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Extensions;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;

namespace MSURandomizer.Views;

public partial class UnknownMsuWindow : ScalableWindow
{
    private readonly UnknownMsuWindowService? _service;
    private bool _pressedSave;
    
    public UnknownMsuWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new UnknownMsuWindowViewModel()
            {
                UnknownMsus =
                [
                    new MsuDetailsWindowViewModel()
                    {
                        Name = "Test MSU",
                        Creator = "Test MSU Creator",
                        MsuPath = "/home/matt/Documents/Test/test.msu",
                        Tracks = [],
                        TrackCount = 30
                    },
                    new MsuDetailsWindowViewModel()
                    {
                        Name = "Test MSU",
                        Creator = "Test MSU Creator",
                        MsuPath = "/home/matt/Documents/Test2/test2.msu",
                        Tracks = [],
                        TrackCount = 43
                    },
                    new MsuDetailsWindowViewModel()
                    {
                        Name = "Test MSU",
                        Creator = "Test MSU Creator",
                        MsuPath = "/home/matt/Documents/Test3/test3.msu",
                        Tracks = [],
                        TrackCount = 16,
                        IsNotLast = false
                    }
                ]
            };
            return;
        }

        _service = this.GetControlService<UnknownMsuWindowService>();
        DataContext = _service!.InitilizeModel();
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _pressedSave = true;
        _service?.Save();
        Close();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_pressedSave)
        {
            _service?.SaveIgnore();
        }
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        if (!_pressedSave)
        {
            _service?.SaveIgnore();
        }
    }
}