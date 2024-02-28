using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using MSURandomizerCrossPlatform.Services;
using MSURandomizerCrossPlatform.ViewModels;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerCrossPlatform.Views;

public partial class MsuDetailsWindow : ScalableWindow
{
    private readonly MsuDetailsService? _service;
    
    public MsuDetailsWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MsuDetailsWindowViewModel()
            {
                Name = "Test MSU",
                Creator = "Test MSU Creator",
                MsuPath = "/home/matt/Documents/Test",
                Tracks = new List<MsuTrackViewModel>()
                {
                    new MsuTrackViewModel() 
                    {
                        TrackNumber = 1,
                        TrackName = "Test Track",
                        Songs = new List<MsuSongViewModel>()
                        {
                            new MsuSongViewModel(new Track()
                            {
                                TrackName = "Test Track Name Alt",
                                Artist = "Test Artist",
                                IsAlt = true,
                                Path = "/home/matt/Documents/test.json"
                            }),
                            new MsuSongViewModel(new Track()
                            {
                                TrackName = "Test Track Name",
                                Artist = "Test Artist",
                                Path = "/home/matt/Documents/test.json"
                            })
                        }
                    },
                }
            };
            return;
        }

        _service = ControlServiceFactory.GetControlService<MsuDetailsService>();
    }

    public void Show(MsuViewModel model, Window? parentWindow)
    {
        Owner = parentWindow;
        DataContext = _service?.InitilizeModel(model);
        if (parentWindow == null)
        {
            base.Show();
        }
        else
        {
            base.ShowDialog(parentWindow);
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
}