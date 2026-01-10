using System.Collections.Generic;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

public partial class HardwareMsuViewModel : ViewModelBase
{
    [Reactive] public partial string Message { get; set; }

    public List<MsuViewModel> Msus { get; set; }
    
    public bool Complete { get; set; }
    
    public Msu? SelectedMsu { get; set; }
    
    public MsuType? SelectedMsuType { get; set; }

    public HardwareMsuViewModel()
    {
        Message = "Connecting";
        Msus = [];
    }
}