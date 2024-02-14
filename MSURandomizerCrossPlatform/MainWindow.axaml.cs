using System.Linq;
using Avalonia.Controls;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizerCrossPlatform;

public partial class MainWindow : Window
{
    private readonly ISnesConnectorService? _snesConnectorService;
    private readonly IMsuMonitorService? _msuMonitorService;
    private readonly MainWindowViewModel? _model;

    public MainWindow() : this(null, null, null, null)
    {
        
    }

    public MainWindow(ISnesConnectorService? snesConnectorService = null, IMsuMonitorService? msuMonitorService = null, IMsuLookupService? msuLookupService = null, IMsuTypeService? msuTypeService = null)
    {
        _snesConnectorService = snesConnectorService;
        _msuMonitorService = msuMonitorService;
        DataContext = _model = new MainWindowViewModel();
        InitializeComponent();

        if (_msuMonitorService == null || msuLookupService == null || msuTypeService == null)
        {
            return;
        }
        
        _msuMonitorService.MsuTrackChanged += MsuMonitorServiceOnMsuTrackChanged;

        _msuMonitorService.StartShuffle(new MsuSelectorRequest()
        {
            Msus = msuLookupService.Msus.ToList(),
            OutputMsuType = msuTypeService.GetSMZ3MsuType(),
            OutputPath = "/home/matt/Games/SMZ3/Roms/SMZ3_Cas_20240205-141710_193353533/SMZ3_Cas_20240205-141710_193353533.msu",
            ShuffleStyle = MsuShuffleStyle.ShuffleWithPairedTracks
        });

        if (snesConnectorService == null)
        {
            return;
        }
        
        snesConnectorService.OnConnected += (sender, args) =>
        {
            _model.IsConnected = true;
            _model.IsConnectorConnecting = false;
            _model.IsDisconnected = false;
            _model.CurrentSong = "N/A";
            _model.CurrentTrack = "N/A";
        };
    }

    private void MsuMonitorServiceOnMsuTrackChanged(object sender, MsuTrackChangedEventArgs e)
    {
        if (_model == null)
        {
            return;
        }
        
        _model.CurrentTrack = $"Track #{e.Track.Number} - {e.Track.TrackName}";
        _model.CurrentSong = e.Track.GetDisplayText();
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_model == null)
        {
            return;
        }
        var selectedItem = (sender as ComboBox)!.SelectedItem as string;
        if (string.IsNullOrEmpty(selectedItem) || !_model.ConnectorMap.TryGetValue(selectedItem, out var selectedConnectorType))
        {
            _snesConnectorService?.Disconnect();
            _model.IsConnected = false;
            _model.IsConnectorConnecting = false;
            _model.IsDisconnected = true;
            _model.CurrentSong = "N/A";
            _model.CurrentTrack = "N/A";
        }
        else
        {
            _snesConnectorService?.Connect(selectedConnectorType);
            _model.IsConnected = false;
            _model.IsConnectorConnecting = true;
            _model.IsDisconnected = false;
        }
    }
}