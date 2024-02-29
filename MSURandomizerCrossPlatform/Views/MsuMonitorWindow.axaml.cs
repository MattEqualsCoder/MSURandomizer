using System;
using System.IO;
using Avalonia.Controls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using MSURandomizerCrossPlatform.Services;
using MSURandomizerCrossPlatform.ViewModels;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerCrossPlatform.Views;

public partial class MsuMonitorWindow : RestorableWindow
{
    private readonly MsuMonitorWindowService? _service;
    
    public MsuMonitorWindow() : base()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MsuMonitorWindowViewModel();
            return;
        }
        
        _service = IControlServiceFactory.GetControlService<MsuMonitorWindowService>();
        DataContext = _service.InitializeModel();
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "msu-monitor.json");
    protected override int DefaultWidth => 600;
    protected override int DefualtHeight => 400;

    public void Show(Msu? msu, Window? parent = null)
    {
        _service?.StartMonitor(msu);
        if (parent == null)
        {
            Show();
        }
        else
        {
            Show(parent);
        }
    }

    private void SnesConnectoTypeEnumComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _service?.ConnectToSnes();
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        _service?.StopMonitor();
    }
}