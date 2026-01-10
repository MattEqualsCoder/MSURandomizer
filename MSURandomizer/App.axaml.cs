using System.Runtime.Versioning;
using AppImageManager;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Controls;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;

namespace MSURandomizer;

public class App : Application
{
    public const string AppId = "org.mattequalscoder.msurandomizer";
    public const string AppName = "MSU Randomizer";
    
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
    
    [SupportedOSPlatform("linux")]
    internal static CreateDesktopFileResponse BuildLinuxDesktopFile()
    {
        return new DesktopFileBuilder(AppId, AppName)
            .AddUninstallAction(Directories.AppDataFolder)
            .Build();
    }
}