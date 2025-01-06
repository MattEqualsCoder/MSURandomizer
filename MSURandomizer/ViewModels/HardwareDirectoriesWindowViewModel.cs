using System.Collections.Generic;
using AvaloniaControls.Models;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class HardwareDirectoriesWindowViewModel : ViewModelBase
{
    [Reactive] public List<HardwareItem> TreeNodes { get; set; } = [];
    
    [Reactive, ReactiveLinkedProperties(nameof(IsHardwareDirectorySelected), nameof(IsHardwareItemSelected))]
    public HardwareItem? SelectedTreeNode { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsHardwareDirectorySelected), nameof(IsHardwareItemSelected))]
    public bool IsLoadingData { get; set; } = true;
    
    [Reactive] public string LoadingDataText { get; set; } = "Loading...";

    [Reactive] public bool IsLoadingIndeterminate { get; set; } = true;
    
    [Reactive] public double LoadingItemCount { get; set; }
    
    [Reactive] public double LoadingProgress { get; set; }
    
    public bool IsHardwareItemSelected => SelectedTreeNode != null && !IsLoadingData;
    
    public bool IsHardwareDirectorySelected => SelectedTreeNode is { IsFolder: true } && !IsLoadingData;
    
    public bool IsSelectWindow => !string.IsNullOrEmpty(MsuToUpload);
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(CloseButtonText), nameof(IsSelectWindow))]
    public string? MsuToUpload { get; set; } = "";

    public string CloseButtonText => string.IsNullOrEmpty(MsuToUpload) ? "Close" : "Cancel";

    public bool DidUpdate { get; set; }
}

public class HardwareItem(string name, string path, string parentPath, bool isFolder)
{
    public string Name => name;
    public string Path => path;
    public string ParentPath => parentPath;
    public bool IsFolder => isFolder;
    public MaterialIconKind Icon => isFolder ? MaterialIconKind.Folder : MaterialIconKind.File;
    public List<HardwareItem> Directories { get; set; } = [];
}