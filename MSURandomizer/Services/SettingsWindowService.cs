using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class SettingsWindowService(
    IMsuUserOptionsService userOptionsService, 
    IMsuTypeService msuTypeService, 
    IMsuLookupService msuLookupService, 
    IMsuCacheService msuCacheService,
    IMapper mapper) : ControlService
{
    private readonly SettingsWindowViewModel _model = new();
    private Dictionary<string, MsuType> _msuTypes = [];
    private List<string> _msuTypeList = [];
    
    public SettingsWindowViewModel InitializeModel()
    {
        mapper.Map(userOptionsService.MsuUserOptions, _model);
        _model.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _msuTypes = msuTypeService.MsuTypes.OrderBy(x => x.DisplayName).ToDictionary(x => x.DisplayName, x => x);
        _msuTypeList = _msuTypes.Keys.ToList();
        _model.MsuDirectoryList = new ObservableCollection<MsuDirectory>(
            userOptionsService.MsuUserOptions.MsuDirectories.Select(x =>
                new MsuDirectory(x.Key, x.Value, _msuTypeList)));
        _model.MsuHardwareDirectoryList = new ObservableCollection<MsuDirectory>(
            userOptionsService.MsuUserOptions.HardwareMsuDirectories.Select(x =>
                new MsuDirectory(x.Key, x.Value, _msuTypeList)));
        _model.DisplayNoMsuDirectoriesMessage = _model.MsuDirectoryList.Count == 0;
        return _model;
    }

    public void SaveModel()
    {
        var options = userOptionsService.MsuUserOptions;
        var hasPathUpdated = HasPathUpdated(options);
        mapper.Map(_model, options);
        options.MsuDirectories = _model.MsuDirectoryList.ToDictionary(x => x.Path, x => x.MsuTypeName);
        options.HardwareMsuDirectories = _model.MsuHardwareDirectoryList.ToDictionary(x => x.Path, x => x.MsuTypeName);
        userOptionsService.Save();
        ScalableWindow.GlobalScaleFactor = options.UiScaling;

        if (hasPathUpdated)
        {
            ITaskService.Run(() =>
            {
                msuCacheService.Clear(false);
                msuLookupService.LookupMsus();
            });
        }
    }

    public bool AddDirectory(string directory)
    {
        if (_model.MsuDirectoryList.Any(x => x.Path == directory))
        {
            return false;
        }
        _model.MsuDirectoryList.Add(new MsuDirectory(directory, _msuTypeList.FirstOrDefault() ?? "", _msuTypeList));
        _model.DisplayNoMsuDirectoriesMessage = false;
        return true;
    }
    
    public bool AddHardwareDirectory(string directory)
    {
        if (_model.MsuHardwareDirectoryList.Any(x => x.Path == directory))
        {
            return false;
        }
        _model.MsuHardwareDirectoryList.Add(new MsuDirectory(directory, _msuTypeList.FirstOrDefault() ?? "", _msuTypeList));
        return true;
    }

    public void RemoveDirectory(string directory)
    {
        var directoryToRemove = _model.MsuDirectoryList.FirstOrDefault(x => x.Path == directory);
        if (directoryToRemove != null)
        {
            _model.MsuDirectoryList.Remove(directoryToRemove);
            _model.DisplayNoMsuDirectoriesMessage = _model.MsuDirectoryList.Count == 0;
        }
    }

    public void RemoveHardwareDirectory(string directory)
    {
        var directoryToRemove = _model.MsuHardwareDirectoryList.FirstOrDefault(x => x.Path == directory);
        if (directoryToRemove != null)
        {
            _model.MsuHardwareDirectoryList.Remove(directoryToRemove);
        }
    }

    private bool HasPathUpdated(MsuUserOptions options)
    {
        if (_model.MsuDirectoryList.Count != options.MsuDirectories.Count)
        {
            return true;
        }

        var currentDirectories = options.MsuDirectories.Select(x => $"{x.Key}={x.Value}");
        var newDirectories = _model.MsuDirectoryList.Select(x => $"{x.Path}={x.MsuTypeName}");
        return !currentDirectories.SequenceEqual(newDirectories);
    }
}