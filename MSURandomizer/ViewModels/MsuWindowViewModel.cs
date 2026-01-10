using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

public partial class MsuWindowViewModel : ViewModelBase
{
    [Reactive] public partial MsuFilter Filter { get; set; }

    [Reactive] public partial string SelectedMsuType { get; set; }

    [Reactive] public partial List<string> MsusTypes { get; set; }

    [Reactive] public partial ICollection<MsuViewModel> SelectedMsus { get; set; }
    
    [Reactive] public partial bool CanDisplaySelectMsuButton { get; set; }
    
    [Reactive] public partial bool CanDisplayCancelButton { get; set; }
    
    [Reactive] public partial bool CanDisplayRandomMsuButton { get; set; }
    
    [Reactive] public partial bool CanDisplayShuffledMsuButton { get; set; }
    
    [Reactive] public partial bool CanDisplayContinuousShuffleButton { get; set; }
    
    [Reactive] public partial bool CanDisplayUploadButton { get; set; }
    
    [Reactive] public partial bool WasClosed { get; set; }

    [Reactive] public partial bool IsHardwareModeButtonVisible { get; set; }

    [Reactive] public partial bool MsuWindowDisplayOptionsButton { get; set; }
    
    [Reactive] public partial bool DisplayMsuTypeComboBox { get; set; }
    
    [Reactive] 
    [ReactiveLinkedProperties(nameof(IsShuffledMsuButtonVisible), nameof(IsContinuousShuffleButtonVisible), nameof(IsMsuWindowUploadButtonVisible))]
    public partial bool IsHardwareModeEnabled { get; set; }

    public bool IsSelectMsuButtonVisible => CanDisplaySelectMsuButton;

    public bool IsCancelButtonVisible => CanDisplayCancelButton;

    public bool IsRandomMsuButtonVisible => CanDisplayRandomMsuButton;

    public bool IsShuffledMsuButtonVisible => CanDisplayShuffledMsuButton && !IsHardwareModeEnabled;

    public bool IsContinuousShuffleButtonVisible => CanDisplayContinuousShuffleButton && !IsHardwareModeEnabled;
    
    public bool IsMsuWindowUploadButtonVisible => CanDisplayUploadButton && IsHardwareModeEnabled;

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsSelectMsuEnabled), nameof(IsRandomMsuEnabled), nameof(IsShuffledMsuEnabled), 
        nameof(IsContinuousShuffleEnabled), nameof(MsuCountText), nameof(SelectMsusText), nameof(RandomMsuText))]
    public partial int MsuCount { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsSelectMsuEnabled), nameof(IsRandomMsuEnabled), nameof(IsShuffledMsuEnabled), 
        nameof(IsContinuousShuffleEnabled))]
    public partial bool AreMsusLoading { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsSelectMsuEnabled), nameof(IsRandomMsuEnabled), nameof(IsShuffledMsuEnabled),
        nameof(IsContinuousShuffleEnabled))]
    public partial bool IsMsuMonitorActive { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(HasGitHubUrl))]
    public partial string? GitHubUrl { get; set; }
    
    [Reactive]
    public partial bool IsSingleSelectionMode { get; set; }

    public bool IsSelectMsuEnabled  => MsuCount > 0 && !AreMsusLoading && !IsMsuMonitorActive;

    public bool IsRandomMsuEnabled => MsuCount > 0 && !AreMsusLoading && !IsMsuMonitorActive;

    public bool IsShuffledMsuEnabled => MsuCount > 0 && !AreMsusLoading && !IsMsuMonitorActive;
    
    public bool IsContinuousShuffleEnabled => MsuCount > 0 && !AreMsusLoading && !IsMsuMonitorActive;
    
    public string MsuCountText => MsuCount == 1 ? "1 MSU Selected" : $"{MsuCount} MSUs Selected";

    public string SelectMsusText => MsuCount > 1 ? $"Select {MsuCount} _MSUs" : "Select _MSU";
    
    public string RandomMsuText => MsuCount > 1 ? "Pick Random _MSU" : "Select _MSU";
    
    public bool IsCancelEnabled => true;

    public bool HasGitHubUrl => !string.IsNullOrEmpty(GitHubUrl);
    
    public bool HasMsuFolder { get; set; }
    
    public bool DisplayDesktopPopupOnLoad { get; set; }
    
    public bool DisplaySettingsWindowOnLoad { get; set; }

    public int FilterColumnIndex { get; set; } = 1;

    public MsuWindowViewModel()
    {
        SelectedMsuType = string.Empty;
        MsusTypes = [];
        SelectedMsus = new List<MsuViewModel>();
        IsHardwareModeButtonVisible = true;
        MsuWindowDisplayOptionsButton = true;
        DisplayMsuTypeComboBox = true;
        AreMsusLoading = true;
    }
}