using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using MessageBox = System.Windows.MessageBox;
using SelectionMode = System.Windows.Controls.SelectionMode;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MsuWindow : Window
{
    private readonly IMsuTypeService _msuTypeService;
    private readonly IMsuLookupService _msuLookupService;
    private readonly IMsuSelectorService _msuSelectorService;
    private readonly IMsuUserOptionsService _msuUserOptionsService;
    private readonly IMsuUiFactory _msuUiFactory;
    private readonly ILogger<MsuWindow> _logger;
    private readonly MsuAppSettings _msuAppSettings;
    private SelectionMode _selectionMode;
        
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msuLookupService"></param>
    /// <param name="msuUserOptions"></param>
    /// <param name="msuTypeService"></param>
    /// <param name="msuSelectorService"></param>
    /// <param name="msuUserOptionsService"></param>
    /// <param name="msuUiFactory"></param>
    /// <param name="msuAppSettings"></param>
    public MsuWindow(ILogger<MsuWindow> logger, IMsuLookupService msuLookupService, MsuUserOptions msuUserOptions, IMsuTypeService msuTypeService, IMsuSelectorService msuSelectorService, IMsuUserOptionsService msuUserOptionsService, IMsuUiFactory msuUiFactory, MsuAppSettings msuAppSettings)
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

        MsuList = new MsuList(_msuUiFactory);
    }
    
    /// <summary>
    /// The user options for the msu window
    /// </summary>
    public new MsuUserOptions DataContext { get; set; }

    /// <summary>
    /// Shows the MSU Window in the multiple selection mode
    /// </summary>
    public new void Show() => Show(SelectionMode.Multiple, false);
    
    /// <summary>
    /// Shows the MSU Window as a dialog in multiple selection mode
    /// </summary>
    public new void ShowDialog() => Show(SelectionMode.Multiple, true);

    /// <summary>
    /// Shows the MSU window
    /// </summary>
    /// <param name="selectionMode">If the list selection mode should be multiple or single</param>
    /// <param name="dialog">If the window should be displayed as a modal or not</param>
    public void Show(SelectionMode selectionMode, bool dialog)
    {
        // Create the MSU List
        MsuList = _msuUiFactory.CreateMsuList(selectionMode);
        ToggleInput(false);
        MainGrid.Children.Add(MsuList);
        MsuList.SelectedMsusUpdated += MsuListOnSelectedMsusUpdated;
        _selectionMode = selectionMode;
        _msuLookupService.OnMsuLookupComplete += (sender, args) => OnMsuLookupComplete(args.Msus);

        if (!string.IsNullOrEmpty(_msuAppSettings.MsuWindowTitle))
        {
            Title = _msuAppSettings.MsuWindowTitle;
        }
        
        if (_msuLookupService.Status == MsuLoadStatus.Loaded)
        {
            Dispatcher.InvokeAsync(() => { OnMsuLookupComplete(_msuLookupService.Msus); });
        }
        FilterComboBox.SelectedItem = _msuUserOptionsService.MsuUserOptions.Filter;
        
        RandomMsuButton.Visibility =
            _msuAppSettings.MsuWindowDisplayRandomButton == true ? Visibility.Visible : Visibility.Collapsed;
        ShuffledMsuButton.Visibility =
            _msuAppSettings.MsuWindowDisplayShuffleButton == true ? Visibility.Visible : Visibility.Collapsed;
        ContinuousShuffledMsuButton.Visibility =
            _msuAppSettings.MsuWindowDisplayContinuousButton == true ? Visibility.Visible : Visibility.Collapsed;
        SelectMsusButtons.Visibility =
            _msuAppSettings.MsuWindowDisplaySelectButton == true ? Visibility.Visible : Visibility.Collapsed;
        CancelButton.Visibility =
            _msuAppSettings.MsuWindowDisplaySelectButton == true ? Visibility.Visible : Visibility.Collapsed;
        OptionsButton.Visibility =
            _msuAppSettings.MsuWindowDisplayOptionsButton == true ? Visibility.Visible : Visibility.Collapsed;

        SelectAllButton.IsEnabled = selectionMode == SelectionMode.Multiple;
        SelectNoneButton.IsEnabled = selectionMode == SelectionMode.Multiple;

        if (!string.IsNullOrEmpty(_msuAppSettings.ForcedMsuType))
        {
            MsuTypesPanel.Visibility = Visibility.Collapsed;
            TopGrid.ColumnDefinitions[0].Width = GridLength.Auto;
            TopGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }

        if (dialog || _msuAppSettings.MsuWindowDisplaySelectButton == true)
        {
            base.ShowDialog();
        }
        else
        {
            base.Show();
        }
    }

    private void ToggleInput(bool isEnabled)
    {
        LoadingBarStackPanel.Visibility = isEnabled ? Visibility.Collapsed : Visibility.Visible;
        MsuList.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
        MsuList.IsEnabled = isEnabled;
        MsuTypesComboBox.IsEnabled = isEnabled;
        FilterComboBox.IsEnabled = isEnabled;
        SelectAllButton.IsEnabled = isEnabled && _selectionMode == SelectionMode.Multiple;
        SelectNoneButton.IsEnabled = isEnabled && _selectionMode == SelectionMode.Multiple;
        SelectMsusButtons.IsEnabled = isEnabled;
        RandomMsuButton.IsEnabled = isEnabled;
        ShuffledMsuButton.IsEnabled = isEnabled;
        OptionsButton.IsEnabled = IsEnabled;
        ContinuousShuffledMsuButton.IsEnabled = isEnabled;
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

        ToggleInput(true);
        
        var availableMsuTypes = msus.Where(x => x.Tracks.Count > 0).Select(x => x.SelectedMsuType).Distinct().Where(x => x is { Selectable: true }).Cast<MsuType>();

        // If we have SMZ3 legacy, make sure SMZ3 is added to the dropdown
        var smz3MsuTypes = msus.Select(x => x.SelectedMsuType)
            .Where(x => x != null && _msuAppSettings.Smz3MsuTypes?.Contains(x.DisplayName) == true)
            .Distinct()
            .Cast<MsuType>()
            .ToList();
        if (smz3MsuTypes.All(x => x.Selectable == false) && smz3MsuTypes.Count > 0)
        {
            var smz3MsuType = _msuTypeService.MsuTypes.First(x =>
                x is { Selectable: true } && _msuAppSettings.Smz3MsuTypes?.Contains(x.DisplayName) == true);
            availableMsuTypes = availableMsuTypes.Append(smz3MsuType).OrderBy(x => x.DisplayName);
        }
        
        MsuTypesComboBox.ItemsSource = availableMsuTypes.Select(x => x.DisplayName).ToList();
        MsuTypesComboBox.IsEnabled = availableMsuTypes.Any();
        if (availableMsuTypes.Any(x => x.DisplayName == DataContext.OutputMsuType))
        {
            MsuTypesComboBox.SelectedItem = availableMsuTypes.First(x => x.DisplayName == DataContext.OutputMsuType).DisplayName;
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
            SelectMsusButtons.Content = "_Select MSU";
            RandomMsuButton.Content = "_Select MSU";
            SelectMsusButtons.IsEnabled = false;
            RandomMsuButton.IsEnabled = false;
            ShuffledMsuButton.IsEnabled = false;
            ContinuousShuffledMsuButton.IsEnabled = false;
        }
        else if (msuCount == 1)
        {
            SelectMsusButtons.Content = "_Select MSU";
            RandomMsuButton.Content = "_Select MSU";
            SelectMsusButtons.IsEnabled = true;
            RandomMsuButton.IsEnabled = true;
            ShuffledMsuButton.IsEnabled = false;
            ContinuousShuffledMsuButton.IsEnabled = false;
            SelectMsusButtons.Content = "_Select MSU";
        }
        else
        {
            SelectMsusButtons.Content = $"_Select {msuCount} MSUs";
            RandomMsuButton.Content = "Pick _Random MSU";
            SelectMsusButtons.IsEnabled = true;
            RandomMsuButton.IsEnabled = true;
            ShuffledMsuButton.IsEnabled = true;
            ContinuousShuffledMsuButton.IsEnabled = true;
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
        var response = _msuSelectorService.PickRandomMsu(new MsuSelectorRequest()
        {
            EmptyFolder = true,
            Msus = msus,
            OutputMsuType = msuType
        });
        ShowSelectorResponseMessage(response);
    }

    /// <summary>
    /// The list of MSUs to pick from
    /// </summary>
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
        var response = _msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
        {
            EmptyFolder = true,
            Msus = msus,
            OutputMsuType = msuType
        });
        ShowSelectorResponseMessage(response);
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
        DataContext.OutputMsuType = msuType?.DisplayName;
        MsuList.TargetMsuType = msuType;
        MsuList.BasePath = msuType != null && DataContext.MsuTypePaths.TryGetValue(msuType, out string? path) && !string.IsNullOrEmpty(path)
            ? path
            : DataContext.DefaultMsuPath;
    }

    private MsuType? SelectedMsuType => string.IsNullOrEmpty(_msuAppSettings.ForcedMsuType) 
        ? _msuTypeService.GetMsuType(MsuTypesComboBox.SelectedItem as string ?? "")
        : _msuTypeService.GetMsuType(_msuAppSettings.ForcedMsuType);

    private void OpenUserSettingsWindow()
    {
        if (!_msuUiFactory.OpenUserSettingsWindow()) return;
        
        ToggleInput(false);

        Task.Run(() =>
        {
            // If any of the paths were modified, look up the MSUs again and refresh the list
            _msuLookupService.LookupMsus(DataContext.DefaultMsuPath, DataContext.MsuTypePaths);

            Dispatcher.Invoke(() =>
            {
                DataContext.OutputMsuType = SelectedMsuType?.DisplayName;
                MsuList.TargetMsuType = SelectedMsuType;
                MsuList.BasePath = SelectedMsuType != null &&
                                   DataContext.MsuTypePaths.TryGetValue(SelectedMsuType, out var path) &&
                                   !string.IsNullOrEmpty(path)
                    ? path
                    : DataContext.DefaultMsuPath;
            });
        });
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

    private void ShowSelectorResponseMessage(MsuSelectorResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            var title = response.Successful ? "Warning" : "Error";
            var icon = response.Successful ? MessageBoxImage.Warning : MessageBoxImage.Error;
            MessageBox.Show(this, response.Message, title, MessageBoxButton.OK, icon);
        }
    }
    
    private void SelectMsusButtons_OnClick(object sender, RoutedEventArgs e)
    {
        var msuType = SelectedMsuType;
        if (msuType == null)
        {
            return;
        }
        
        DataContext.OutputMsuType = msuType.DisplayName;
        DataContext.SelectedMsus = MsuList.SelectedMsus.Select(x => x.Path).ToList();
        _msuUserOptionsService.Save();
        DialogResult = MsuList.SelectedMsus.Any();
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}