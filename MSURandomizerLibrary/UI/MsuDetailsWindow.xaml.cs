using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MSURandomizerLibrary.UI;

public partial class MsuDetailsWindow : Window, INotifyPropertyChanged
{
    public MsuDetailsWindow()
    {
        InitializeComponent();
    }
    
    private MsuDetailsViewModel _viewModel = new();

    public MsuDetailsViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            DataContext = value;
            SetField(ref _viewModel, value);   
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.ToString(),
            UseShellExecute = true
        });
    }

    private void UpdateMsuButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void NameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.CheckChanges(name: NameTextBox.Text);
    }
    
    private void CreatorTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.CheckChanges(creator: CreatorTextBox.Text);
    }
}