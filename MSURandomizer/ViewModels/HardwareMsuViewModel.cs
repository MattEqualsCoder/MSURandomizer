using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class HardwareMsuViewModel : ViewModelBase
{
    [Reactive] public string Message { get; set; }
    
    public List<MsuViewModel> Msus { get; set; }
}