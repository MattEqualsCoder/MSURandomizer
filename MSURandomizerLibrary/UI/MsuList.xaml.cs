using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.UI;

/// <summary>
/// Interaction logic for MSURandomizerControl.xaml
/// </summary>
public partial class MsuList : UserControl
{
    public new readonly MsuListViewModel DataContext;

    private ILogger _logger;
    
    public MsuList(MsuListViewModel viewModel, ILogger logger)
    {
        _logger = logger;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.MsuListUpdated += ViewModelOnMsuListUpdated;
        viewModel.AvailableMsusUpdated += ViewModelOnAvailableMsusUpdated;
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
        MsuListView.ItemsSource = DataContext.AvailableMsus;
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
        SelectedMsusUpdated?.Invoke(this, new MsuListEventArgs(MsuListView.SelectedItems.Cast<Msu>().ToList()));
    }
}