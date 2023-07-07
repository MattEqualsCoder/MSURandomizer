using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MsuRandomizer;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Services;

namespace MSURandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public new readonly MsuRandomizerOptions DataContext;
        
        public MainWindow(IMsuUiFactory uiFactory, IMsuTypeService msuTypeService, MsuRandomizerOptions options)
        {
            DataContext = options;
            InitializeComponent();
            MsuList = uiFactory.CreateMsuList(msuTypeService.MsuTypes.First(x => x.Name == "Super Metroid"), MsuFilter.Compatible, SelectionMode.Multiple);
            if (MsuList != null)
            {
                MainGrid.Children.Add(MsuList);    
            }
        }
        
        private static string GetConfigPath()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MSURandomizer");
            Directory.CreateDirectory(basePath);
            return Path.Combine(basePath, "options.yml");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            /*MsuRandomizerControl.Options = _viewModel.Options;
            MsuRandomizerControl.OnRomGenerated += (o, args) => SaveOptions(args.Options);
            MsuRandomizerControl.OnSettingsUpdated += (o, args) => SaveOptions(args.Options);*/
        }

        private void RandomMSUButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MsuList == null) return;
            var msus = MsuList.SelectedMsus;
            var window = new MsuCreateWindow(DataContext, false);
            if (window.ShowDialog() != true) return;
            var msuType = MsuRandomizerService.Instance.GetMsuType("Super Metroid");
            MsuRandomizerService.Instance.PickRandomMsu(msus, msuType!, DataContext);
        }

        public MsuList? MsuList
        {
            get;
            set;
        }

        private void GeneratedButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MsuList == null) return;
            var msus = MsuList.SelectedMsus;
            var window = new MsuCreateWindow(DataContext, true);
            if (window.ShowDialog() != true) return;
            var msuType = MsuRandomizerService.Instance.GetMsuType("Super Metroid");
            MsuRandomizerService.Instance.CreateShuffledMsu(msus, msuType!, DataContext);
        }

        private void OptionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new MsuOptionsWindow(DataContext);
            if (window.ShowDialog() != true) return;
            DataContext.SaveOptions();
        }
    }
}
