﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Material.Icons.WPF;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using MSURandomizerUI.Models;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for MSURandomizerControl.xaml
/// </summary>
public partial class MsuList : UserControl
{
    private readonly IMsuUiFactory _msuUiFactory;
    private readonly List<string> _displayedErrors = new();
    private readonly IMsuUserOptionsService _msuUserOptionsService;
    private readonly IMsuAppSettingsService _msuAppSettingsService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msuUiFactory"></param>
    /// <param name="msuUserOptionsService"></param>
    /// <param name="msuAppSettingsService"></param>
    public MsuList(IMsuUiFactory msuUiFactory, IMsuUserOptionsService msuUserOptionsService, IMsuAppSettingsService msuAppSettingsService)
    {
        _msuUiFactory = msuUiFactory;
        _msuUserOptionsService = msuUserOptionsService;
        _msuAppSettingsService = msuAppSettingsService;
        DataContext = new MsuListViewModel();
        InitializeComponent();
    }
    
    /// <summary>
    /// The view model with all of the MSUs
    /// </summary>
    internal new MsuListViewModel DataContext { get; private set; }

    /// <summary>
    /// Sets the view model and sets up events
    /// </summary>
    /// <param name="model">The view model to set</param>
    internal void SetDataContext(MsuListViewModel model)
    {
        DataContext = model;
        model.MsuMonitorWindowMenuItemVisibility = _msuAppSettingsService.MsuAppSettings.DisableMsuMonitorWindow == true
            ? Visibility.Collapsed
            : Visibility.Visible;
        model.MsuListUpdated += ViewModelOnMsuListUpdated;
        model.AvailableMsusUpdated += ViewModelOnAvailableMsusUpdated;
        UpdateSelectedItems();
    }

    private void ViewModelOnAvailableMsusUpdated(object? sender, MsuListEventArgs e)
    {
        UpdateSelectedItems();
    }

    private void ViewModelOnMsuListUpdated(object? sender, MsuListEventArgs e)
    {
        UpdateSelectedItems();
    }

