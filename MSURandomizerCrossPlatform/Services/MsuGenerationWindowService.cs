using System.Collections.Generic;
using AutoMapper;
using AvaloniaControls.ControlServices;
using MSURandomizerCrossPlatform.ViewModels;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Services;

namespace MSURandomizerCrossPlatform.Services;

public class MsuGenerationWindowService(
    IMsuUserOptionsService userOptionsService, 
    IMapper mapper) : IControlService
{
    public MsuGenerationViewModel Model { get; set; } = new();

    public MsuGenerationViewModel InitializeModel(MsuRandomizationStyle style, string outputMsuType, ICollection<string> selectedMsus)
    {
        mapper.Map(userOptionsService.MsuUserOptions, Model);
        Model.RandomizationStyle = style;
        Model.OutputMsuType = outputMsuType;
        Model.SelectedMsus = selectedMsus;
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
}