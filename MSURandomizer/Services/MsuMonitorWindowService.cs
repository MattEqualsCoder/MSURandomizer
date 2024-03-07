using System;
using System.Linq;
using AutoMapper;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizer.Services;

public class MsuMonitorWindowService(
    IMsuMonitorService msuMonitorService,
    ISnesConnectorService snesConnectorService,
    IMsuUserOptionsService msuUserOptionsService,
    IMsuGameService msuGameService,
    IMsuAppSettingsService msuAppSettingsService,
    IMsuTypeService msuTypeService,
    IMsuLookupService msuLookupService,
    IMapper mapper,
    ILogger<MsuMonitorWindowService> logger) : IControlService
{
    public MsuMonitorWindowViewModel Model { get; set; } = new();

    public MsuMonitorWindowViewModel InitializeModel()
    {
        snesConnectorService.Connected += SnesConnectorServiceOnOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnOnDisconnected;
        msuMonitorService.MsuShuffled += MsuMonitorServiceOnMsuShuffled;
        mapper.Map(msuUserOptionsService.MsuUserOptions.SnesConnectorSettings, Model);
        return Model;
    }

    public void StartMonitor(Msu? msu = null)
    {
        if (msu == null)
        {
            var outputMsuType = msuTypeService.GetMsuType(msuUserOptionsService.MsuUserOptions.OutputMsuType);

            if (outputMsuType == null)
            {
                Model.ErrorMessage = "Invalid MSU Type Selected";
                return;
            }
            else if (!VerifyMsuTypeCompatibility(outputMsuType))
            {
                Model.ErrorMessage = "Sorry, that game is not compatible yet with reading the current playing track";
                return;
            }
            
            ConnectToSnes();
            
            ITaskService.Run(() =>
            {
                var msus = msuLookupService.Msus.Where(x =>
                    msuUserOptionsService.MsuUserOptions.SelectedMsus?.Contains(x.Path) == true).ToList();
                msuMonitorService.StartShuffle(new MsuSelectorRequest()
                {
                    EmptyFolder = true,
                    Msus = msus,
                    OutputMsuType = outputMsuType,
                    ShuffleStyle = msuUserOptionsService.MsuUserOptions.MsuShuffleStyle,
                    OpenFolder = false
                }, msuAppSettingsService.MsuAppSettings.ContinuousReshuffleSeconds ?? 60);
            });
        }
        else
        {
            if (msu.MsuType == null)
            {
                Model.ErrorMessage = "Invalid MSU Type Selected";
                return;
            }
            else if (!VerifyMsuTypeCompatibility(msu.MsuType))
            {
                Model.ErrorMessage = "Sorry, that game is not compatible yet with reading the current playing track";
                return;
            }
            
            ConnectToSnes();
            
            ITaskService.Run(() =>
            {
                msuMonitorService.StartMonitor(msu);
            });
        }
    }

    public void ConnectToSnes()
    {
        var connectorSettings = mapper.Map<SnesConnectorSettings>(Model);

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

    public void StopMonitor()
    {
        msuMonitorService.Stop();
        snesConnectorService.Connected -= SnesConnectorServiceOnOnConnected;
        snesConnectorService.Disconnected -= SnesConnectorServiceOnOnDisconnected;
        msuMonitorService.MsuShuffled -= MsuMonitorServiceOnMsuShuffled;
    }

    private bool VerifyMsuTypeCompatibility(MsuType msuType) => msuGameService.IsMsuTypeCompatible(msuType);

    private void MsuMonitorServiceOnMsuShuffled(object? sender, EventArgs e)
    {
        Model.LastUpdateTime = DateTime.Now;
    }

    private void SnesConnectorServiceOnOnDisconnected(object? sender, EventArgs e)
    {
        Model.ConnectionStatus = "Disconnected";
    }

    private void SnesConnectorServiceOnOnConnected(object? sender, EventArgs e)
    {
        Model.ConnectionStatus = "Connected";
    }
}