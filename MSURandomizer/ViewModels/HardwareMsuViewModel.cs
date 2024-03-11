using System.Collections.Generic;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class HardwareMsuViewModel : ViewModelBase
{
    [Reactive] public string Message { get; set; } = "Connecting";

    public List<MsuViewModel> Msus { get; set; } = new();
    
    public bool Complete { get; set; }
    
    public Msu? SelectedMsu { get; set; }
}