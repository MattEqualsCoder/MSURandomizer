using System;
using System.Collections.Generic;
using MSURandomizerCrossPlatform.ViewModels;

namespace MSURandomizerCrossPlatform;

public class SelectedMsusChangedEventArgs : EventArgs
{
    public ICollection<MsuViewModel>? SelectedMsus { get; set; }

    public int Count => SelectedMsus?.Count ?? 0;

    public SelectedMsusChangedEventArgs(ICollection<MsuViewModel>? selectedMsus)
    {
        SelectedMsus = selectedMsus;
    }
}