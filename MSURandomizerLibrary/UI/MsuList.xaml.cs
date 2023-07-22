using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibrary.UI;

/// <summary>
/// Interaction logic for MSURandomizerControl.xaml
/// </summary>
public partial class MsuList : UserControl
{
    private readonly IMsuUiFactory _msuUiFactory;
    private readonly List<string> _displayedErrors = new();

    public MsuList(IMsuUiFactory msuUiFactory)
    {
        _msuUiFactory = msuUiFactory;
        DataContext = new MsuListViewModel();
        InitializeComponent();
    }
    
    public new MsuListViewModel DataContext { get; private set; }

    public void SetDataContext(MsuListViewModel model)
    {
        DataContext = model;
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
        MsuListView.ItemsSource = DataContext.AvailableMsus.OrderBy(x => x.Name);
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

    public void SelectAll() => MsuListView.SelectAll();

    public void UnselectAll() => MsuListView.UnselectAll();

    public SelectionMode SelectionMode
    {
        get => DataContext.SelectionMode;
        set => DataContext.SelectionMode = value;
    }

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

    public ICollection<string> SelectedMsuPaths
    {
        get => MsuListView.SelectedItems.Cast<Msu>().Select(x => x.Path).ToList();
        set => DataContext.SelectedMsuPaths = value;
    }
    
    public string? BasePath
    {
        get => DataContext.BasePath;
        set => DataContext.BasePath = value;
    }

    public ICollection<Msu> SelectedMsus => MsuListView.SelectedItems.Cast<Msu>().ToList();
    
    public event EventHandler<MsuListEventArgs>? SelectedMsusUpdated;

    private void MsuList_OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void MsuListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedMsusUpdated?.Invoke(this, new MsuListEventArgs(MsuListView.SelectedItems.Cast<Msu>().ToList(), null));
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    { 
        if (sender is not MenuItem { Tag: Msu msu })
            return;

        _msuUiFactory.OpenMsuDetailsWindow(msu);
    }

    private void MsuListView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}