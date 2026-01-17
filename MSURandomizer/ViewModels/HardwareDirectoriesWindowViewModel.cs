using System.Collections.Generic;
using AvaloniaControls.Models;
using Material.Icons;
using ReactiveUI.SourceGenerators;

namespace MSURandomizer.ViewModels;

public partial class HardwareDirectoriesWindowViewModel : ViewModelBase
{
    [Reactive] public partial List<HardwareItem> TreeNodes { get; set; }
    
    [Reactive, ReactiveLinkedProperties(nameof(IsHardwareDirectorySelected), nameof(IsHardwareItemSelected))]
    public partial HardwareItem? SelectedTreeNode { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsHardwareDirectorySelected), nameof(IsHardwareItemSelected))]
    public partial bool IsLoadingData { get; set; }
    
    [ReactiveLinkedProperties(nameof(ShowHeader), nameof(AcceptButtonText), nameof(IsSelectWindow))]
    public bool IsUpload { get; set; }
    
    public bool IsConnected { get; set; }
    public bool HasConnectorWindowUp { get; set; }
    
    [Reactive] public partial string LoadingDataText { get; set; }

    [Reactive] public partial bool IsLoadingIndeterminate { get; set; }
    
    [Reactive] public partial double LoadingItemCount { get; set; }
    
    [Reactive] public partial double LoadingProgress { get; set; }

    public bool ShowHeader => IsUpload;
    
    public bool IsHardwareItemSelected => SelectedTreeNode != null && !IsLoadingData;
    
    public bool IsHardwareDirectorySelected => SelectedTreeNode is { IsFolder: true } && !IsLoadingData;
    
    public bool IsSelectWindow => !string.IsNullOrEmpty(MsuToUpload) || !IsUpload;
    
    [Reactive]
    [ReactiveLinkedProperties(nameof(CloseButtonText), nameof(IsSelectWindow))]
    public partial string? MsuToUpload { get; set; }

    public string CloseButtonText => string.IsNullOrEmpty(MsuToUpload) ? "Close" : "Cancel";
    public string AcceptButtonText => IsUpload ? "Upload MSU" : "Select Folder";

    public bool DidUpdate { get; set; }

    public HardwareDirectoriesWindowViewModel()
    {
        TreeNodes = [];
        MsuToUpload = string.Empty;
        IsLoadingData = true;
        LoadingDataText = "Loading...";
        IsLoadingIndeterminate = true;
    }
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