using System;
using System.IO;
using System.Linq;
using AutoMapper;
using AvaloniaControls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary;
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
    IRomLauncherService romLauncherService,
    ILogger<MsuMonitorWindowService> logger) : ControlService
{
    private readonly MsuMonitorWindowViewModel _model = new();

    public MsuMonitorWindowViewModel InitializeModel()
    {
        snesConnectorService.Connected += SnesConnectorServiceOnOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnOnDisconnected;
        msuMonitorService.MsuShuffled += MsuMonitorServiceOnMsuShuffled;
        mapper.Map(msuUserOptionsService.MsuUserOptions.SnesConnectorSettings, _model);
        _model.ConnectionStatus = snesConnectorService.IsConnected ? "Connected" : "Disconnected";
        return _model;
    }

    public string RestoreFilePath =>
        Path.Combine(msuAppSettingsService.MsuAppSettings.SaveDataDirectory.ExpandSpecialFolders(), "msu-monitor.json");

    public void StartMonitor(Msu? msu = null, MsuType? outputMsuType = null)
    {
        if (msu == null)
        {
            outputMsuType ??= msuTypeService.GetMsuType(msuUserOptionsService.MsuUserOptions.OutputMsuType);

            if (outputMsuType == null)
            {
                _model.ErrorMessage = "Invalid MSU Type Selected";
                return;
            }
            else if (!VerifyMsuTypeCompatibility(outputMsuType))
            {
                _model.ErrorMessage = "The selected MSU type is not compatible with reading the current\r\nplaying tracks, but the MSU will be re-shuffled every 60 seconds\r\nas long as you keep this window up.";
            }
            else
            {
                ConnectToSnes();
                _model.ShowConnectorDropdown = true;
            }
            
            ITaskService.Run(() =>
            {
                var msus = msuLookupService.Msus.Where(x =>
                    msuUserOptionsService.MsuUserOptions.SelectedMsus?.Contains(x.Path) == true).ToList();
                msuMonitorService.StartShuffle(new MsuSelectorRequest()
                {
                    EmptyFolder = false,
                    Msus = msus,
                    OutputMsuType = outputMsuType,
                    ShuffleStyle = msuUserOptionsService.MsuUserOptions.MsuShuffleStyle,
                    OpenFolder = false
                }, msuAppSettingsService.MsuAppSettings.ContinuousReshuffleSeconds ?? 60);

                if (msuAppSettingsService.MsuAppSettings.CanLaunchRoms &&
                    msuUserOptionsService.MsuUserOptions.LaunchRom &&
                    !string.IsNullOrEmpty(msuUserOptionsService.MsuUserOptions.OutputRomPath))
                {
                    romLauncherService.LaunchRom(msuUserOptionsService.MsuUserOptions.OutputRomPath);
                }
            });
        }
        else
        {
            if (msu.MsuType == null)
            {
                _model.ErrorMessage = "Invalid MSU Type Selected";
                return;
            }
            else if (!VerifyMsuTypeCompatibility(msu.MsuType))
            {
                _model.ErrorMessage =
                    "The selected MSU type is not compatible with reading the current\r\nplaying tracks.";
                return;
            }

            _model.ShowConnectorDropdown = true;
            
            outputMsuType ??= msu.MsuType;
            
            ConnectToSnes();
            
            ITaskService.Run(() =>
            {
                msuMonitorService.StartMonitor(msu, outputMsuType!);
            });
        }
    }

    public void ConnectToSnes()
    {
        if (_model.ConnectorType == _model.CurrentConnectorType || !_model.ShowConnectorDropdown)
        {
            return;
        }

        _model.CurrentConnectorType = _model.ConnectorType;
        
        var connectorSettings = mapper.Map<SnesConnectorSettings>(_model);

        if (connectorSettings.ConnectorType == SnesConnectorType.None)
        {
            snesConnectorService.Disconnect();
            return;
        }
        
        snesConnectorService.Connect(connectorSettings);
        
        snesConnectorService.UpdateTimeoutSeconds(connectorSettings.TimeoutSeconds);
        
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

    public void OpenLuaFolder()
    {
        CrossPlatformTools.OpenDirectory(msuAppSettingsService.MsuAppSettings.DefaultLuaDirectory);
    }

    private bool VerifyMsuTypeCompatibility(MsuType msuType) => msuGameService.IsMsuTypeCompatible(msuType);

    private void MsuMonitorServiceOnMsuShuffled(object? sender, EventArgs e)
    {
        _model.LastUpdateTime = DateTime.Now;
    }

    private void SnesConnectorServiceOnOnDisconnected(object? sender, EventArgs e)
    {
        _model.ConnectionStatus = "Disconnected";
    }

    private void SnesConnectorServiceOnOnConnected(object? sender, EventArgs e)
    {
        _model.ConnectionStatus = "Connected";
    }
}