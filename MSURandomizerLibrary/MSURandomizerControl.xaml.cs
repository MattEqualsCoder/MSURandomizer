using System;
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
            var generateOptionsWindow = new MSUCreateWindow(Options);
            generateOptionsWindow.ShowDialog();
            if (!generateOptionsWindow.Generate) return;
            Options.SelectedMSUs = MSUListView.SelectedItems.Cast<MSU>().Select(x => x.Name).ToList();
            if (!MSURandomizerService.RandomizeMSU(Options, out var error))
            {
                
            }
            else
            {
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
            
            var msuTypes = MSURandomizerService.GetMSUTypes();
            if (string.IsNullOrEmpty(Options.OutputType) || msuTypes.All(x => x.Name != Options.OutputType))
                Options.OutputType = msuTypes.First().Name;
            _viewModel.MSUTypes = msuTypes.Select(x => x.Name).ToList();
            
            if (Options.SelectedMSUs == null) return;
            foreach (var selectedMSUName in Options.SelectedMSUs)
            {
                MSUListView.SelectedItems.Add(msus.First(x => x.Name == selectedMSUName));
            }
        }
    }
}
