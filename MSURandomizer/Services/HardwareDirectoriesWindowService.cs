using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using Microsoft.Extensions.Logging;
using MSURandomizer.ViewModels;
using MSURandomizer.Views;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using Path = System.IO.Path;

namespace MSURandomizer.Services;

public class HardwareDirectoriesWindowService(ISnesConnectorService snesConnectorService, IMsuUserOptionsService msuUserOptionsService, ILogger<HardwareDirectoriesWindowService> logger, IMsuHardwareService msuHardwareService) : ControlService
{
    private HardwareDirectoriesWindowViewModel _model = new();
    private List<SnesFile> _allSnesFiles = new();
    private CancellationTokenSource? _cts;
    private HardwareDirectoriesWindow _hardwareDirectoriesWindow = null!;
    
    public HardwareDirectoriesWindowViewModel InitializeModel(HardwareDirectoriesWindow window)
    {
        _hardwareDirectoriesWindow = window;
        snesConnectorService.Connected += SnesConnectorServiceOnConnected;
        return _model;
    }

    public void LoadData()
    {
        if (snesConnectorService.IsConnected)
        {
            LoadHardwareDirectories();    
        }
        else
        {
            snesConnectorService.Connect(msuUserOptionsService.MsuUserOptions.SnesConnectorSettings);
        }
    }

    public async Task<List<Msu>?> ReloadHardwareMsus()
    {
        _model.IsLoadingIndeterminate = true;
        _model.LoadingDataText = "Reloading hardware MSUs...";
        _model.IsLoadingData = true;
        return await msuHardwareService.GetMsusFromDevice();
    }

    public void Disconnect()
    {
        snesConnectorService.Connected -= SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnect();
        _cts?.Cancel();
    }

    public async Task CreateNewDirectory(string directoryName)
    {
        var node = _model.SelectedTreeNode;
        var directory = $"/{directoryName}";
        
        if (node != null)
        {
            var path = node.IsFolder ? node.Path : node.ParentPath; 
            directory = $"{path}/{directoryName}".Replace("//", "/");
        }

        await snesConnectorService.CreateDirectoryAsync(new SnesCreateDirectoryRequest()
        {
            Path = directory
        });
        
        LoadHardwareDirectories();
    }

    public async Task<bool> UploadMsu()
    {
        if (string.IsNullOrEmpty(_model.MsuToUpload))
        {
            return false;
        }
        
        var deviceDirectory = $"{_model.SelectedTreeNode?.Path}/";

        var msuToUpload = new FileInfo(_model.MsuToUpload);
        var folder = msuToUpload.DirectoryName!;
        var baseName = Path.GetFileNameWithoutExtension(_model.MsuToUpload);

        var filesToUpload = Directory.EnumerateFiles(folder, $"{baseName}*.pcm").Concat([_model.MsuToUpload]).ToList();
        
        if (filesToUpload.Count == 0)
        {
            return true;
        }

        var shouldContinue = await MessageWindow.ShowYesNoDialog(
            $"You are about to upload {filesToUpload.Count} files to {deviceDirectory}. This could take a very long time. Are you sure you want to continue?",
            "Continue?", _hardwareDirectoriesWindow);
        if (!shouldContinue)
        {
            return true;
        }
        
        _model.LoadingProgress = 0;
        _model.IsLoadingIndeterminate = false;
        _model.LoadingDataText = "Uploading...";
        _model.IsLoadingData = true;
        _model.LoadingItemCount = filesToUpload.Count;
        
        float uploadedFiles = 0;
        
        _cts = new CancellationTokenSource();

        snesConnectorService.UpdateTimeoutSeconds(180);
        
        logger.LogInformation("Uploading {Count} files to {Directory}", filesToUpload.Count, deviceDirectory);
        
        foreach (var file in filesToUpload)
        {
            if (_cts.IsCancellationRequested)
            {
                break;
            }

            if (!file.EndsWith(".msu", StringComparison.OrdinalIgnoreCase))
            {
                _model.LoadingDataText = $"Uploading {Path.GetFileName(file)}";
                await snesConnectorService.UploadFileAsync(new SnesUploadFileRequest()
                {
                    LocalFilePath = file,
                    TargetFilePath = $"{deviceDirectory}{Path.GetFileName(file)}"
                });
            }
            else
            {
                _model.LoadingDataText = $"Uploading temp {Path.GetFileName(file)}";
                var tempFile = Path.Combine(System.IO.Path.GetTempPath(), $"{baseName}.msu");
                await File.WriteAllTextAsync(tempFile, "msu-1");
                await snesConnectorService.UploadFileAsync(new SnesUploadFileRequest()
                {
                    LocalFilePath = tempFile,
                    TargetFilePath = $"{deviceDirectory}{Path.GetFileName(file)}"
                });
                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                    // Do nothing
                }
            }

            uploadedFiles++;
            
            _model.LoadingProgress = uploadedFiles;
        }

