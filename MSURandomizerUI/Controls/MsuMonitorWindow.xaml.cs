using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Window for displaying the current song and connecting to the emulator
/// </summary>
public partial class MsuMonitorWindow : Window
{
    private readonly IMsuMonitorService? _msuMonitorService;
    private readonly ISnesConnectorService? _snesConnectorService;
    private readonly IMsuUserOptionsService? _msuUserOptionsService;
    private readonly IMsuGameService? _msuGameService;
    private readonly MsuCurrentPlayingTrackControl _mainControl;
    private IMsuAppSettingsService? _msuAppSettingsService;
    private MsuType? _msuType;
    private bool _controlAdded;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msuMonitorService"></param>
    /// <param name="snesConnectorService"></param>
    /// <param name="msuUserOptionsService"></param>
    /// <param name="mainControl"></param>
    /// <param name="msuAppSettingsService"></param>
    /// <param name="msuGameService"></param>
    public MsuMonitorWindow(IMsuMonitorService? msuMonitorService, ISnesConnectorService? snesConnectorService, IMsuUserOptionsService? msuUserOptionsService, MsuCurrentPlayingTrackControl mainControl, IMsuAppSettingsService? msuAppSettingsService, IMsuGameService? msuGameService)
    {
        _msuMonitorService = msuMonitorService;
        _snesConnectorService = snesConnectorService;
        _msuUserOptionsService = msuUserOptionsService;
        _msuAppSettingsService = msuAppSettingsService;
        _msuGameService = msuGameService;
        _mainControl = mainControl;

        InitializeComponent();

        if (_snesConnectorService == null || _msuMonitorService == null)
        {
            return;
        }
        
        _snesConnectorService.OnConnected += SnesConnectorServiceOnOnConnected;
        _snesConnectorService.OnDisconnected += SnesConnectorServiceOnOnDisconnected;
        _msuMonitorService.MsuShuffled += MsuMonitorServiceOnMsuShuffled;

        SnesConnectorTypeComboBox.SelectedItem =
            msuUserOptionsService?.MsuUserOptions.SnesConnectorSettings.ConnectorType;
        LastUpdatedTextBlock.Text = $"Last updated {DateTime.Now.ToShortTimeString()}";
    }

    private void MsuMonitorServiceOnMsuShuffled(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            LastUpdatedTextBlock.Text = $"Last updated {DateTime.Now.ToShortTimeString()}";
        });
    }

    private void SnesConnectorServiceOnOnDisconnected(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            ConnectionStatusTextBlock.Text = "Disconnected";
        });
    }

    private void SnesConnectorServiceOnOnConnected(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            ConnectionStatusTextBlock.Text = "Connected";
        });
    }

    /// <summary>
    /// Shows the MSU Monitor window and starts continuous shuffling
    /// </summary>
    /// <param name="request">The request to use for continuous shuffling</param>
    public void Show(MsuSelectorRequest request)
    {
        _msuType = request.OutputMsuType;
        Connect();
        
        _ = Task.Run(() =>
        {
            _msuMonitorService?.StartShuffle(request, _msuAppSettingsService?.MsuAppSettings?.ContinuousReshuffleSeconds ?? 60);
        });
        
        
        Show();
    }
    
    /// <summary>
    /// Shows the MSU Monitor window and looks for a the current playing song 
    /// </summary>
    /// <param name="msu">The MSU to look for songs</param>
    public void Show(Msu msu)
    {
        _msuType = msu.MsuType;
        Connect();
        
        _ = Task.Run(() =>
        {
            _msuMonitorService?.StartMonitor(msu);
        });
        
        Show();
    }

    private void Connect()
    {
        if (_msuUserOptionsService?.MsuUserOptions == null || _snesConnectorService == null || _snesConnectorService.IsConnected)
        {
            return;
        }

        if (_msuType == null || _msuGameService?.IsMsuTypeCompatible(_msuType) != true)
        {
            WarningTextBlock.Visibility = Visibility.Visible;
            return;
        }

        if (!_controlAdded)
        {
            MainDockPanel.Children.Add(_mainControl);
            _controlAdded = true;
        }
        
        _snesConnectorService.Connect(_msuUserOptionsService.MsuUserOptions.SnesConnectorSettings);
    }

    private void SnesConnectorTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SnesConnectorTypeComboBox.SelectedItem is not SnesConnectorType connectorType || _msuUserOptionsService == null || _msuUserOptionsService.MsuUserOptions.SnesConnectorSettings.ConnectorType == connectorType) return;
        _msuUserOptionsService.MsuUserOptions.SnesConnectorSettings.ConnectorType = connectorType;
        
        if (_msuType != null && _msuGameService?.IsMsuTypeCompatible(_msuType) == true)
        {
            Connect();
        }
        
        _msuUserOptionsService.Save();
    }

    private void MsuMonitorWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        _msuMonitorService?.Stop();
        
        if (_snesConnectorService != null)
        {
            _snesConnectorService.OnConnected -= SnesConnectorServiceOnOnConnected;
            _snesConnectorService.OnDisconnected -= SnesConnectorServiceOnOnDisconnected;    
        }

        if (_msuMonitorService != null)
        {
            _msuMonitorService.MsuShuffled -= MsuMonitorServiceOnMsuShuffled;    
        }
    }
}