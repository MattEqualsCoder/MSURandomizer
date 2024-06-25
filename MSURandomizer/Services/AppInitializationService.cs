using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AvaloniaControls.Controls;
using GitHubReleaseChecker;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class AppInitializationService(
    ILogger<AppInitializationService> logger,
    IMsuRandomizerInitializationService msuRandomizerInitializationService,
    IGitHubReleaseService gitHubReleaseService,
    IGitHubReleaseCheckerService gitHubReleaseCheckerService,
    IMsuUserOptionsService msuUserOptionsService,
    IMsuGameService msuGameService)
{
    public void Initialize(string[] args)
    {
        if (!IsEnabled)
        {
            return;
        }
        
        msuRandomizerInitializationService.Initialize(new MsuRandomizerInitializationRequest()
        {
            InitializeAppSettings = true,
            InitializeMsuTypes = false,
            InitializeCache = false,
            InitializeUserOptions = true,
            LookupMsus = false,
        });

        ScalableWindow.GlobalScaleFactor = msuUserOptionsService.MsuUserOptions.UiScaling;

        var initPassedRomArgument = msuUserOptionsService.MsuUserOptions.PassedRomArgument;
        var initOutputRomPath = msuUserOptionsService.MsuUserOptions.OutputRomPath;
        msuUserOptionsService.MsuUserOptions.PassedRomArgument = false;
        
        if (args.Length > 0 && !string.IsNullOrEmpty(args.Last()))
        {
            var file = new FileInfo(args.Last());
            if (file.Exists && file.Extension.ToLower() is ".sfc" or ".smc" or ".gb" or ".gbc")
            {
                msuUserOptionsService.MsuUserOptions.OutputFolderPath = null;
                msuUserOptionsService.MsuUserOptions.OutputRomPath = file.FullName;
                msuUserOptionsService.MsuUserOptions.PassedRomArgument = true;
            }
        }

        if (msuUserOptionsService.MsuUserOptions.PassedRomArgument != initPassedRomArgument || msuUserOptionsService.MsuUserOptions.OutputRomPath != initOutputRomPath)
        {
            msuUserOptionsService.Save();
        }
    }

    public bool IsEnabled { get; set; } = true;

    public void FinishInitialization()
    {
        if (!IsEnabled)
        {
            IsLoading = false;
            return;
        }
        
        Task.Run(() =>
        {
            _ = VerifyVersionNumber();
            msuRandomizerInitializationService.Initialize(new MsuRandomizerInitializationRequest()
            {
                InitializeAppSettings = false,
                InitializeMsuTypes = true,
                InitializeCache = true,
                InitializeUserOptions = true,
                LookupMsus = true
            });
            msuGameService.InstallLuaScripts();
        });
    }

    public GitHubRelease? LatestFullRelease { get; private set; }
    public GitHubRelease? LatestPreRelease { get; private set; }
    public bool IsLoading { get; private set; }

    public event EventHandler? InitializationComplete;

    private async Task VerifyVersionNumber()
    {
        if (!msuUserOptionsService.MsuUserOptions.PromptOnUpdate)
        {
            return;
        }
        
        var version = "10.0.0";//FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "";
        logger.LogInformation("Starting MSU Randomizer {Version}", version);
        
        var gitHubReleases = await gitHubReleaseService.GetReleasesAsync("MattEqualsCoder", "MSURandomizer");
        if (gitHubReleases == null)
        {
            return;
        }

        var releasesInOrder = gitHubReleases.OrderByDescending(x => x.Created).ToList();
        
        var latestRelease = releasesInOrder.Where(x => !x.PreRelease).MaxBy(x => x.Created);
        if (latestRelease != null && gitHubReleaseCheckerService.IsCurrentVersionOutOfDate(version, latestRelease.Tag))
        {
            LatestFullRelease = latestRelease;
        }
        
        latestRelease = releasesInOrder.Where(x => x.PreRelease).MaxBy(x => x.Created);
        if (latestRelease != null && gitHubReleaseCheckerService.IsCurrentVersionOutOfDate(version, latestRelease.Tag))
        {
            LatestPreRelease = latestRelease;
        }
        
        IsLoading = false;
        InitializationComplete?.Invoke(this, EventArgs.Empty);
        
        logger.LogInformation("Latest Full Release: {Full} | Latest Pre Release {Pre}", LatestFullRelease?.Tag, LatestPreRelease?.Tag);
    }
}