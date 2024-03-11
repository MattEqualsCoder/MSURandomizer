using System;
using System.IO;
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

public partial class MsuMonitorWindow : RestorableWindow
{
    private readonly MsuMonitorWindowService? _service;
    
    public MsuMonitorWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MsuMonitorWindowViewModel();
            return;
        }
        
        _service = this.GetControlService<MsuMonitorWindowService>();
        DataContext = _service!.InitializeModel();
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

    private void FolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenLuaFolder();
    }
}