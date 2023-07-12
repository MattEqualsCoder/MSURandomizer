using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibrary.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MsuWindow
{
    public new readonly MsuUserOptions DataContext;
    private readonly IMsuTypeService _msuTypeService;
    private readonly IMsuLookupService _msuLookupService;
    private readonly IMsuSelectorService _msuSelectorService;
    private readonly IMsuUserOptionsService _msuUserOptionsService;
    private readonly IMsuUiFactory _msuUiFactory;
    private readonly ILogger<MsuWindow> _logger;
    private readonly MsuAppSettings _msuAppSettings;
        
    public MsuWindow(ILogger<MsuWindow> logger, IMsuUiFactory uiFactory, IMsuLookupService msuLookupService, MsuUserOptions msuUserOptions, IMsuTypeService msuTypeService, IMsuSelectorService msuSelectorService, IMsuUserOptionsService msuUserOptionsService, IMsuUiFactory msuUiFactory, MsuAppSettings msuAppSettings)
    {
        DataContext = msuUserOptions;
        _msuTypeService = msuTypeService;
        _msuSelectorService = msuSelectorService;
        _msuUserOptionsService = msuUserOptionsService;
        _msuUiFactory = msuUiFactory;
        _msuAppSettings = msuAppSettings;
        _msuLookupService = msuLookupService;
        _logger = logger;
        InitializeComponent();

        MsuList = uiFactory.CreateMsuList();
        MainGrid.Children.Add(MsuList);
        MsuList.SelectedMsusUpdated += MsuListOnSelectedMsusUpdated;
        
        FilterComboBox.SelectedItem = _msuUserOptionsService.MsuUserOptions.Filter;

        msuLookupService.OnMsuLookupComplete += (sender, args) => OnMsuLookupComplete(args.Msus);
        if (msuLookupService.Status == MsuLoadStatus.Loaded)
        {
            Dispatcher.InvokeAsync(() => { OnMsuLookupComplete(msuLookupService.Msus); });
        }
    }

    private void OnMsuLookupComplete(IEnumerable<Msu> msus)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => OnMsuLookupComplete(msus));
            return;
        }

        if (!msus.Any())
        {
            OpenUserSettingsWindow();
            return;
        }
 
        var availableMsuTypes = msus.Select(x => x.SelectedMsuType).Distinct().Where(x => x is { Selectable: true }).Cast<MsuType>();
        
        // If we have SMZ3 legacy, make sure SMZ3 is added to the dropdown
        if (msus.Select(x => x.SelectedMsuType).Any(x => x is { Selectable: false} && x.Name == _msuAppSettings.Smz3LegacyMsuTypeName) && availableMsuTypes.All(x => x.Name != _msuAppSettings.Smz3MsuTypeName))
        {
            availableMsuTypes = availableMsuTypes.Append(_msuTypeService.GetMsuType(_msuAppSettings.Smz3MsuTypeName)!).OrderBy(x => x.Name);
        }
        
        MsuTypesComboBox.ItemsSource = availableMsuTypes.Select(x => x.Name).ToList();
        MsuTypesComboBox.IsEnabled = availableMsuTypes.Any();
        if (availableMsuTypes.Any(x => x.Name == DataContext.OutputMsuType))
        {
            MsuTypesComboBox.SelectedItem = availableMsuTypes.First(x => x.Name == DataContext.OutputMsuType).Name;
        }
        else
        {
            MsuTypesComboBox.SelectedIndex = 0;
        }
    }

    private void MsuListOnSelectedMsusUpdated(object? sender, MsuListEventArgs e)
    {
        var msuCount = e.Msus.Count;

        if (msuCount == 0)
        {
            RandomMsuButton.Content = "_Select MSU";
            RandomMsuButton.IsEnabled = false;
            ShuffledMsuButton.IsEnabled = false;
        }
        else if (msuCount == 1)
        {
            RandomMsuButton.Content = "_Select MSU";
            RandomMsuButton.IsEnabled = true;
            ShuffledMsuButton.IsEnabled = false;
        }
        else
        {
            RandomMsuButton.Content = "Pick _Random MSU";
            RandomMsuButton.IsEnabled = true;
            ShuffledMsuButton.IsEnabled = true;
        }
    }

    private void RandomMsuButton_OnClick(object sender, RoutedEventArgs e)
    {
        var msus = MsuList.SelectedMsus;
        var window = new MsuCreateWindow(DataContext, MsuRandomizationStyle.Single);
        if (window.ShowDialog() != true) return;
        var msuType = SelectedMsuType;
        if (msuType == null)
        {
            return;
        }
        
        DataContext.SelectedMsus = msus.Select(x => x.Path).ToList();
        _msuUserOptionsService.Save();
        _msuSelectorService.PickRandomMsu();
    }

    public MsuList MsuList
    {
        get;
        set;
    }

    private void ShuffledMsuButton_OnClick(object sender, RoutedEventArgs e)
    {
        var msus = MsuList.SelectedMsus;
        var window = new MsuCreateWindow(DataContext, MsuRandomizationStyle.Shuffled);
        if (window.ShowDialog() != true) return;
        var msuType = SelectedMsuType;
        if (msuType == null)
        {
            return;
        }

        DataContext.SelectedMsus = msus.Select(x => x.Path).ToList();
        _msuUserOptionsService.Save();
        _msuSelectorService.CreateShuffledMsu();
    }
    
    private void ContinuousMsuButton_OnClick(object sender, RoutedEventArgs e)
    {
        var msus = MsuList.SelectedMsus;
        var window = new MsuCreateWindow(DataContext, MsuRandomizationStyle.Continuous);
        if (window.ShowDialog() != true) return;
        var msuType = SelectedMsuType;
        if (msuType == null)
        {
            return;
        }

        DataContext.SelectedMsus = msus.Select(x => x.Path).ToList();
        _msuUserOptionsService.Save();
        _msuUiFactory.OpenContinousShuffleWindow();
    }

    private void OptionsButton_OnClick(object sender, RoutedEventArgs e)
    {
        OpenUserSettingsWindow();
    }

    private void MsuTypesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var msuType = SelectedMsuType;
        DataContext.OutputMsuType = msuType?.Name;
        MsuList.TargetMsuType = msuType;
        MsuList.BasePath = msuType != null && DataContext.MsuTypePaths.TryGetValue(msuType, out string? path) && !string.IsNullOrEmpty(path)
            ? path
            : DataContext.DefaultMsuPath;
    }

    private MsuType? SelectedMsuType => _msuTypeService.GetMsuType(MsuTypesComboBox.SelectedItem as string ?? "");

    private void OpenUserSettingsWindow()
    {
        if (!_msuUiFactory.OpenUserSettingsWindow()) return;
            
        // If any of the paths were modified, look up the MSUs again and refresh the list
        _msuLookupService.LookupMsus(DataContext.DefaultMsuPath, DataContext.MsuTypePaths);  
        DataContext.OutputMsuType = SelectedMsuType?.Name;
        MsuList.TargetMsuType = SelectedMsuType;
        MsuList.BasePath = SelectedMsuType != null && DataContext.MsuTypePaths.TryGetValue(SelectedMsuType, out var path) && !string.IsNullOrEmpty(path)
            ? path
            : DataContext.DefaultMsuPath;
    }

    private void SelectAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        MsuList.SelectAll();
    }
    
    private void SelectNoneButton_OnClick(object sender, RoutedEventArgs e)
    {
        MsuList.UnselectAll();
    }

    private void FilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FilterComboBox.SelectedItem is not MsuFilter filter) return;
        DataContext.Filter = filter;
        MsuList.MsuFilter = filter;
    }

    
}