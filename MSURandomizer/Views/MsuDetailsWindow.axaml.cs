using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Extensions;
using AvaloniaControls.Services;
using MSURandomizer.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.Views;

public partial class MsuDetailsWindow : ScalableWindow
{
    private readonly MsuDetailsService? _service;
    private MsuDetailsWindowViewModel _model = new();
    
    public MsuDetailsWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = _model = new MsuDetailsWindowViewModel
            {
                Name = "Test MSU",
                Creator = "Test MSU Creator",
                MsuPath = "/home/matt/Documents/Test",
                Tracks =
                [
                    new MsuTrackViewModel
                    {
                        TrackNumber = 1,
                        TrackName = "Test Track",
                        Display = true,
                        Songs =
                        [
                            new MsuSongViewModel(new Track
                            {
                                TrackName = "Test Track Name Alt",
                                Artist = "Test Artist",
                                IsAlt = true,
                                Path = "/home/matt/Documents/test.json",
                                IsCopyrightSafe = true,
                                IsCopyrightSafeOverride = null
                            }),

                            new MsuSongViewModel(new Track
                            {
                                TrackName = "Test Track Name",
                                Artist = "Test Artist",
                                Path = "/home/matt/Documents/test.json",
                                IsCopyrightSafe = true,
                                IsCopyrightSafeOverride = false
                            })
                        ]
                    }

                ]
            };
            return;
        }

        _service = this.GetControlService<MsuDetailsService>();
    }

    public void Show(MsuViewModel model, Window? parentWindow)
    {
        Owner = parentWindow;
        DataContext = _model = _service?.InitilizeModel(model) ?? new MsuDetailsWindowViewModel();
        if (parentWindow == null)
        {
            base.Show();
        }
        else
        {
            ShowDialog(parentWindow);
        }

        Title = $"{_service?.Model.DefaultMsuName} MSU Details";
    }

    private void TrackLinkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not LinkControl { Tag: string url } || string.IsNullOrWhiteSpace(url) )
        {
            return;
        }

        CrossPlatformTools.OpenUrl(url);
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.Save();
        Close();
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CopyrightOverrideUndoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is MsuSongViewModel song)
        {
            song.IsCopyrightSafeOverrideValue = null;
        }
        else
        {
            foreach (var trackSong in _model.Tracks.SelectMany(x => x.Songs))
            {
                trackSong.IsCopyrightSafeOverrideValue = null;
            }
        }
        
        _model.UpdateCopyrightOptions();
    }

    private void CopyrightCheckboxButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }
        
        if (button.Tag is MsuSongViewModel song)
        {
            if (song.IsCopyrightSafeCombined == null)
            {
                song.IsCopyrightSafeOverrideValue = true;
            }
            else
            {
                song.IsCopyrightSafeOverrideValue = !song.IsCopyrightSafeCombined;
            }
        }
        else
        {
            var newValue = !_model.AreAllCopyrightSafe;
            foreach (var trackSong in _model.Tracks.SelectMany(x => x.Songs))
            {
                trackSong.IsCopyrightSafeOverrideValue = newValue;
            }
        }
        
        _model.UpdateCopyrightOptions();
    }
}