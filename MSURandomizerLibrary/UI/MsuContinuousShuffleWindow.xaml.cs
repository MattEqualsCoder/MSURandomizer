using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizerLibrary.UI;

/// <summary>
/// Interaction logic for MSUContinuousShuffleWindow.xaml
/// </summary>
public partial class MsuContinuousShuffleWindow
{
    private const int Duration = 1 * 10 * 1000;
    private readonly Timer _reshuffleTimer;
    private readonly IMsuSelectorService _msuSelectorService;

    private readonly List<Msu> _msus;
    private Msu? _previousMsu;
    private readonly string? _outputPath;
    private readonly MsuType? _msuType;
    private readonly bool _avoidDuplicates;

    public MsuContinuousShuffleWindow(IMsuUserOptionsService msuUserOptionsService, IMsuSelectorService msuSelectorService, ILogger<MsuContinuousShuffleWindow> logger, IMsuLookupService msuLookupService, IMsuTypeService msuTypeService)
    {
        _msuSelectorService = msuSelectorService;

        InitializeComponent();

        var msuUserOptions = msuUserOptionsService.MsuUserOptions;
        
        if (msuUserOptions.SelectedMsus == null || !msuUserOptions.SelectedMsus.Any())
        {
            logger.LogError("No selected MSUs");
            throw new InvalidOperationException("No selected MSUs");
        }
        
        if (string.IsNullOrEmpty(msuUserOptions.OutputMsuType))
        {
            logger.LogError("No output MSU type selected");
            throw new InvalidOperationException("No output MSU type selected");
        }
        
        _msus = msuLookupService.Msus
            .Where(x => msuUserOptions.SelectedMsus?.Contains(x.Path) == true)
            .ToList();
        
        if (!_msus.Any())
        {
            logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        _msuType = msuTypeService.GetMsuType(msuUserOptions.OutputMsuType);

        if (_msuType == null)
        {
            logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }
        
        _outputPath = !string.IsNullOrEmpty(msuUserOptions.OutputFolderPath) 
            ? Path.Combine(msuUserOptions.OutputFolderPath, $"{msuUserOptions.Name}.msu") 
            : msuUserOptions.OutputRomPath;

        if (string.IsNullOrWhiteSpace(_outputPath))
        {
            logger.LogError("No output path");
            throw new InvalidOperationException("No output path");
        }

        _avoidDuplicates = msuUserOptions.AvoidDuplicates;
            
        _reshuffleTimer = new Timer(Duration);
        _reshuffleTimer.Elapsed += OnTimedEvent;
        _reshuffleTimer.Interval = Duration;
        _reshuffleTimer.Start();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        GenerateMsu();
    }

    private void UpdateText(string text)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            MessageTextBlock.Text = text;
        }));
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _reshuffleTimer.Stop();
    }

    private void MSUContinuousShuffleWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        GenerateMsu();
    }

    private void GenerateMsu()
    {
        UpdateText("Reshuffling...");
        _previousMsu = _msuSelectorService.CreateShuffledMsu(_msus, _msuType!, _outputPath!, _previousMsu, false,
            _avoidDuplicates);
        UpdateText($"Last Reshuffle: {DateTime.Now:h\\:mm tt}");
    }
}