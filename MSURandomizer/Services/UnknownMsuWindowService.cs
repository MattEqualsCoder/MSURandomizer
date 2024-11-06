using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class UnknownMsuWindowService(
    IMsuUserOptionsService userOptionsService,
    IMapper mapper,
    IMsuTypeService msuTypeService,
    IMsuCacheService msuCacheService,
    IMsuLookupService msuLookupService,
    IMsuHardwareService msuHardwareService) : ControlService
{
    private UnknownMsuWindowViewModel _model = new();
    private const int MaxPathLength = 50;
    private bool _isHardwareMode;
    
    public UnknownMsuWindowViewModel InitilizeModel(bool isHardwareMode)
    {
        _isHardwareMode = isHardwareMode;
        
        List<string> msuTypes = [""];
        msuTypes.AddRange(msuTypeService.MsuTypes.Select(x => x.DisplayName).OrderBy(x => x));

        List<MsuDetailsWindowViewModel> msuModels = [];

        var msuList = isHardwareMode ? msuHardwareService.Msus : msuLookupService.Msus;
        var msus = msuList.Where(x =>
            x is { IgnoreUnknown: false, MsuType: null, NumUniqueTracks: >= 15 } &&
            string.IsNullOrEmpty(x.Settings.MsuTypeName));
        
        foreach (var msu in msus)
        {
            var msuModel = mapper.Map<MsuDetailsWindowViewModel>(msu.Settings);
            msuModel.Msu = msu;
            msuModel.TrackCount = msu.NumUniqueTracks;
            msuModel.MsuTypeNames = msuTypes;
            msuModel.HasBeenModified = false;

            if (msuModel.MsuPath.Length > MaxPathLength)
            {
                msuModel.AbbreviatedPath = $"...{msuModel.MsuPath.Substring(msuModel.MsuPath.Length - MaxPathLength)}";
            }
            else
            {
                msuModel.AbbreviatedPath = msuModel.MsuPath;
            }
            
            msuModel.PropertyChanged += (sender, args) =>
            {
                _model.HasBeenModified = true;
            };
            
            msuModels.Add(msuModel);
        }

        msuModels.Last().IsNotLast = false;

        _model.UnknownMsus = msuModels;
        _model.HasBeenModified = false;

        return _model;
    }

    public void Save()
    {
        foreach (var msuModel in _model.UnknownMsus.Where(x => x.Msu != null))
        {
            mapper.Map(msuModel, msuModel.Msu!.Settings);
            msuModel.Msu.MsuType = msuTypeService.GetMsuType(msuModel.MsuTypeName);
            msuModel.Msu.Settings.IsUserUnknownMsu = string.IsNullOrEmpty(msuModel.MsuTypeName);
            userOptionsService.UpdateMsuSettings(msuModel.Msu);
            
            if (_isHardwareMode && !string.IsNullOrEmpty(msuModel.MsuTypeName))
            {
                msuHardwareService.RefreshMsu(msuModel.MsuPath);
            }
            else if (!_isHardwareMode)
            {
                msuCacheService.Remove(msuModel.Msu.Path, false);
            }
        }

        userOptionsService.Save();

        if (!_isHardwareMode)
        {
            ITaskService.Run(() =>
            {
                msuLookupService.LookupMsus();
            });
        }
    }

    public void SaveIgnore()
    {
        foreach (var msuModel in _model.UnknownMsus.Where(x => x.Msu != null))
        {
            msuModel.Msu!.Settings.IsUserUnknownMsu = true;
            userOptionsService.UpdateMsuSettings(msuModel.Msu);
        }

        userOptionsService.Save();
    }
}