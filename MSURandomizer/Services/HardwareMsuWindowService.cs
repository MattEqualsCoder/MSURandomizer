using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using AvaloniaControls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using MSURandomizer.ViewModels;
using MSURandomizerLibrary.Services;
using SnesConnectorLibrary;

namespace MSURandomizer.Services;

public class HardwareMsuWindowService(ITaskService taskService,
    IMsuUserOptionsService msuUserOptionsService,
    IMsuHardwareService msuHardwareService,
    ISnesConnectorService snesConnectorService) : IControlService
{
    private HardwareMsuViewModel _model = new();

    public HardwareMsuViewModel InitializeModel()
    {
        if (string.IsNullOrEmpty(msuUserOptionsService.MsuUserOptions.OutputRomPath))
        {
            _model.Message = "No rom selected";
            return _model;
        }
        snesConnectorService.Connected += SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnDisconnected;
        return _model;
    }

    private void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        
        _ = UploadMsuRom();
    }

    public async Task UploadMsuRom()
    {
        _model.Message = "Uploading rom";
        
        /*var path = await CrossPlatformTools.OpenFileDialogAsync(window, FileInputControlType.OpenFile, "Rom Files:*.sfc,*.smc,*.gb,*.gbc", userOptions.MsuUserOptions.OutputRomPath ?? userOptions.MsuUserOptions.OutputFolderPath);
        if (path is not IStorageFile file || file.TryGetLocalPath() == null)
        {
            return;
        }*/
        
        await taskService.RunTask(async() =>
        {
            var msus = _model.Msus.Select(x => x.Msu).ToList();
            await msuHardwareService.UploadMsuRom(msus, msuUserOptionsService.MsuUserOptions.OutputRomPath!,
                msuUserOptionsService.MsuUserOptions.LaunchRom);
            _model.Message = "Complete";
        });
        
    }
    
    private void SnesConnectorServiceOnDisconnected(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    
    public void SelectAndUploadMsu(List<MsuViewModel> msus)
    {
        _model.Message = "Connecting";
        snesConnectorService.Connect(msuUserOptionsService.MsuUserOptions.SnesConnectorSettings);
    }

    public void Disconnect()
    {
        snesConnectorService.Connected -= SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected -= SnesConnectorServiceOnDisconnected;
        snesConnectorService.Disconnect();
    }
    
}