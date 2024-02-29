using System.Linq;
using AutoMapper;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class MsuDetailsService(
    IMsuUserOptionsService userOptionsService,
    IMapper mapper,
    IMsuTypeService msuTypeService,
    IMsuCacheService msuCacheService,
    IMsuLookupService msuLookupService) : IControlService
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
        Model.Tracks =
            _parentModel.Msu.MsuType?.Tracks.OrderBy(x => x.Number)
                .Select(t => new MsuTrackViewModel(t, _parentModel.Msu.Tracks)).ToList() ??
            [];
        Model.MsuTypeNames = [""];
        Model.MsuTypeNames.AddRange(msuTypeService.MsuTypes.Select(x => x.DisplayName).OrderBy(x => x));
        Model.HasBeenModified = false;
        return Model;
    }

    public void Save()
    {
        if (Model.Msu == null) return;
        mapper.Map(Model, Model.Msu.Settings);
        userOptionsService.SaveMsuSettings(Model.Msu);
        _parentModel.MsuName = Model.Msu?.DisplayName;
        _parentModel.MsuCreator = Model.Msu?.DisplayCreator;
        
        if (_originalMsuTypeName != Model.MsuTypeName)
        {
            msuCacheService.Remove(Model.MsuPath, false);
            ITaskService.Run(() =>
            {
                msuLookupService.LookupMsus();
            });
        }
        
    }
}