    private void UpdateSelectedItems()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(UpdateSelectedItems);
            return;
        }

        MsuListView.SelectionMode = DataContext.SelectionMode;
        MsuListView.ItemsSource = DataContext.AvailableMsus.OrderBy(x => !x.Settings.IsFavorite).ThenBy(x => x.Name);
        MsuListView.UnselectAll();
        if (SelectionMode == SelectionMode.Multiple)
        {
            foreach (var msu in MsuListView.Items.Cast<Msu>())
            {
                if (DataContext.SelectedMsuPaths.Contains(msu?.Path ?? ""))
                {
                    MsuListView.SelectedItems.Add(msu);
                }
            }    
        }
        else
        {
            for (var i = 0; i < MsuListView.Items.Count; i++)
            {
                var msu = MsuListView.Items[i] as Msu;
                if (DataContext.SelectedMsuPaths.Contains(msu?.Path ?? ""))
                {
                    MsuListView.SelectedIndex = i;
                    return;
                }
            }
        }

        // Display errors from loading if there are any
        if (DataContext.Errors == null) return;
        var window = Window.GetWindow(this);
        if (window == null) return;
        foreach (var error in DataContext.Errors)
        {
            var fileInfo = new FileInfo(error.Key);
            var message = $"Problem loading MSU \"{fileInfo.Directory?.Name}/{fileInfo.Name}\"\r\n\r\n{error.Value}";
            if (!_displayedErrors.Contains(message))
            {
                MessageBox.Show(window, message, "MSU Load Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _displayedErrors.Add(message);
            }
        }
    }

    /// <summary>
    /// Selects all MSUs
    /// </summary>
    public void SelectAll() => MsuListView.SelectAll();

    /// <summary>
    /// Deselects all MSUs
    /// </summary>
    public void UnselectAll() => MsuListView.UnselectAll();

    /// <summary>
    /// If the user should be able to select a single or multiple MSUs
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => DataContext.SelectionMode;
        set => DataContext.SelectionMode = value;
    }

    /// <summary>
    /// The MSU type that user is trying to generate
    /// </summary>
    public MsuType? TargetMsuType
    {
        get => DataContext.MsuType;
        set
        {
            if (DataContext.MsuType != null && value != DataContext.MsuType)
            {
                DataContext.SelectedMsuPaths = new List<string>();    
            }
            DataContext.MsuType = value;
        }
    }

    /// <summary>
    /// The filter that should be applied to the MSU type
    /// </summary>
    public MsuFilter? MsuFilter
    {
        get => DataContext.MsuFilter;
        set
        {
            if (DataContext.MsuFilter != null && value != DataContext.MsuFilter)
            {
                DataContext.SelectedMsuPaths = new List<string>();
            }
            DataContext.MsuFilter = value;
        }
    }

    /// <summary>
    /// The base directory to filter MSUs by
    /// </summary>
    public string? BasePath
    {
        get => DataContext.BasePath;
        set => DataContext.BasePath = value;
    }

    /// <summary>
    /// The opened MSU Monitor Windows
    /// </summary>
    public MsuMonitorWindow? MsuMonitorWindow { get; private set; }

    /// <summary>
    /// The currently selected MSUs
    /// </summary>
    public ICollection<Msu> SelectedMsus => MsuListView.SelectedItems.Cast<Msu>().ToList();
    
    /// <summary>
    /// Event that is fired when the currently selected MSUs has changed
    /// </summary>
    public event EventHandler<MsuListEventArgs>? SelectedMsusUpdated;

    /// <summary>
    /// Event for when the MSU monitor window is opened
    /// </summary>
    public event EventHandler? MsuMonitorWindowOpened;
    
    /// <summary>
    /// Event for when the MSU monitor window is closed
    /// </summary>
    public event EventHandler? MsuMonitorWindowClosed;
    
    /// <summary>
    /// Opens the MSU Monitor window
    /// </summary>
    /// <param name="msu">The MSU to open for the MSU monitor</param>
    /// <returns></returns>
    public MsuMonitorWindow? OpenMsuMonitorWindow(Msu? msu = null)
    {
        if (_msuAppSettingsService.MsuAppSettings.DisableMsuMonitorWindow == true || MsuMonitorWindow != null)
        {
            return null;
        }
        
        MsuMonitorWindow = msu == null ? _msuUiFactory.OpenMsuMonitorWindow() : _msuUiFactory.OpenMsuMonitorWindow(msu);
        DataContext.MsuMonitorWindowEnabled = false;
        MsuMonitorWindowOpened?.Invoke(this, EventArgs.Empty);
        MsuMonitorWindow.Closed += (_, _) =>
        {
            MsuMonitorWindow = null;
            DataContext.MsuMonitorWindowEnabled = true;
            MsuMonitorWindowClosed?.Invoke(this, EventArgs.Empty);
        };

        return MsuMonitorWindow;
    }

    private void MsuList_OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void MsuListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedMsusUpdated?.Invoke(this, new MsuListEventArgs(MsuListView.SelectedItems.Cast<Msu>().ToList(), null));
    }

    private void OpenMenuDetailsMenuItem_OnClick(object sender, RoutedEventArgs e)
    { 
        if (sender is not MenuItem { Tag: Msu msu })
            return;

        _msuUiFactory.OpenMsuDetailsWindow(msu);
    }
    
    private void OpenFolderMenuItem_OnClick(object sender, RoutedEventArgs e)
    { 
        if (sender is not MenuItem { Tag: Msu msu })
            return;

        if (!File.Exists(msu.Path)) return;
        var directory = new FileInfo(msu.Path).DirectoryName;
        if (Directory.Exists(directory))
            Process.Start("explorer.exe", directory);
    }

    private void MsuListView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void FavoriteBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Msu msu, Content: StackPanel stackPanel})
            return;
        msu.Settings.IsFavorite = !msu.Settings.IsFavorite;
        foreach (var imageAwesome in stackPanel.Children.Cast<MaterialIcon>())
        {
            var tag = imageAwesome.Tag as string;
            if (tag == "True" && msu.Settings.IsFavorite || tag == "False" && !msu.Settings.IsFavorite)
            {
                imageAwesome.Visibility = Visibility.Visible;
            }
            else
            {
                imageAwesome.Visibility = Visibility.Collapsed;
            }
        }
        _msuUserOptionsService.SaveMsuSettings(msu);
    }

    private void MonitorWindowMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: Msu msu })
            return;

        if (!File.Exists(msu.Path)) return;
        OpenMsuMonitorWindow(msu);
    }

    private void ShuffleDefaultFrequencyMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: Msu msu, Parent: ContextMenu { PlacementTarget: Button { Content: StackPanel stackPanel} } } )
            return;

        if (msu.Settings.ShuffleFrequency == ShuffleFrequency.Default)
            return;
        
        msu.Settings.ShuffleFrequency = ShuffleFrequency.Default;
        UpdateFrequencyIcons(msu, stackPanel);
        _msuUserOptionsService.SaveMsuSettings(msu);
    }

    private void ShuffleMoreFrequentMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: Msu msu, Parent: ContextMenu { PlacementTarget: Button { Content: StackPanel stackPanel} } } )
            return;
        
        if (msu.Settings.ShuffleFrequency == ShuffleFrequency.MoreFrequent)
            return;
        
        msu.Settings.ShuffleFrequency = ShuffleFrequency.MoreFrequent;
        UpdateFrequencyIcons(msu, stackPanel);
        _msuUserOptionsService.SaveMsuSettings(msu);
    }

    private void ShuffleLessFrequentMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: Msu msu, Parent: ContextMenu { PlacementTarget: Button { Content: StackPanel stackPanel} } } )
            return;
        
        if (msu.Settings.ShuffleFrequency == ShuffleFrequency.LessFrequent)
            return;
        
        msu.Settings.ShuffleFrequency = ShuffleFrequency.LessFrequent;
        UpdateFrequencyIcons(msu, stackPanel);
        _msuUserOptionsService.SaveMsuSettings(msu);
    }

    private void UpdateFrequencyIcons(Msu msu, StackPanel stackPanel)
    {
        foreach (var imageAwesome in stackPanel.Children.Cast<MaterialIcon>())
        {
            if (!int.TryParse(imageAwesome.Tag as string, out var intValue))
            {
                break;
            }

            var enumValue = (ShuffleFrequency)intValue;

            imageAwesome.Visibility =
                enumValue == msu.Settings.ShuffleFrequency ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void FrequencyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.ContextMenu == null)
            return;
        
        ContextMenu contextMenu = button.ContextMenu;
        contextMenu.PlacementTarget = button;
        contextMenu.IsOpen = true;
        e.Handled = true;
    }
}