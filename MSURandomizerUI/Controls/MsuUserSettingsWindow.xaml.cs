using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for MSUOptionsWindow.xaml
/// </summary>
public partial class MsuUserSettingsWindow : Window
{
    public new readonly MsuUserOptions DataContext;

    private readonly List<MsuDirectoryControl> _directoryControls = new();
    
    private bool _originalPromptOnUpdate { get; set; } = true;
    private bool _originalPromptOnPreRelease { get; set; }
        
    public MsuUserSettingsWindow(MsuUserOptions msuUserOptions, IMsuTypeService msuTypeService)
    {
        DataContext = msuUserOptions;
        InitializeComponent();
        AddDirectoryControl(null, msuUserOptions.DefaultMsuPath);
        foreach(var msuType in msuTypeService.MsuTypes.Where(x => x.Selectable).OrderBy(x => x.DisplayName))
        {
            msuUserOptions.MsuTypePaths.TryGetValue(msuType, out var typePath);
            AddDirectoryControl(msuType, typePath);
        }

        PromptOnUpdateCheckBox.IsChecked = _originalPromptOnUpdate = msuUserOptions.PromptOnUpdate;
        PromptOnPreReleaseCheckBox.IsChecked = _originalPromptOnPreRelease = msuUserOptions.PromptOnPreRelease;
    }

    public void AddDirectoryControl(MsuType? msuType, string? path)
    {
        var control = new MsuDirectoryControl(this, msuType, path);
        _directoryControls.Add(control);
        if (msuType == null)
        {
            BaseSettings.Children.Add(control);
        }
        else
        {
            MsuTypeSettings.Children.Add(control);
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var hasModified = false;
            
        // Update options to match what the user has set
        foreach (var control in _directoryControls)
        {
            hasModified |= control.HasModified;
            if (control.MsuType == null)
            {
                DataContext.DefaultMsuPath = control.MsuDirectory;
            }
            else if (!string.IsNullOrWhiteSpace(control.MsuDirectory) && Directory.Exists(control.MsuDirectory))
            {
                DataContext.MsuTypePaths[control.MsuType] = control.MsuDirectory;
                DataContext.MsuTypeNamePaths[control.MsuType.DisplayName] = control.MsuDirectory;
            }
            else if (string.IsNullOrWhiteSpace(control.MsuDirectory) &&
                     DataContext.MsuTypeNamePaths.ContainsKey(control.MsuType.DisplayName))
            {
                DataContext.MsuTypePaths.Remove(control.MsuType);
                DataContext.MsuTypeNamePaths.Remove(control.MsuType.DisplayName);
            }
        }

        if (_originalPromptOnUpdate != PromptOnUpdateCheckBox.IsChecked)
        {
            DataContext.PromptOnUpdate = PromptOnUpdateCheckBox.IsChecked ?? false;
            hasModified = true;
        }
        
        if (_originalPromptOnPreRelease != PromptOnPreReleaseCheckBox.IsChecked)
        {
            DataContext.PromptOnPreRelease = PromptOnPreReleaseCheckBox.IsChecked ?? false;
            hasModified = true;
        }

        if (hasModified)
        {
            DialogResult = true;
        }
            
        Close();
    }

    private void PromptOnUpdateCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
        PromptOnPreReleaseCheckBox.IsEnabled = PromptOnUpdateCheckBox.IsChecked ?? false;
    }
}