using MSURandomizerLibrary.MSUTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MSURandomizerLibrary
{
    public class MSURandomizerViewModel: INotifyPropertyChanged
    {
        private ICollection<MSU> _msus = new List<MSU>();
        private ICollection<string> _msuTypes = new List<string>();

        public MSURandomizerOptions Options { get; set; } = new();

        public ICollection<MSU> MSUs
        {
            get => _msus;
            set
            {
                _msus = value;
                OnPropertyChanged(nameof(VisibleMSUs));
            }
        }
        
        public ICollection<string> MSUTypes
        {
            get => _msuTypes;
            set
            {
                _msuTypes = value;
                OnPropertyChanged(nameof(SelectedMsuType));
                OnPropertyChanged(nameof(MSUTypes));
                OnPropertyChanged(nameof(VisibleMSUs));
            }
        }
        
        public string? SelectedMsuType
        {
            get => Options.OutputType;
            set
            {
                Options.OutputType = value;
                OnPropertyChanged(nameof(SelectedMsuType));
                OnPropertyChanged(nameof(VisibleMSUs));
            }
        }
        
        public MSUFilter Filter
        {
            get => Options.Filter;
            set
            {
                Options.Filter = value;
                OnPropertyChanged(nameof(Filter));
                OnPropertyChanged(nameof(VisibleMSUs));
            }
        }

        public IEnumerable<MSU> VisibleMSUs => MSURandomizerService.ApplyFilter(MSUs, Options).OrderBy(x => x.Name);

        private bool _canGenerate;

        public bool CanGenerate
        {
            get => _canGenerate;
            set
            {
                _canGenerate = value;
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
