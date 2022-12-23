using System.Windows;

namespace MSURandomizerLibrary
{
    /// <summary>
    /// Interaction logic for GenerateOptionsWindow.xaml
    /// </summary>
    public partial class MSUCreateWindow : Window
    {
        public bool Generate = false;

        public MSUCreateWindow(MSURandomizerOptions options)
        {
            InitializeComponent();
            DataContext = options;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Generate = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
