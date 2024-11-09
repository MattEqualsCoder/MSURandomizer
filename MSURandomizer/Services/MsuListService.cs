using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvaloniaControls;
using AvaloniaControls.ControlServices;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class MsuListService(AppInitializationService appInitializationService,
    IMsuLookupService msuLookupService,
    IMsuUserOptionsService userOptions,
    IMsuTypeService msuTypeService,
    IMsuAppSettingsService appSettingsService,
    IMsuMonitorService msuMonitorService,
    IMsuHardwareService msuHardwareService) : ControlService
{
    public MsuListViewModel Model { get; set; } = new()
    {
        IsLoading = true
    };

    public event EventHandler? OnDisplayUnknownMsuWindowRequest;
    
    public MsuListViewModel InitializeModel()
    {
        msuMonitorService.MsuMonitorStarted += (_, _) =>
        {
            Model.IsMsuMonitorActive = true;
        };
        
        msuMonitorService.MsuMonitorStopped += (_, _) =>
        {
            Model.IsMsuMonitorActive = false;
        };
        
        appInitializationService.InitializationComplete += InitializationServiceOnInitializationComplete;
        msuLookupService.OnMsuLookupComplete += MsuLookupServiceOnOnMsuLookupComplete;
        msuLookupService.OnMsuLookupStarted += MsuLookupServiceOnOnMsuLookupStarted;
        msuHardwareService.HardwareMsusChanged += MsuHardwareServiceOnHardwareMsusChanged;
        Model.IsMsuMonitorDisabled = appSettingsService.MsuAppSettings.DisableMsuMonitorWindow == true;
        CheckIfLoading();
        return Model;
    }

    private void MsuHardwareServiceOnHardwareMsusChanged(object? sender, MsuListEventArgs e)
    {
        PopulateMsuViewModels(e.Msus.ToList());
    }

    private void MsuLookupServiceOnOnMsuLookupStarted(object? sender, EventArgs e)
    {
        CheckIfLoading();
    }

    public void FilterMSUs(string msuTypeName, MsuFilter msuFilter)
    {
        var msuType = msuTypeService.GetMsuType(msuTypeName);
        if (msuType == null)
        {
            return;
        }

        FilterMSUs(msuType, msuFilter);
    }
    
    public void FilterMSUs(MsuType msuType, MsuFilter msuFilter)
    {
        Model.MsuTypeName = msuType.DisplayName;
        Model.MsuType = msuType;
        
        // Hardware MSUs are more limited in compatibility
        List<string>? compatibleMsuNames = null;
        if (Model.HardwareMode)
        {
            if (appSettingsService.MsuAppSettings.HardwareCompatibleMsuTypes.TryGetValue(msuType.DisplayName,
                    out compatibleMsuNames))
            {
                compatibleMsuNames.Add(msuType.DisplayName);
            }
            else
            {
                compatibleMsuNames = [msuType.DisplayName];
            }
        }
        
        var filteredMsus = Model.MsuViewModels
            .Where(x => x.Msu.MatchesFilter(msuFilter, msuType, compatibleMsuNames) &&
                        (x.Msu.NumUniqueTracks > x.Msu.MsuType?.RequiredTrackNumbers.Count / 5 || x.Msu.NumUniqueTracks > 10))
            .OrderBy(x => x.MsuName)
            .ToList();
        Model.FilteredMsus = filteredMsus;
        Model.SelectedMsus = Model.FilteredMsus
            .Where(x => Model.SelectedMsus.Select(vm => vm.MsuPath).Contains(x.MsuPath)).ToList();
    }

    public void SelectAll()
    {
        Model.SelectedMsus = Model.FilteredMsus.ToList();
    }

    public void SelectNone()
    {
        Model.SelectedMsus = new List<MsuViewModel>();
    }

    public void ToggleFavorite(MsuViewModel model)
    {
        model.IsFavorite = !model.IsFavorite;
        model.Msu.Settings.IsFavorite = model.IsFavorite;
        userOptions.SaveMsuSettings(model.Msu);
    }
    
    public void UpdateFrequency(MsuViewModel model, ShuffleFrequency frequency)
    {
        model.ShuffleFrequency = frequency;
        model.Msu.Settings.ShuffleFrequency = model.ShuffleFrequency;
        userOptions.SaveMsuSettings(model.Msu);
    }

    public void OpenMsuFolder(MsuViewModel model)
    {
        CrossPlatformTools.OpenDirectory(model.MsuPath, true);
    }

    private void MsuLookupServiceOnOnMsuLookupComplete(object? sender, MsuListEventArgs e)
    {
        CheckIfLoading();
    }

    private void InitializationServiceOnInitializationComplete(object? sender, EventArgs e)
    {
        CheckIfLoading();
    }

    private bool CheckIfLoading()
    {
        var prevValue = Model.IsLoading;
        Model.IsLoading = appInitializationService.IsLoading ||
                          msuLookupService.Status != MsuLoadStatus.Loaded;
        
        if (!Model.IsLoading && prevValue)
        {
            PopulateMsuViewModels(msuLookupService.Msus.ToList());
        }
        return Model.IsLoading;
    }

    public void ToggleHardwareMode(bool isEnabled)
    {
        Model.HardwareMode = isEnabled;
    }
    
    public void PopulateMsuViewModels(List<Msu>? msus)
    {
        msus = msus?.Count > 0 ? msus : msuLookupService.Msus.ToList();
        
        Model.Msus = msus.ToList();
        
        Model.MsuViewModels = msus.Select(x => new MsuViewModel(x)).ToList();

        var filterMsuType = Model.MsuTypeName
                            ?? userOptions.MsuUserOptions.OutputMsuType
                            ?? msuTypeService.MsuTypes.OrderBy(x => x.DisplayName).First().DisplayName;
        
        FilterMSUs(filterMsuType, userOptions.MsuUserOptions.Filter);
        Model.SelectedMsus = Model.FilteredMsus
            .Where(x => userOptions.MsuUserOptions.SelectedMsus?.Contains(x.MsuPath) == true).ToList();

        Model.DisplayUnknownMsuWindow =
            Model.Msus.Any(x => x.MsuType == null && x is { NumUniqueTracks: > 15, IgnoreUnknown: false } && string.IsNullOrEmpty(x.Settings.MsuTypeName) );

        if (Model.DisplayUnknownMsuWindow)
        {
            OnDisplayUnknownMsuWindowRequest?.Invoke(this, EventArgs.Empty);    
        }
    }
}