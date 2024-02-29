using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.Views;

public partial class MsuWindow : RestorableWindow
{
    private MsuWindowService? _service;
    private MsuList _msuList;

    public MsuWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MsuWindowViewModel();
        }
        else
        {
            _service = IControlServiceFactory.GetControlService<MsuWindowService>();
            DataContext = _service.InitializeModel();
            _service.MsuMonitorStarted += (sender, args) =>
            {
                Dispatcher.UIThread.Invoke(Hide);
            };
            _service.MsuMonitorStopped += (sender, args) =>
            {
                Dispatcher.UIThread.Invoke(Show);
            };
        }

        _msuList = this.Find<MsuList>(nameof(MsuList))!;
        _msuList.SelectedMsusChanged += MsuList_OnSelectedMsusChanged;
    }

    private void MsuListOnSelectedMsusChanged(object? sender, SelectedMsusChangedEventArgs e)
    {
        _service?.OnSelectedMsusChanged(e.SelectedMsus);
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "main-window.json");
    protected override int DefaultWidth => 800;
    protected override int DefualtHeight => 600;

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _service?.FinishInitialization();
    }

    private void MsuTypeComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _service?.FilterMsuList(_msuList);
    }

    private void EnumComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _service?.FilterMsuList(_msuList);
    }

    private void SelectAllButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _msuList.SelectAll();
    }

    private void SelectNoneButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _msuList.SelectNone();
    }

    private void MsuList_OnSelectedMsusChanged(object? sender, SelectedMsusChangedEventArgs e)
    {
        _service?.UpdateSelectedMsus(e.SelectedMsus);
    }

    private void SettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog(this);
    }

    private void SelectMsuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var generationWindow = new MsuGenerationWindow();
        generationWindow.ShowDialog(this);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void RandomMsuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        
        var generationWindow = new MsuGenerationWindow();
        generationWindow.ShowDialog(this, MsuRandomizationStyle.Single, _service.Model.SelectedMsuType, _service.Model.SelectedMsus.Select(x => x.MsuPath).ToList());
        generationWindow.Closed += (o, args) =>
        {
            if (!generationWindow.DialogResult)
            {
                return;
            }
        
            GenerateMsu();
        };
    }

    private void ContinuousShuffleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        
        var generationWindow = new MsuGenerationWindow();
        generationWindow.ShowDialog(this, MsuRandomizationStyle.Continuous, _service.Model.SelectedMsuType, _service.Model.SelectedMsus.Select(x => x.MsuPath).ToList());
        generationWindow.Closed += (o, args) =>
        {
            if (!generationWindow.DialogResult)
            {
                return;
            }
        
            OpenMsuMonitorWindow(null);
        };
    }

    private void ShuffledMsuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        
        var generationWindow = new MsuGenerationWindow();
        generationWindow.ShowDialog(this, MsuRandomizationStyle.Shuffled, _service.Model.SelectedMsuType, _service.Model.SelectedMsus.Select(x => x.MsuPath).ToList());
        generationWindow.Closed += (o, args) =>
        {
            if (!generationWindow.DialogResult)
            {
                return;
            }
        
            GenerateMsu();
        };
    }

    private void GenerateMsu()
    {
        if (_service == null)
        {
            return;
        }
        
        var result = _service.GenerateMsu(out var error, out var openContinuousWindow, out var msu);
        if (result != true)
        {
            var errorWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = $"The following error happened while trying to generate the MSU: {error}",
                Icon = MessageWindowIcon.Error,
                Buttons = MessageWindowButtons.OK
            });
            errorWindow.ShowDialog(this);
        }
        else if (openContinuousWindow)
        {
            OpenMsuMonitorWindow(msu);
        }
    }

    private void OpenMsuMonitorWindow(Msu? msu)
    {
        var window = new MsuMonitorWindow();
        window.Show(msu);
    }
}