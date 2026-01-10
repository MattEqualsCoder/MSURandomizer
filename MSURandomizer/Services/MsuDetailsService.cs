using System.Linq;
using AutoMapper;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class MsuDetailsService(
    IMsuUserOptionsService userOptionsService,
    IMapper mapper,
    IMsuTypeService msuTypeService,
    IMsuCacheService msuCacheService,
    IMsuLookupService msuLookupService,
    IMsuHardwareService msuHardwareService) : ControlService
{
    private MsuViewModel _parentModel = new();
    public MsuDetailsWindowViewModel Model { get; set; } = new();
    private string? _originalMsuTypeName;
    
    public MsuDetailsWindowViewModel InitilizeModel(MsuViewModel model)
    {
        _parentModel = model;
        mapper.Map(_parentModel.Msu.Settings, Model);
        _originalMsuTypeName = Model.MsuTypeName;
        Model.Msu = _parentModel.Msu;

        if (_parentModel.Msu.MsuType != null)
        {
            Model.Tracks = _parentModel.Msu.MsuType.Tracks.OrderBy(x => x.Number)
                .Select(t => new MsuTrackViewModel(t, _parentModel.Msu.Tracks)).ToList();
        }
        else
        {
            Model.Tracks = _parentModel.Msu.Tracks.OrderBy(x => x.Number)
                .Select(t => new MsuTrackViewModel(t, _parentModel.Msu.Tracks)).ToList();
        }
        
        Model.TrackCount = Model.Tracks.Count;
        Model.MsuTypeNames = [""];
        Model.MsuTypeNames.AddRange(msuTypeService.MsuTypes.Select(x => x.DisplayName).OrderBy(x => x));
        Model.MsuPath = _parentModel.MsuPath;
        Model.UpdateCopyrightOptions();
        Model.HasBeenModified = false;
        return Model;
    }

    public void Save()
    {
        if (Model.Msu == null) return;
        mapper.Map(Model, Model.Msu.Settings);
        Model.Msu.Settings.MsuType = msuTypeService.GetMsuType(Model.MsuTypeName);
        Model.Msu.Settings.IsUserUnknownMsu = string.IsNullOrEmpty(Model.MsuTypeName);
        Model.Msu.Settings.IsCopyrightSafeOverrides = Model.Tracks.SelectMany(x => x.Songs)
            .Where(x => x.IsCopyrightSafeValueOverridden)
            .ToDictionary(x => x.Path, x => x.IsCopyrightSafeCombined ?? false);

        foreach (var track in Model.Msu.Tracks)
        {
            if (Model.Msu.Settings.IsCopyrightSafeOverrides.TryGetValue(track.Path, out var value))
            {
                track.IsCopyrightSafeOverride = value;
            }
            else
            {
                track.IsCopyrightSafeOverride = null;
            }
        }
        
        Model.Msu.AreAllTracksCopyrightSafe = Model.Msu.Tracks.All(x => x.IsCopyrightSafeCombined == true);
        
        userOptionsService.SaveMsuSettings(Model.Msu);
        _parentModel.MsuName = Model.Msu?.DisplayName;
        _parentModel.MsuCreator = Model.Msu?.DisplayCreator;
        
        if (_originalMsuTypeName != Model.MsuTypeName)
        {
            if (Model.Msu?.IsHardwareMsu != true)
            {
                msuCacheService.Remove(Model.MsuPath, false);
                ITaskService.Run(() =>
                {
                    msuLookupService.LookupMsus();
                });
            }
            else
            {
                ITaskService.Run(() =>
                {
                    msuHardwareService.RefreshMsu(Model.MsuPath);
                });
            }
        }
        
    }
}