using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using Timer = System.Timers.Timer;

namespace MSURandomizerUI.Controls;

/// <summary>
/// Interaction logic for MSUContinuousShuffleWindow.xaml
/// </summary>
public partial class MsuContinuousShuffleWindow
{
    private readonly Timer _reshuffleTimer;
    private readonly IMsuSelectorService _msuSelectorService;
    private readonly MsuSelectorRequest _request;
    private Msu? _previousMsu;
    
    public MsuContinuousShuffleWindow(IMsuUserOptionsService msuUserOptionsService, IMsuSelectorService msuSelectorService, ILogger<MsuContinuousShuffleWindow> logger, IMsuLookupService msuLookupService, IMsuTypeService msuTypeService, MsuAppSettings appSettings)
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
        
        var msus = msuLookupService.Msus
            .Where(x => msuUserOptions.SelectedMsus?.Contains(x.Path) == true)
            .ToList();
        
        if (!msus.Any())
        {
            logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        var msuType = msuTypeService.GetMsuType(msuUserOptions.OutputMsuType);

        if (msuType == null)
        {
            logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }
        
        var outputPath = !string.IsNullOrEmpty(msuUserOptions.OutputFolderPath) 
            ? Path.Combine(msuUserOptions.OutputFolderPath, $"{msuUserOptions.Name}.msu") 
            : msuUserOptions.OutputRomPath;

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            logger.LogError("No output path");
            throw new InvalidOperationException("No output path");
        }

        _request = new MsuSelectorRequest()
        {
            Msus = msus,
            OutputMsuType = msuType,
            OutputPath = outputPath,
            AvoidDuplicates = msuUserOptions.AvoidDuplicates,
            OpenFolder = false
        };
            
        _reshuffleTimer = new Timer(TimeSpan.FromSeconds(appSettings.ContinuousReshuffleSeconds ?? 60));
        _reshuffleTimer.Elapsed += OnTimedEvent;
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
        _request.PrevMsu = _previousMsu;
        var response = _msuSelectorService.CreateShuffledMsu(_request);
        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            if (response.Successful)
            {
                _previousMsu = response.Msu;
                UpdateText($"Last Reshuffle: {DateTime.Now:h\\:mm tt}\nWarning: {response.Message}");
            }
            else
            {
                UpdateText($"Error: {response.Message}");
            }
        }
        else 
        {
            _previousMsu = response.Msu;
            UpdateText($"Last Reshuffle: {DateTime.Now:h\\:mm tt}");
        }
    }
}