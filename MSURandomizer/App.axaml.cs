using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Controls;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;
using MSURandomizer.ViewModels;

namespace MSURandomizer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && Program.MainHost != null)
        {
            var msuWindow = Program.MainHost.Services.GetRequiredService<MsuWindow>();
            MessageWindow.GlobalParentWindow = msuWindow;
            desktop.MainWindow = msuWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}