        _model.DidUpdate = true;
        var successful = !_cts.IsCancellationRequested;
        
        logger.LogInformation("Uploading finished {State}", successful ? "successfully" : "unsuccessfully");
        
        _cts = null;
        _model.IsLoadingData = false;
        return successful;
    }

    public async Task DeleteSelectedItem()
    {
        var selectedFile = _model.SelectedTreeNode?.Path;

        if (string.IsNullOrEmpty(selectedFile))
        {
            return;
        }

        var filesToDelete = _allSnesFiles.Where(x => x.FullPath.StartsWith(selectedFile)).OrderBy(x => x.IsFolder)
            .ToList();

        if (filesToDelete.Count == 0)
        {
            return;
        }

        _model.LoadingProgress = 0;
        _model.IsLoadingIndeterminate = false;
        _model.LoadingDataText = "Deleting...";
        _model.IsLoadingData = true;
        _model.LoadingItemCount = filesToDelete.Count;

        float deletedFiles = 0;
        
        _cts = new CancellationTokenSource();

        foreach (var file in filesToDelete)
        {
            if (_cts.IsCancellationRequested)
            {
                break;
            }
            
            _model.LoadingDataText = $"Deleting {file.ParentName}/{file.Name}";
            
            await snesConnectorService.DeleteFileAsync(new SnesDeleteFileRequest()
            {
                Path = file.FullPath
            });

            _model.DidUpdate = true;
            
            deletedFiles++;
            
            _model.LoadingProgress = deletedFiles;
        }
        
        _cts = null;
        LoadHardwareDirectories();
    }

    private void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        LoadHardwareDirectories();
    }

    public void LoadHardwareDirectories()
    {
        _model.LoadingDataText = "Loading directory list...";
        _model.IsLoadingIndeterminate = true;
        _model.IsLoadingData = true;
        
        snesConnectorService.GetFileList(new SnesFileListRequest
        {
            Recursive = true,
            OnResponse = list =>
            {
                _allSnesFiles = list.ToList();
                _model.TreeNodes = GetListFromResult(list);
                _model.IsLoadingData = false;
            }
        });
    }

    public List<HardwareItem> GetListFromResult(List<SnesFile> snesFiles)
    {
        if (!string.IsNullOrEmpty(_model.MsuToUpload))
        {
            snesFiles = snesFiles.Where(x => x.IsFolder).ToList();
        }
        
        return snesFiles.Where(x => x is { ParentName: "" })
            .OrderBy(x => !x.IsFolder)
            .ThenBy(x => x.Name)
            .Select(x => ToHardwareDirectory(x, snesFiles, "/"))
            .ToList();
    }

    public HardwareItem ToHardwareDirectory(SnesFile file, List<SnesFile> allFiles, string parentPath)
    {
        if (!parentPath.EndsWith("/"))
        {
            parentPath += "/";
        }

        var directory = new HardwareItem(file.Name, file.FullPath, parentPath, file.IsFolder);

        if (directory.IsFolder)
        {
            var directoryPath = file.FullPath;
            if (!directoryPath.EndsWith("/"))
            {
                directoryPath += "/";
            }
            
            directory.Directories = file.IsFolder
                ? allFiles.Where(x => x.FullPath == directoryPath + x.Name)
                    .OrderBy(x => !x.IsFolder)
                    .ThenBy(x => x.Name)
                    .Select(x => ToHardwareDirectory(x, allFiles, file.FullPath))
                    .ToList()
                : [];
        }
        
        return directory;
    }
    
}