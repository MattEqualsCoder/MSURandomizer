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
    IMsuLookupService msuLookupService) : ControlService
{
    private UnknownMsuWindowViewModel _model = new();
    private const int MaxPathLength = 50;
    
    public UnknownMsuWindowViewModel InitilizeModel()
    {
        List<string> msuTypes = [""];
        msuTypes.AddRange(msuTypeService.MsuTypes.Select(x => x.DisplayName).OrderBy(x => x));

        List<MsuDetailsWindowViewModel> msuModels = [];
        
        foreach (var msu in msuLookupService.Msus.Where(x => x is { IgnoreUnknown: false, MsuType: null, NumUniqueTracks: >= 15 } && string.IsNullOrEmpty(x.Settings.MsuTypeName) ))
        {
            var msuModel = mapper.Map<MsuDetailsWindowViewModel>(msu.Settings);
            msuModel.Msu = msu;
            msuModel.TrackCount = msu.NumUniqueTracks;
            msuModel.MsuTypeNames = msuTypes;
            msuModel.HasBeenModified = false;

            if (msuModel.MsuPath.Length > MaxPathLength)
            {
                msuModel.MsuPath = $"...{msuModel.MsuPath.Substring(msuModel.MsuPath.Length - MaxPathLength)}";
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
            msuModel.Msu.Settings.MsuPath = msuModel.Msu.Path;
            msuModel.Msu.Settings.IsUserUnknownMsu = string.IsNullOrEmpty(msuModel.MsuTypeName);
            userOptionsService.UpdateMsuSettings(msuModel.Msu);
            msuCacheService.Remove(msuModel.Msu.Path, false);
        }

        userOptionsService.Save();
        
        ITaskService.Run(() =>
        {
            msuLookupService.LookupMsus();
        });
        
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