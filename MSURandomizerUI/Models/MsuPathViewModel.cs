using System.IO;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerUI.Models;

internal class MsuPathViewModel : ViewModel
{
    public MsuPathViewModel()
    {
    }
    
    public MsuPathViewModel(MsuType? msuType, string? path)
    {
        MsuType = msuType;
        MsuTypeName = msuType?.DisplayName ?? "Default MSU Path";
        _msuPath = path;
    }

    public void UpdateSettings(MsuUserOptions options)
    {
        if (MsuType == null)
        {
            options.DefaultMsuPath = _msuPath;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_msuPath) && options.MsuTypePaths.ContainsKey(MsuType))
            {
                options.MsuTypePaths.Remove(MsuType);
                options.MsuTypeNamePaths.Remove(MsuTypeName ?? "");
            }
            else if (!string.IsNullOrWhiteSpace(_msuPath) && Directory.Exists(_msuPath))
            {
                options.MsuTypePaths[MsuType] = _msuPath ?? "";
                options.MsuTypeNamePaths[MsuType.DisplayName] = _msuPath ?? "";
            }
        }
    }
    
    public MsuType? MsuType { get; set; }
    
    public string? MsuTypeName { get; set; } = "";
    
    
    private string? _msuPath;

    public string? MsuPath
    {
        get => _msuPath;
        set => SetField(ref _msuPath, value);
    }
}