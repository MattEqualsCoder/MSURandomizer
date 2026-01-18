using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AppImageManager;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using AvaloniaControls.Models;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizer.Views;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;

namespace MSURandomizer.Services;

public class MsuWindowService(ILogger<MsuWindowService> logger, 
    AppInitializationService appInitializationService, 
    IMsuTypeService msuTypeService,
    IMsuUserOptionsService userOptions,
    IMsuAppSettingsService appSettings,
    IMsuSelectorService msuSelectorService,
    IMsuLookupService msuLookupService,
    IMsuMonitorService msuMonitorService,
    IRomLauncherService romLauncherService,
    IRomCopyService romCopyService) : ControlService
{
    public MsuWindowViewModel Model { get; set; } = new();
    
    public string RestoreFilePath =>
        Path.Combine(appSettings.MsuAppSettings.SaveDataDirectory.ExpandSpecialFolders(), "main-window.json");
    
    public event EventHandler? MsuMonitorStarted;

    public event EventHandler? MsuMonitorStopped;

    public MsuWindowViewModel InitializeModel()
    {
        msuLookupService.OnMsuLookupStarted += (_, _) =>
        {
            Model.AreMsusLoading = true;
        };
        
        msuLookupService.OnMsuLookupComplete += (_, _) =>
        {
            Model.AreMsusLoading = false;
        };

        msuMonitorService.MsuMonitorStarted += (_, _) =>
        {
            Model.IsMsuMonitorActive = true;
            MsuMonitorStarted?.Invoke(this, EventArgs.Empty);
        };
        
        msuMonitorService.MsuMonitorStopped += (_, _) =>
        {
            Model.IsMsuMonitorActive = false;
            MsuMonitorStopped?.Invoke(this, EventArgs.Empty);
        };
        
        appInitializationService.InitializationComplete += AppInitializationServiceOnInitializationComplete;
            
        var settings = appSettings.MsuAppSettings;
        Model.CanDisplayRandomMsuButton = settings.MsuWindowDisplayRandomButton == true;
        Model.CanDisplayShuffledMsuButton = settings.MsuWindowDisplayShuffleButton == true;
        Model.CanDisplayContinuousShuffleButton = settings.MsuWindowDisplayContinuousButton == true;
        Model.CanDisplaySelectMsuButton = settings.MsuWindowDisplaySelectButton == true;
        Model.CanDisplayCancelButton = settings.MsuWindowDisplaySelectButton == true;
        Model.CanDisplayUploadButton = settings.MsuWindowDisplayUploadButton == true;
        Model.IsHardwareModeButtonVisible = !appSettings.MsuAppSettings.DisableHardwareMode;
        Model.MsuWindowDisplayOptionsButton = appSettings.MsuAppSettings.MsuWindowDisplayOptionsButton != false;
        Model.HasMsuFolder = Model.MsuWindowDisplayOptionsButton && userOptions.MsuUserOptions.HasMsuFolder();
        Model.AreMsusLoading = msuLookupService.Status is MsuLoadStatus.Default or MsuLoadStatus.Loading;
        
        if (!string.IsNullOrEmpty(settings.ForcedMsuType))
        {
            Model.SelectedMsuType = settings.ForcedMsuType;
            Model.DisplayMsuTypeComboBox = false;
            Model.FilterColumnIndex = 0;
        }

        Model.DisplaySettingsWindowOnLoad = Model is { MsuWindowDisplayOptionsButton: true, HasMsuFolder: false };

        if (OperatingSystem.IsLinux() && Model.MsuWindowDisplayOptionsButton && !userOptions.MsuUserOptions.SkipDesktopFile &&
            !AppImage.DoesDesktopFileExist(App.AppId))
        {
            Model.DisplayDesktopPopupOnLoad = true;
        }
        
        return Model;
    }

    public void HandleUserDesktopResponse(bool addDesktopFile)
    {
        if (addDesktopFile && OperatingSystem.IsLinux())
        {
            App.BuildLinuxDesktopFile();
        }
        else
        {
            userOptions.MsuUserOptions.SkipDesktopFile = true;
            userOptions.Save();
        }
    }

    private void AppInitializationServiceOnInitializationComplete(object? sender, EventArgs e)
    {
        if (appInitializationService.LatestFullRelease == null)
        {
            return;
        }
        
        var downloadUrl = appInitializationService.ReleaseDownloadUrl;
        var hasDownloadUrl = !string.IsNullOrEmpty(downloadUrl);
        var canDownload = hasDownloadUrl && (OperatingSystem.IsWindows() || OperatingSystem.IsLinux());

        Dispatcher.UIThread.Invoke(async () =>
        {
            var messageWindow = new MessageWindow(new MessageWindowRequest
            {
                Title = $"MSU Randomizer {appInitializationService.LatestFullRelease.Tag}",
                Message = "A new version of the MSU Randomizer has been released.",
                LinkText = "Go to the release page on GitHub",
                LinkUrl = appInitializationService.LatestFullRelease.Url,
                Icon = MessageWindowIcon.Info,
                CheckBoxText = "Do not check for updates",
                Buttons = canDownload ? MessageWindowButtons.YesNo : MessageWindowButtons.OK,
                PrimaryButtonText = canDownload ? "Download Update" : "OK",
                SecondaryButtonText = "Close"
            });
            await messageWindow.ShowDialog(MessageWindow.GlobalParentWindow!);
            
            if (messageWindow.DialogResult?.CheckedBox == true)
            {
                userOptions.MsuUserOptions.PromptOnUpdate = false;
                userOptions.Save();    
            }

            if (canDownload && messageWindow.DialogResult?.PressedAcceptButton == true)
            {
                string? error;
                
                if (OperatingSystem.IsLinux())
                {
                    error = await DownloadLinuxVersion(downloadUrl!);
                }
                else if (OperatingSystem.IsWindows())
                {
                    error = await DownloadWindowsVersion(downloadUrl!);
                }
                else
                {
                    error = "Download functionality is only available on Windows and Linux";
                }

                if (!string.IsNullOrEmpty(error))
                {
                    await MessageWindow.ShowErrorDialog(error, "Update Failed", MessageWindow.GlobalParentWindow);
                }
            }
        });
        
    }

    [System.Runtime.Versioning.SupportedOSPlatform("Linux")]
    private async Task<string?> DownloadLinuxVersion(string url)
    {
        var downloadResult = await AppImage.DownloadAsync(new DownloadAppImageRequest
        {
            Url = url
        });
                    
        if (downloadResult.Success)
        {
            MessageWindow.GlobalParentWindow!.Close();
            return null;
        }
        else if (downloadResult.DownloadedSuccessfully)
        {
            return "AppImage was downloaded, but it could not be launched.";
        }
        else
        {
            return "Failed downloading AppImage";
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("Windows")]
    private async Task<string?> DownloadWindowsVersion(string url)
    {
        var filename = Path.GetFileName(new Uri(url).AbsolutePath);
        var localPath = Path.Combine(Path.GetTempPath(), filename);

        logger.LogInformation("Downloading {Url} to {LocalPath}", url, localPath);

        var response = await DownloadFileAsyncAttempt(url, localPath);

        if (!response.Item1)
        {
            logger.LogInformation("Download failed: {Error}", response.Item2);
            return response.Item2;
        }

        try
        {
            logger.LogInformation("Launching setup file");

            var psi = new ProcessStartInfo
            {
                FileName = localPath,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                CreateNoWindow = true
            };

            Process.Start(psi);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to download and install update");
            return "Failed to start setup file";
        }
    }
    
    private static async Task<(bool, string?)> DownloadFileAsyncAttempt(string url, string target, int attemptNumber = 0, int totalAttempts = 3)
    {
        using var httpClient = new HttpClient();

        try
        {
            await using var downloadStream = await httpClient.GetStreamAsync(url);
            await using var fileStream = new FileStream(target, FileMode.Create);
            await downloadStream.CopyToAsync(fileStream);
            return (true, null);
        }
        catch (Exception ex)
        {
            if (attemptNumber < totalAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(attemptNumber));
                return await DownloadFileAsyncAttempt(url, target, attemptNumber + 1, totalAttempts);
            }
            else
            {
                return (false, $"Download failed: {ex.Message}");
            }
        }
    }

    public void OnSelectedMsusChanged(ICollection<MsuViewModel>? msus)
    {
        Model.MsuCount = msus?.Count ?? 0;
    }

    public void FinishInitialization()
    {
        msuTypeService.OnMsuTypeLoadComplete += MsuTypeServiceOnOnMsuTypeLoadComplete;
        logger.LogInformation("Finish Init");
        appInitializationService.FinishInitialization();
    }

    public void FilterMsuList(MsuList msuList)
    {
        if (string.IsNullOrEmpty(Model.SelectedMsuType))
        {
            return;
        }
        
        var msuType = msuTypeService.GetMsuType(Model.SelectedMsuType);
        if (msuType == null)
        {
            logger.LogWarning("Invalid MSU type selected: {Selection}", Model.SelectedMsuType);
            return;
        }
        msuList.FilterMSUs(msuType, Model.Filter);
    }

    public void UpdateSelectedMsus(ICollection<MsuViewModel>? msus)
    {
        Model.MsuCount = msus?.Count ?? 0;
        Model.SelectedMsus = msus ?? new List<MsuViewModel>();
    }

    public void UpdateHardwareMode(MsuList msuList, List<Msu>? msus)
    {
        logger.LogInformation("Hardware mode MSU list updating with {Count} msus", msus?.Count ?? 0);
        Model.Filter = MsuFilter.Compatible;
        Model.IsHardwareModeEnabled = msus?.Count > 0;
        msuList.ToggleHardwareMode(Model.IsHardwareModeEnabled);
        msuList.PopulateMsuViewModels(msus);
        logger.LogInformation("Hardware mode MSU list updated");
    }

    public async Task UploadMsu(MsuWindow msuWindow, MsuList msuList)
    {
        var msuPathToUpload = await GetMsuToUpload(msuWindow);
        if (string.IsNullOrEmpty(msuPathToUpload))
        {
            return;
        }
        
        var hardwareDirectoriesWindow = new HardwareDirectoriesWindow();
        await hardwareDirectoriesWindow.ShowDialog(msuWindow, msuPathToUpload);
        
        if (hardwareDirectoriesWindow.HardwareMsus?.Count > 0)
        {
            UpdateHardwareMode(msuList, hardwareDirectoriesWindow.HardwareMsus);
        }
        
    }
    
    public async Task BrowseDevice(MsuWindow msuWindow, MsuList msuList)
    {
        var hardwareDirectoriesWindow = new HardwareDirectoriesWindow();
        await hardwareDirectoriesWindow.ShowDialog(msuWindow, null);
        
        if (hardwareDirectoriesWindow.HardwareMsus?.Count > 0)
        {
            UpdateHardwareMode(msuList, hardwareDirectoriesWindow.HardwareMsus);
        }
    }

    public bool GenerateMsu(out string error, out bool openContinuousWindow, out Msu? msu, out string? warningMessage)
    {
        warningMessage = "";
        error = "";
        msu = null;

        var outputMsuType = msuTypeService.GetMsuType(Model.SelectedMsuType);

        if (outputMsuType == null)
        {
            error = "Invalid MSU Type";
            openContinuousWindow = false;
            return false;
        }

        var msus = Model.SelectedMsus.Select(x => x.Msu).ToList();
        if (msus.Count == 0)
        {
            error = "No MSUs selected";
            openContinuousWindow = false;
            return false;
        }

        if (!string.IsNullOrEmpty(userOptions.MsuUserOptions.OutputRomPath) &&
            !string.IsNullOrEmpty(userOptions.MsuUserOptions.CopyRomDirectory))
        {
            if (romCopyService.CopyRom(userOptions.MsuUserOptions.OutputRomPath, out var outPath, out var copyError))
            {
                userOptions.MsuUserOptions.OutputRomPath = outPath;
            }
            else
            {
                error = copyError ?? "Error copying rom to copy rom directory";
                openContinuousWindow = false;
                return false;
            }
        }
        
        var options = userOptions.MsuUserOptions;
        if (options.RandomizationStyle == MsuRandomizationStyle.Single)
        {
            if (options.SelectedMsus?.Count == 1)
            {
                var response = msuSelectorService.AssignMsu(new MsuSelectorRequest()
                {
                    EmptyFolder = true,
                    Msu = msus.First(),
                    OutputMsuType = msuTypeService.GetMsuType(Model.SelectedMsuType)
                });

                if (!response.Successful)
                {
                    error = response.Message ?? "There was an error generating the MSU";
                    logger.LogError(error);
                    openContinuousWindow = false;
                    return false;
                }
                else
                {
                    msu = response.Msu;
                    openContinuousWindow = options.OpenMonitorWindow;
                    warningMessage = response.Message;

                    if (options.LaunchRom && !string.IsNullOrEmpty(options.OutputRomPath))
                    {
                        romLauncherService.LaunchRom(options.OutputRomPath);
                    }
                    return true;
                }
            }
            else
            {
                var response = msuSelectorService.PickRandomMsu(new MsuSelectorRequest()
                {
                    EmptyFolder = true,
                    Msus = msus,
                    OutputMsuType = outputMsuType
                });
                
                if (!response.Successful)
                {
                    error = response.Message ?? "There was an error generating the MSU";
                    logger.LogError(error);
                    openContinuousWindow = false;
                    return false;
                }
                else
                {
                    msu = response.Msu;
                    openContinuousWindow = options.OpenMonitorWindow;
                    warningMessage = response.Message;
                    
                    if (options.LaunchRom && !string.IsNullOrEmpty(options.OutputRomPath))
                    {
                        romLauncherService.LaunchRom(options.OutputRomPath);
                    }
                    
                    return true;
                }
            }
        }
        else if (options.RandomizationStyle == MsuRandomizationStyle.Shuffled)
        {
            var response = msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                EmptyFolder = true,
                Msus = msus,
                OutputMsuType = outputMsuType,
                ShuffleStyle = options.MsuShuffleStyle,
                MsuCopyrightSafety = options.MsuCopyrightSafety,
            });
            
            if (!response.Successful)
            {
                error = response.Message ?? "There was an error generating the MSU";
                logger.LogError(error);
                openContinuousWindow = false;
                return false;
            }
            else
            {
                msu = response.Msu;
                openContinuousWindow = options.OpenMonitorWindow;
                warningMessage = response.Message;
                
                if (options.LaunchRom && !string.IsNullOrEmpty(options.OutputRomPath))
                {
                    romLauncherService.LaunchRom(options.OutputRomPath);
                }
                
                return true;
            }
        }

        error = "Invalid randomization style";
        openContinuousWindow = false;
        logger.LogError(error);
        return false;
    }

    public bool ShouldOpenMonitorWindow => userOptions.MsuUserOptions.OpenMonitorWindow;

    public void SetMsuBasePath(string? msuBasePath)
    {
        if (string.IsNullOrEmpty(msuBasePath) || !Directory.Exists(msuBasePath))
        {
            return;
        }

        userOptions.MsuUserOptions.DefaultMsuPath = msuBasePath;
    }

    public MsuType? GetMsuType(string msuTypeName)
    {
        return msuTypeService.GetMsuType(msuTypeName);
    }

    public void SaveSelectedMsuType()
    {
        userOptions.MsuUserOptions.OutputMsuType = Model.SelectedMsuType;
        userOptions.Save();
    }

    private void MsuTypeServiceOnOnMsuTypeLoadComplete(object? sender, EventArgs e)
    {
        Model.MsusTypes = msuTypeService.MsuTypes.Select(x => x.DisplayName).Order().ToList();
        Model.SelectedMsuType = userOptions.MsuUserOptions.OutputMsuType ?? Model.MsusTypes.First();
    }

    private async Task<string?> GetMsuToUpload(MsuWindow msuWindow)
    {
        var outputMsuType = msuTypeService.GetMsuType(Model.SelectedMsuType);
        if (outputMsuType == null)
        {
            return null;
        }

        var selectedPath = "";
        
        foreach (var entry in userOptions.MsuUserOptions.MsuDirectories)
        {
            var path = entry.Key;
            var directoryMsuType = msuTypeService.GetMsuType(Model.SelectedMsuType);
            if (directoryMsuType == null)
            {
                continue;
            }

            if (outputMsuType.IsCompatibleWith(directoryMsuType))
            {
                selectedPath = path;
                break;
            }
        }

        if (string.IsNullOrEmpty(selectedPath))
        {
            return null;
        }
        
        var storagePath = await CrossPlatformTools.OpenFileDialogAsync(msuWindow, FileInputControlType.OpenFile, "MSU files (*.msu)|*.msu|All files (*.*)|*.*", selectedPath);

        if (storagePath == null)
        {
            return "";
        }

        return storagePath.TryGetLocalPath();
    }
}