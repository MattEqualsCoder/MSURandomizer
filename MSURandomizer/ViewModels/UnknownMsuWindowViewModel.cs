using System.Collections.Generic;

namespace MSURandomizer.ViewModels;

public class UnknownMsuWindowViewModel : ViewModelBase
{
    public List<MsuDetailsWindowViewModel> UnknownMsus { get; set; } = [];
}