using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Extensions;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.Views;

public partial class MsuList : UserControl
{
    private readonly MsuListService? _service;
    private readonly MsuListViewModel _model;
    
    public MsuList()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = _model = new MsuListViewModel()
            {
                FilteredMsus = new List<MsuViewModel>()
                {
                    new()
                    {
                        MsuName = "Test MSU",
                        MsuPath = "/media/matt/Shared Data/SMZ3/msus/Fighting/FIGHTING-LTTP-MSU.msu",
                        MsuTypeName = "A Link to the Past",
                        MsuTrackCount = "61 Tracks",
                        DisplayPath = "Fighting/FIGHTING-LTTP-MSU.msu"
                    }
                }
            };
        }
        else
        {
            _service = this.GetControlService<MsuListService>();
            DataContext = _model = _service!.InitializeModel();
        }
    }

    public event EventHandler<SelectedMsusChangedEventArgs>? SelectedMsusChanged;
    
    public static readonly StyledProperty<List<MsuViewModel>?> SelectedMsusProperty = AvaloniaProperty.Register<MsuList, List<MsuViewModel>?>(
        nameof(SelectedMsus));

    public List<MsuViewModel>? SelectedMsus
    {
        get => GetValue(SelectedMsusProperty);
        set => SetValue(SelectedMsusProperty, value);
    }
    
    public static readonly StyledProperty<int?> SelectedMsuCountProperty = AvaloniaProperty.Register<MsuList, int?>(
        nameof(SelectedMsuCount));

    public int? SelectedMsuCount
    {
        get => GetValue(SelectedMsuCountProperty);
        set => SetValue(SelectedMsuCountProperty, value);
    }

    public void SetIsSingleSelectionMode(bool value)
    {
        _model.IsSingleSelectionMode = value;
    }

    public void FilterMSUs(MsuType msuType, MsuFilter msuFilter)
    {
        _service?.FilterMSUs(msuType, msuFilter);
    }

    public void SelectAll()
    {
        _service?.SelectAll();
    }
    
    public void SelectNone()
    {
        _service?.SelectNone();
    }

    public void PopulateMsuViewModels(List<Msu>? msus)
    {
        _service?.PopulateMsuViewModels(msus);
        SelectedMsus = _service?.Model.SelectedMsus;
        SelectedMsuCount = SelectedMsus?.Count;
        SelectedMsusChanged?.Invoke(this, new SelectedMsusChangedEventArgs(SelectedMsus));
    }

    public void ToggleHardwareMode(bool isEnabled)
    {
        _service?.ToggleHardwareMode(isEnabled);
    }
    
    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedMsus = _service?.Model.SelectedMsus;
        SelectedMsuCount = SelectedMsus?.Count;
        SelectedMsusChanged?.Invoke(this, new SelectedMsusChangedEventArgs(SelectedMsus));
    }

    private void FavoriteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: MsuViewModel model })
        {
            return;
        }
        _service?.ToggleFavorite(model);
    }

    private void ShuffleDefaultMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MsuViewModel model })
        {
            return;
        }
        _service?.UpdateFrequency(model, ShuffleFrequency.Default);
    }

    private void ShuffleMoreMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MsuViewModel model })
        {
            return;
        }
        _service?.UpdateFrequency(model, ShuffleFrequency.MoreFrequent);
    }

    private void ShuffleLessMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MsuViewModel model })
        {
            return;
        }
        _service?.UpdateFrequency(model, ShuffleFrequency.LessFrequent);
    }

    private void ShuffleFrequencyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var contextMenu = button.ContextMenu;
        if (contextMenu == null)
        {
            return;
        }
        
        contextMenu.PlacementTarget = button;
        contextMenu.Open();
        e.Handled = true;
    }

    private void OpenDetailsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MsuViewModel model })
        {
            return;
        }

        var window = new MsuDetailsWindow();
        window.Show(model, TopLevel.GetTopLevel(this) as Window);
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MsuViewModel model })
        {
            return;
        }
        
        _service?.OpenMsuFolder(model);
    }

    private void OpenMonitorWindowMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MsuViewModel model })
        {
            return;
        }
        
        var window = new MsuMonitorWindow();
        window.Show(model.Msu, _model.MsuType);
    }
}