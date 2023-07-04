using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary;

/// <summary>
/// Interaction logic for MSURandomizerControl.xaml
/// </summary>
public partial class MsuList : UserControl
{
    private MsuListViewModel _viewModel;
    private bool _previouslyLoaded;

    public MsuList(MsuListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;
        viewModel.MsuListUpdated += ViewModelOnMsuListUpdated;
    }

    private void ViewModelOnMsuListUpdated(object? sender, MsuLookupEventArgs e)
    {
        if (_previouslyLoaded) return;
        _previouslyLoaded = true;
        MsuListView.UnselectAll();
        foreach (var msu in MsuListView.Items.Cast<Msu>())
        {
            if (_viewModel.SelectedMsuPaths.Contains(msu?.Path ?? ""))
            {
                MsuListView.SelectedItems.Add(msu);
            }
        }
    }

    public SelectionMode SelectionMode
    {
        get => _viewModel.SelectionMode;
        set => _viewModel.SelectionMode = value;
    }

    public MsuType TargetMsuType
    {
        get => _viewModel.MsuType;
        set => _viewModel.MsuType = value;
    }
        
    public MsuFilter MsuFilter
    {
        get => _viewModel.MsuFilter;
        set => _viewModel.MsuFilter = value;
    }

    public ICollection<string> SelectedMsuPaths
    {
        get => MsuListView.SelectedItems.Cast<Msu>().Select(x => x.Path).ToList();
        set => _viewModel.SelectedMsuPaths = value;
    }

    public ICollection<Msu> SelectedMsus => MsuListView.SelectedItems.Cast<Msu>().ToList();

    private void MsuList_OnLoaded(object sender, RoutedEventArgs e)
    {
    }
}