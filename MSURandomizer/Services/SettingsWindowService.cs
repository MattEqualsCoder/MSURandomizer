using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class SettingsWindowService(
    IMsuUserOptionsService userOptionsService, 
    IMsuTypeService msuTypeService, 
    IMsuLookupService msuLookupService, 
    IMapper mapper) : ControlService
{
    private readonly SettingsWindowViewModel _model = new();
    
    public SettingsWindowViewModel InitializeModel()
    {
        mapper.Map(userOptionsService.MsuUserOptions, _model);
        _model.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        foreach (var msuType in msuTypeService.MsuTypes.OrderBy(x => x.DisplayName))
        {
            _model.MsuTypeNamePathsList.Add(new MsuTypePath()
            {
                MsuType = msuType,
                Path = _model.MsuTypeNamePaths.GetValueOrDefault(msuType.DisplayName, ""),
                DefaultDirectory = _model.DefaultDirectory
            });
        }
        return _model;
    }

    public void SaveModel()
    {
        var options = userOptionsService.MsuUserOptions;
        var hasPathUpdated = HasPathUpdated(options);
        mapper.Map(_model, options);
        options.MsuTypePaths = _model.MsuTypeNamePathsList
            .Where(x => !string.IsNullOrWhiteSpace(x.Path) && Directory.Exists(x.Path) && x.MsuType != null)
            .ToDictionary(x => x.MsuType!, x => x.Path);
        options.MsuTypeNamePaths = _model.MsuTypeNamePathsList
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
        if (_model.DefaultMsuPath != options.DefaultMsuPath)
        {
            return true;
        }
        
        foreach (var msuPath in _model.MsuTypeNamePathsList.Where(x => x.MsuType != null))
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