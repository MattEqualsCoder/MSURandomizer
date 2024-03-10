using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AvaloniaControls;
using AvaloniaControls.ControlServices;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizer.Services;

public class HardwareModeWindowService(
    ISnesConnectorService snesConnectorService,
    IMapper mapper,
    IMsuUserOptionsService msuUserOptionsService,
    IMsuHardwareService msuHardwareService,
    ILogger<HardwareModeWindowService> logger) : IControlService
{
    private HardwareModeWindowViewModel _model = new();

    public HardwareModeWindowViewModel InitializeModel()
    {
        mapper.Map(msuUserOptionsService.MsuUserOptions, _model);
        snesConnectorService.Connected += SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnDisconnected;
        return _model;
    }

    private void SnesConnectorServiceOnDisconnected(object? sender, EventArgs e)
    {
        _model.ConnectionStatus = "Disconnected";
        _model.CanAccept = false;
    }

    private void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        _model.CanAccept = false;
        
        if (!snesConnectorService.CurrentConnectorFunctionality.CanAccessFiles)
        {
            _model.ConnectionStatus = "Incompatible device";
            return;
        }

        _ = GetMsuList();
    }

    private async Task GetMsuList()
    {
        _model.ConnectionStatus = "Retrieving MSUs from device";
        
        try
        {
            _model.Msus = await msuHardwareService.GetMsusFromDevice();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get MSUs");
            _model.ConnectionStatus ="Unable to get MSU list from device. Please try again";
            return;
        }
        
        if (_model.Msus == null || _model.Msus.Count == 0)
        {
            _model.Msus = null;
            _model.ConnectionStatus = "Unable to retrieve MSUs from device";
            return;
        }

        _model.ConnectionStatus = $"{_model.Msus.Count} MSUs found";
        _model.CanAccept = true;
    }

    public void Disconnect()
    {
        snesConnectorService.Connected -= SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected -= SnesConnectorServiceOnDisconnected;
        snesConnectorService.Disconnect();
    }
    
    public void ConnectToSnes()
    {
        var connectorSettings = mapper.Map<SnesConnectorSettings>(_model.SnesConnectorSettings);

        if (connectorSettings.ConnectorType == SnesConnectorType.None)
        {
            snesConnectorService.Disconnect();
            return;
        }
        
        snesConnectorService.Connect(connectorSettings);

        if (msuUserOptionsService.MsuUserOptions.SnesConnectorSettings.ConnectorType != connectorSettings.ConnectorType)
        {
            msuUserOptionsService.MsuUserOptions.SnesConnectorSettings.ConnectorType = connectorSettings.ConnectorType;
            try
            {
                msuUserOptionsService.Save();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error saving connector type");
            }
        }
    }
}