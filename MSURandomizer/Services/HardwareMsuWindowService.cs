using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
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
    ISnesConnectorService snesConnectorService) : ControlService
{
    private readonly HardwareMsuViewModel _model = new();

    public HardwareMsuViewModel InitializeModel()
    {
        snesConnectorService.Connected += SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnDisconnected;
        return _model;
    }

    public event EventHandler? OpenMsuMonitorWindow;

    private void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        taskService.RunTask(async () =>
        {
            await UploadMsuRom();
        });
    }

    public async Task UploadMsuRom()
    {
        if (!msuUserOptionsService.MsuUserOptions.PassedRomArgument)
        {
            var path = await CrossPlatformTools.OpenFileDialogAsync((Window)ParentControl!, FileInputControlType.OpenFile,
                "Rom Files:*.sfc,*.smc,*.gb,*.gbc",
                msuUserOptionsService.MsuUserOptions.OutputRomPath ??
                msuUserOptionsService.MsuUserOptions.OutputFolderPath);
        
            if (path is not IStorageFile file || file.TryGetLocalPath() == null)
            {
                _model.Message = "You must select a rom file";
                return;
            }

            msuUserOptionsService.MsuUserOptions.OutputRomPath = path.TryGetLocalPath();
        }
        
        _model.Message = "Uploading rom";
        
        await taskService.RunTask(async() =>
        {
            var msus = _model.Msus.Select(x => x.Msu).ToList();
            var msu = await msuHardwareService.UploadMsuRom(msus, msuUserOptionsService.MsuUserOptions.OutputRomPath!,
                msuUserOptionsService.MsuUserOptions.LaunchRom);
            if (msu == null)
            {
                _model.Message = "Unable to upload rom to hardware";
                return;
            }
            _model.SelectedMsu = msu;
            if (msuUserOptionsService.MsuUserOptions.LaunchRom)
            {
                _model.Message = "Booting rom";
                await Task.Delay(TimeSpan.FromSeconds(5));
                _model.Complete = true;
                _model.Message = "Complete";

                if (msuUserOptionsService.MsuUserOptions.OpenMonitorWindow)
                {
                    OpenMsuMonitorWindow?.Invoke(this, EventArgs.Empty);    
                }
                else
                {
                    Disconnect(true);
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                _model.Complete = true;
                _model.Message = "Complete";
                
                if (msuUserOptionsService.MsuUserOptions.OpenMonitorWindow)
                {
                    OpenMsuMonitorWindow?.Invoke(this, EventArgs.Empty);    
                }
                else
                {
                    Disconnect(true);
                }
                
            }
            
        });
    }
    
    private void SnesConnectorServiceOnDisconnected(object? sender, EventArgs e)
    {
        _model.Message = "Disconnected";
    }
    
    public void Connect()
    {
        _model.Message = "Connecting";
        snesConnectorService.Connect(msuUserOptionsService.MsuUserOptions.SnesConnectorSettings);
    }

    public void Disconnect(bool fromConnector)
    {
        snesConnectorService.Connected -= SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected -= SnesConnectorServiceOnDisconnected;
        
        if (fromConnector)
        {
            snesConnectorService.Disconnect();    
        }
    }
}