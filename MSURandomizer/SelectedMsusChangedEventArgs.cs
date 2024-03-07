using System;
using System.Collections.Generic;
using MSURandomizer.ViewModels;

namespace MSURandomizer;

public class SelectedMsusChangedEventArgs(ICollection<MsuViewModel>? selectedMsus) : EventArgs
{
    public ICollection<MsuViewModel>? SelectedMsus { get; set; } = selectedMsus;

    public int Count => SelectedMsus?.Count ?? 0;
}