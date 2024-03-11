using System.Collections.Generic;
using AutoMapper;
using AvaloniaControls.ControlServices;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class MsuGenerationWindowService(
    IMsuUserOptionsService userOptionsService, 
    IMsuAppSettingsService appSettingsService,
    IMapper mapper) : ControlService
{
    public MsuGenerationViewModel Model { get; set; } = new();

    public MsuGenerationViewModel InitializeModel(MsuRandomizationStyle style, string outputMsuType, ICollection<string> selectedMsus)
    {
        mapper.Map(userOptionsService.MsuUserOptions, Model);
        Model.RandomizationStyle = style;
        Model.OutputMsuType = outputMsuType;
        Model.SelectedMsus = selectedMsus;
        Model.IsLaunchRomVisible = appSettingsService.MsuAppSettings.CanLaunchRoms;
        return Model;
    }

    public void SaveFolder(string path)
    {
        Model.OutputFolderPath = path;
        Model.OutputRomPath = null;
        mapper.Map(Model, userOptionsService.MsuUserOptions);
        userOptionsService.Save();
    }

    public void SaveFile(string path)
    {
        Model.OutputFolderPath = null;
        Model.OutputRomPath = path;
        mapper.Map(Model, userOptionsService.MsuUserOptions);
        userOptionsService.Save();
    }
    
    public void Save()
    {
        mapper.Map(Model, userOptionsService.MsuUserOptions);
        userOptionsService.Save();
    }
}