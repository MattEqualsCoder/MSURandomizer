using System.Collections.Generic;
using Avalonia.Controls;
using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class MsuListViewModel : ViewModelBase
{
    [Reactive] public bool IsLoading { get; set; }

    [Reactive] public IReadOnlyCollection<Msu> Msus { get; set; } = new List<Msu>();

    [Reactive] public List<MsuViewModel> MsuViewModels { get; set; } = new();
    
    [Reactive] public List<MsuViewModel> SelectedMsus { get; set; } = new();
    
    [Reactive] public List<MsuViewModel> FilteredMsus { get; set; } = new();
    
    [Reactive] public string? MsuTypeName { get; set; }
    
    [Reactive] public MsuType? MsuType { get; set; }
    
    [Reactive] public bool IsMsuMonitorActive { get; set; }
    
    [Reactive] public bool IsMsuMonitorDisabled { get; set; }
    
    [Reactive] public bool HardwareMode { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(SelectionMode))]
    public bool IsSingleSelectionMode { get; set; }

    public SelectionMode SelectionMode =>
        IsSingleSelectionMode ? SelectionMode.Single : SelectionMode.Multiple | SelectionMode.Toggle;
}