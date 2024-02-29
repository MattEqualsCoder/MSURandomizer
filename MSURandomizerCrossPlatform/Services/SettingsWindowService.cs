using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using MSURandomizerCrossPlatform.ViewModels;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerCrossPlatform.Services;

public class SettingsWindowService(
    IMsuUserOptionsService userOptionsService, 
    IMsuTypeService msuTypeService, 
    IMsuLookupService msuLookupService, 
    IMapper mapper) : IControlService
{
    public SettingsWindowViewModel Model = new();
    
    public SettingsWindowViewModel InitializeModel()
    {
        mapper.Map(userOptionsService.MsuUserOptions, Model);
        Model.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        foreach (var msuType in msuTypeService.MsuTypes.OrderBy(x => x.DisplayName))
        {
            Model.MsuTypeNamePathsList.Add(new MsuTypePath()
            {
                MsuType = msuType,
                Path = Model.MsuTypeNamePaths.GetValueOrDefault(msuType.DisplayName, ""),
                DefaultDirectory = Model.DefaultDirectory
            });
        }
        return Model;
    }

    public void SaveModel()
    {
        var options = userOptionsService.MsuUserOptions;
        var hasPathUpdated = HasPathUpdated(options);
        mapper.Map(Model, options);
        options.MsuTypePaths = Model.MsuTypeNamePathsList
            .Where(x => !string.IsNullOrWhiteSpace(x.Path) && Directory.Exists(x.Path) && x.MsuType != null)
            .ToDictionary(x => x.MsuType!, x => x.Path);
        options.MsuTypeNamePaths = Model.MsuTypeNamePathsList
            .Where(x => !string.IsNullOrWhiteSpace(x.Path) && Directory.Exists(x.Path) && x.MsuType != null)
            .ToDictionary(x => x.MsuType!.DisplayName, x => x.Path);
        userOptionsService.Save();
        ScalableWindow.GlobalScaleFactor = options.UiScaling;

        if (hasPathUpdated)
        {
            ITaskService.Run(() =>
            {
                msuLookupService.LookupMsus();
            });
        }
    }

    private bool HasPathUpdated(MsuUserOptions options)
    {
        if (Model.DefaultMsuPath != options.DefaultMsuPath)
        {
            return true;
        }
        
        foreach (var msuPath in Model.MsuTypeNamePathsList.Where(x => x.MsuType != null))
        {
            var newPath = msuPath.Path.Trim();
            var oldPath = options.MsuTypePaths.GetValueOrDefault(msuPath.MsuType!, "").Trim();
            if (newPath != oldPath)
            {
                return true;
            }
        }

        return false;
    }
}