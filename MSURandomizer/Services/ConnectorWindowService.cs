using System;
using AutoMapper;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizer.Services;

public class ConnectorWindowService(ISnesConnectorService snesConnectorService, IMsuUserOptionsService msuUserOptionsService, ILogger<HardwareDirectoriesWindowService> logger, IMapper mapper) : ControlService
{
    private ConnectorWindowViewModel _model = new();
    
    public ConnectorWindowViewModel InitializeModel()
    {
        mapper.Map(msuUserOptionsService.MsuUserOptions, _model);
        snesConnectorService.Connected += SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnDisconnected;
        return _model;
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

    public void OnClose()
    {
        snesConnectorService.Connected -= SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected -= SnesConnectorServiceOnDisconnected;
    }

    private void SnesConnectorServiceOnDisconnected(object? sender, EventArgs e)
    {
        _model.ConnectionStatus = "Disconnected";
        _model.CanAccept = false;
    }

    private void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        if (!snesConnectorService.CurrentConnectorFunctionality.CanAccessFiles)
        {
            _model.ConnectionStatus = "Incompatible device";
            _model.CanAccept = false;
        }
        else
        {
            _model.ConnectionStatus = "Connected";
            _model.CanAccept = true;
        }
    }
}