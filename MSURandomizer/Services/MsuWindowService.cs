using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizer.Views;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class MsuWindowService(ILogger<MsuWindowService> logger, 
    AppInitializationService appInitializationService, 
    IMsuTypeService msuTypeService,
    IMsuUserOptionsService userOptions,
    IMsuAppSettingsService appSettings,
    IMsuSelectorService msuSelectorService,
    IMsuLookupService msuLookupService,
    IMsuMonitorService msuMonitorService,
    IRomLauncherService romLauncherService) : IControlService
{
    public MsuWindowViewModel Model { get; set; } = new();

    public event EventHandler? MsuMonitorStarted;

    public event EventHandler? MsuMonitorStopped;

    public MsuWindowViewModel InitializeModel()
    {
        msuLookupService.OnMsuLookupStarted += (sender, args) =>
        {
            Model.AreMsusLoading = true;
        };
        
        msuLookupService.OnMsuLookupComplete += (sender, args) =>
        {
            Model.AreMsusLoading = false;
        };

        msuMonitorService.MsuMonitorStarted += (sender, args) =>
        {
            Model.IsMsuMonitorActive = true;
            MsuMonitorStarted?.Invoke(this, EventArgs.Empty);
        };
        
        msuMonitorService.MsuMonitorStopped += (sender, args) =>
        {
            Model.IsMsuMonitorActive = false;
            MsuMonitorStopped?.Invoke(this, EventArgs.Empty);
        };
        
        appInitializationService.InitializationComplete += AppInitializationServiceOnInitializationComplete;
            
        var settings = appSettings.MsuAppSettings;
        Model.CanDisplayRandomMsuButton = settings.MsuWindowDisplayRandomButton == true;
        Model.CanDisplayShuffledMsuButton = settings.MsuWindowDisplayShuffleButton == true;
        Model.CanDisplayContinuousShuffleButton = settings.MsuWindowDisplayContinuousButton == true;
        Model.CanDisplayCancelButton = settings.MsuWindowDisplaySelectButton == true;
        Model.HasMsuFolder = userOptions.MsuUserOptions.HasMsuFolder();
        Model.IsHardwareModeButtonVisible = !appSettings.MsuAppSettings.DisableHardwareMode;
        return Model;
    }

    private void AppInitializationServiceOnInitializationComplete(object? sender, EventArgs e)
    {
        if (appInitializationService.LatestFullRelease == null)
        {
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Title = $"MSU Randomizer {appInitializationService.LatestFullRelease.Tag}",
                Message = "A new version of the MSU Randomizer has been released.",
                LinkText = "Go to the release page on GitHub",
                LinkUrl = appInitializationService.LatestFullRelease.Url,
                Icon = MessageWindowIcon.Info,
                CheckBoxText = "Do not check for updates",
                Buttons = MessageWindowButtons.OK
            });
            messageWindow.Closed += (o, args) =>
            {
                if (messageWindow.DialogResult?.CheckedBox == true)
                {
                    userOptions.MsuUserOptions.PromptOnUpdate = false;
                    userOptions.Save();    
                }
            };
            messageWindow.ShowDialog();
        });
        
    }

    public void OnSelectedMsusChanged(ICollection<MsuViewModel>? msus)
    {
        Model.MsuCount = msus?.Count ?? 0;
    }

    public void FinishInitialization()
    {
        msuTypeService.OnMsuTypeLoadComplete += MsuTypeServiceOnOnMsuTypeLoadComplete;
        logger.LogInformation("Finish Init");
        appInitializationService.FinishInitialization();
    }

    public void FilterMsuList(MsuList msuList)
    {
        var msuType = msuTypeService.GetMsuType(Model.SelectedMsuType);
        if (msuType == null)
        {
            logger.LogWarning("Invalid MSU type");
            return;
        }
        msuList.FilterMSUs(msuType, Model.Filter);
    }

    public void UpdateSelectedMsus(ICollection<MsuViewModel>? msus)
    {
        Model.MsuCount = msus?.Count ?? 0;
        Model.SelectedMsus = msus ?? new List<MsuViewModel>();
    }

    public void UpdateHardwareMode(MsuList msuList, List<Msu>? msus)
    {
        Model.IsHardwareModeEnabled = msus?.Count > 0;
        msuList.ToggleHardwareMode(Model.IsHardwareModeEnabled);
        msuList.PopulateMsuViewModels(msus);
    }

    public bool GenerateMsu(out string error, out bool openContinuousWindow, out Msu? msu)
    {
        error = "";
        msu = null;

        var outputMsuType = msuTypeService.GetMsuType(Model.SelectedMsuType);

        if (outputMsuType == null)
        {
            error = "Invalid MSU Type";
            openContinuousWindow = false;
            return false;
        }

        var msus = Model.SelectedMsus.Select(x => x.Msu).ToList();
        if (msus.Count == 0)
        {
            error = "No MSUs selected";
            openContinuousWindow = false;
            return false;
        }
        
        var options = userOptions.MsuUserOptions;
        if (options.RandomizationStyle == MsuRandomizationStyle.Single)
        {
            if (options.SelectedMsus?.Count == 1)
            {
                var response = msuSelectorService.AssignMsu(new MsuSelectorRequest()
                {
                    EmptyFolder = true,
                    Msu = msus.First(),
                    OutputMsuType = msuTypeService.GetMsuType(Model.SelectedMsuType)
                });

                if (!response.Successful)
                {
                    error = response.Message ?? "There was an error generating the MSU";
                    logger.LogError(error);
                    openContinuousWindow = false;
                    return false;
                }
                else
                {
                    msu = response.Msu;
                    openContinuousWindow = options.OpenMonitorWindow;

                    if (options.LaunchRom && !string.IsNullOrEmpty(options.OutputRomPath))
                    {
                        romLauncherService.LaunchRom(options.OutputRomPath);
                    }
                    return true;
                }
            }
            else
            {
                var response = msuSelectorService.PickRandomMsu(new MsuSelectorRequest()
                {
                    EmptyFolder = true,
                    Msus = msus,
                    OutputMsuType = outputMsuType
                });
                
                if (!response.Successful)
                {
                    error = response.Message ?? "There was an error generating the MSU";
                    logger.LogError(error);
                    openContinuousWindow = false;
                    return false;
                }
                else
                {
                    msu = response.Msu;
                    openContinuousWindow = options.OpenMonitorWindow;
                    
                    if (options.LaunchRom && !string.IsNullOrEmpty(options.OutputRomPath))
                    {
                        romLauncherService.LaunchRom(options.OutputRomPath);
                    }
                    
                    return true;
                }
            }
        }
        else if (options.RandomizationStyle == MsuRandomizationStyle.Shuffled)
        {
            var response = msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                EmptyFolder = true,
                Msus = msus,
                OutputMsuType = outputMsuType,
                ShuffleStyle = options.MsuShuffleStyle
            });
            
            if (!response.Successful)
            {
                error = response.Message ?? "There was an error generating the MSU";
                logger.LogError(error);
                openContinuousWindow = false;
                return false;
            }
            else
            {
                msu = response.Msu;
                openContinuousWindow = options.OpenMonitorWindow;
                
                if (options.LaunchRom && !string.IsNullOrEmpty(options.OutputRomPath))
                {
                    romLauncherService.LaunchRom(options.OutputRomPath);
                }
                
                return true;
            }
        }

        error = "Invalid randomization style";
        openContinuousWindow = false;
        logger.LogError(error);
        return false;
    }

    private void MsuTypeServiceOnOnMsuTypeLoadComplete(object? sender, EventArgs e)
    {
        Model.MsusTypes = msuTypeService.MsuTypes.Select(x => x.DisplayName).Order().ToList();
        Model.SelectedMsuType = userOptions.MsuUserOptions.OutputMsuType ?? Model.MsusTypes.First();
    }
}