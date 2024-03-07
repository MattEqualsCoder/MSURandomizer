using System;
using System.Collections.Generic;
using AvaloniaControls.Models;
using MSURandomizerLibrary;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class MsuWindowViewModel : ViewModelBase
{
    [Reactive] public MsuFilter Filter { get; set; }

    [Reactive] public string SelectedMsuType { get; set; } = "";

    [Reactive] public List<string> MsusTypes { get; set; } = new();

    [Reactive] public ICollection<MsuViewModel> SelectedMsus { get; set; } = new List<MsuViewModel>();
    
    [Reactive] public bool IsSelectMsuVisible { get; set; }
    
    [Reactive] public bool IsCancelVisible { get; set; }
    
    [Reactive] public bool IsRandomMsuVisible { get; set; }
    
    [Reactive] public bool IsShuffledMsuVisible { get; set; }
    
    [Reactive] public bool IsContinuousShuffleVisible { get; set; }
    
    [Reactive] public bool WasClosed { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsSelectMsuEnabled), nameof(IsRandomMsuEnabled), nameof(IsShuffledMsuEnabled), 
        nameof(IsContinuousShuffleEnabled), nameof(MsuCountText), nameof(SelectMsusText), nameof(RandomMsuText))]
    public int MsuCount { get; set; }
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(IsSelectMsuEnabled), nameof(IsRandomMsuEnabled), nameof(IsShuffledMsuEnabled), 
        nameof(IsContinuousShuffleEnabled))]
    public bool AreMsusLoading { get; set; } = true;

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsSelectMsuEnabled), nameof(IsRandomMsuEnabled), nameof(IsShuffledMsuEnabled),
        nameof(IsContinuousShuffleEnabled))]
    public bool IsMsuMonitorActive { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(HasGitHubUrl))]
    public string? GitHubUrl { get; set; }

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
}