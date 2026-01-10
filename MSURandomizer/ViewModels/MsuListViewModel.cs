using System.Collections.Generic;
using Avalonia.Controls;
using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

public partial class MsuListViewModel : ViewModelBase
{
    [Reactive] public partial bool IsLoading { get; set; }

    [Reactive] public partial IReadOnlyCollection<Msu> Msus { get; set; }

    [Reactive] public partial List<MsuViewModel> MsuViewModels { get; set; }
    
    [Reactive] public partial List<MsuViewModel> SelectedMsus { get; set; }
    
    [Reactive] public partial List<MsuViewModel> FilteredMsus { get; set; }
    
    [Reactive] public partial string? MsuTypeName { get; set; }
    
    [Reactive] public partial MsuType? MsuType { get; set; }
    
    [Reactive] public partial bool IsMsuMonitorActive { get; set; }
    
    [Reactive] public partial bool IsMsuMonitorDisabled { get; set; }
    
    [Reactive] public partial bool HardwareMode { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(SelectionMode))]
    public partial bool IsSingleSelectionMode { get; set; }

    public bool DisplayUnknownMsuWindow { get; set; }
    
    public SelectionMode SelectionMode =>
        IsSingleSelectionMode ? SelectionMode.Single : SelectionMode.Multiple | SelectionMode.Toggle;

    public MsuListViewModel()
    {
        Msus = [];
        MsuViewModels = [];
        SelectedMsus = [];
        FilteredMsus = [];
    }
}