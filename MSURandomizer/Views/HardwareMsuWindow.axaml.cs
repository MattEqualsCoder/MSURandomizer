using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Controls;

namespace MSURandomizer.Views;

public partial class HardwareMsuWindow : ScalableWindow
{
    public HardwareMsuWindow()
    {
        InitializeComponent();
    }

    public bool DialogResult { get; private set; } = true;
    
    private void CreateMsuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}