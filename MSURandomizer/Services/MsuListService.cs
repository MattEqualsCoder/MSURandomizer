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
    IMsuMonitorService msuMonitorService) : ControlService
{
    public MsuListViewModel Model { get; set; } = new()
    {
        IsLoading = true
    };
    
    public MsuListViewModel InitializeModel()
    {
        msuMonitorService.MsuMonitorStarted += (sender, args) =>
        {
            Model.IsMsuMonitorActive = true;
        };
        
        msuMonitorService.MsuMonitorStopped += (sender, args) =>
        {
            Model.IsMsuMonitorActive = false;
        };
        
        appInitializationService.InitializationComplete += InitializationServiceOnInitializationComplete;
        msuLookupService.OnMsuLookupComplete += MsuLookupServiceOnOnMsuLookupComplete;
        msuLookupService.OnMsuLookupStarted += MsuLookupServiceOnOnMsuLookupStarted;
        Model.IsMsuMonitorDisabled = appSettingsService.MsuAppSettings.DisableMsuMonitorWindow == true;
        CheckIfLoading();
        return Model;
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
        var msuTypePath = Model.HardwareMode ? "" :
            userOptions.MsuUserOptions.MsuTypePaths.TryGetValue(msuType, out var path) ? path :
            userOptions.MsuUserOptions.DefaultMsuPath;
        var rootPath = Model.HardwareMode ? "" : GetMsuTypeBasePath(msuType);
        var useAbsolutePath = string.IsNullOrWhiteSpace(rootPath);
        var filteredMsus = Model.MsuViewModels
            .Where(x => x.Msu.MatchesFilter(msuFilter, msuType, msuTypePath) &&
                        x.Msu.NumUniqueTracks > x.Msu.MsuType?.RequiredTrackNumbers.Count / 5)
            .OrderBy(x => x.MsuName)
            .ToList();
        foreach (var filteredMsu in filteredMsus)
        {
            filteredMsu.DisplayPath = useAbsolutePath ? filteredMsu.MsuPath : Path.GetRelativePath(rootPath!, filteredMsu.MsuPath);
        }

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
    }

    private string? GetMsuTypeBasePath(MsuType? msuType)
    {
        if (msuType == null)
        {
            return userOptions.MsuUserOptions.DefaultMsuPath;
        }
        
        if (userOptions.MsuUserOptions.MsuTypeNamePaths.TryGetValue(msuType.DisplayName, out string? path))
        {
            return path;
        }

        return userOptions.MsuUserOptions.DefaultMsuPath;
    }
}