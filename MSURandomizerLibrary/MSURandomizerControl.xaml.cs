using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MSURandomizerLibrary
{
    /// <summary>
    /// Interaction logic for MSURandomizerControl.xaml
    /// </summary>
    public partial class MSURandomizerControl : UserControl
    {
        private MSURandomizerViewModel _viewModel;

        public MSURandomizerControl()
        {
            InitializeComponent();
            DataContext = _viewModel = new MSURandomizerViewModel();
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(MSURandomizerOptions), typeof(MSURandomizerControl), new PropertyMetadata(new MSURandomizerOptions()));

        public event EventHandler<MSURandomizerEventArgs>? OnRomGenerated;

        public event EventHandler<MSURandomizerEventArgs>? OnSettingsUpdated;

        public MSURandomizerOptions Options
        {
            get => (MSURandomizerOptions)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        private void MSURandomizerControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Options = Options;
            if (string.IsNullOrEmpty(Options.Directory))
                OpenOptionsWindow();
            else
                Reload();
        }

        private void GeneratedButton_Click(object sender, RoutedEventArgs e)
        {
            var generateOptionsWindow = new MSUCreateWindow(Options, true);
            generateOptionsWindow.ShowDialog();
            if (!generateOptionsWindow.Generate) return;
            Options.SelectedMSUs = MSUListView.SelectedItems.Cast<MSU>().Select(x => x.FileName).ToList();
            var romPath = Options.RomPath;
            if (!MSURandomizerService.ShuffleMSU(Options, out var error))
            {
                MessageBox.Show(error, "Error");
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Warning");
                }
                OnRomGenerated?.Invoke(this, new MSURandomizerEventArgs(Options));
                if (Options.ContinuousReshuffle)
                {
                    var continuousReshuffleWindow = new MSUContinuousShuffleWindow(Options, romPath);
                    continuousReshuffleWindow.ShowDialog();
                }
            }
        }
        
        private void RandomMSUButton_Click(object sender, RoutedEventArgs e)
        {
            var generateOptionsWindow = new MSUCreateWindow(Options, false);
            generateOptionsWindow.ShowDialog();
            if (!generateOptionsWindow.Generate) return;
            Options.SelectedMSUs = MSUListView.SelectedItems.Cast<MSU>().Select(x => x.FileName).ToList();
            if (!MSURandomizerService.PickRandomMSU(Options, out var error))
            {
                MessageBox.Show(error, "Error");
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Warning");
                }
                OnRomGenerated?.Invoke(this, new MSURandomizerEventArgs(Options));
            }
        }

        private void MSUListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.CanGenerate = MSUListView.SelectedItems.Count > 0;
        }

        private void SelectNoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            MSUListView.UnselectAll();
        }

        private void SelectAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            MSUListView.SelectAll();
            MSUListView.Focus();
        }

        private void OptionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenOptionsWindow();
        }

        private void OpenOptionsWindow()
        {
            var initDirectory = Options.Directory;
            var optionsWindow = new MSUOptionsWindow(Options);
            optionsWindow.ShowDialog();
            OnSettingsUpdated?.Invoke(this, new MSURandomizerEventArgs(Options));
            _viewModel.UpdateDisplayNames();
            if (initDirectory == Options.Directory) return;
            Reload();
        }

        private void Reload()
        {
            var msus = MSURandomizerService.GetMSUs(Options, out var error);
            if (msus == null)
            {
                return;
            }
            _viewModel.MSUs = msus;
            
            var msuTypes = MSURandomizerService.GetMSUTypes(Options);

            if (string.IsNullOrEmpty(Options.ForcedMsuType))
            {
                if (string.IsNullOrEmpty(Options.OutputType) || msuTypes.All(x => x.Name != Options.OutputType))
                    Options.OutputType = msuTypes.First().Name;
                _viewModel.MSUTypes = msuTypes.Select(x => x.Name).ToList();
            }
            else
            {
                Options.OutputType = Options.ForcedMsuType;
                _viewModel.MSUTypes = new List<string> { Options.ForcedMsuType };
            }
            
            if (Options.SelectedMSUs == null) return;
            foreach (var selectedMSUName in Options.SelectedMSUs)
            {
                var msu = msus.FirstOrDefault(x => x.FileName == selectedMSUName);
                if (msu != null) MSUListView.SelectedItems.Add(msu);
            }
        }
    }
}
