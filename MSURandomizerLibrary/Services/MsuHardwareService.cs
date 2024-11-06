using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;

namespace MSURandomizerLibrary.Services;

internal class MsuHardwareService(
    ISnesConnectorService snesConnectorService,
    IMsuLookupService msuLookupService,
    IMsuUserOptionsService msuUserOptionsService,
    ILogger<MsuHardwareService> logger) : IMsuHardwareService
{
    private Dictionary<string, (SnesFile msuFile, List<SnesFile> pcmFiles)> _hardwareFileCache = [];
    private Dictionary<string, Msu> _hardwareMsuList = [];
    private List<Msu> _msus;

    public async Task<List<Msu>?> GetMsusFromDevice()
    {
        var cts = new CancellationTokenSource();
        List<Msu>? response = null;
        _hardwareFileCache = [];

        if (!snesConnectorService.GetFileList(new SnesFileListRequest()
            {
                Recursive = true,
                Filter = file => IsMsuFile(file.Name),
                OnResponse = list =>
                {
                    response = ProcessFileList(list);
                    cts.Cancel();
                } 
            }))
        {
            return null;
        }

        while (!cts.Token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.25), cts.Token)
                .ContinueWith(tsk => tsk.Exception == default, CancellationToken.None);
        }

        _hardwareMsuList = response?.ToDictionary(x => x.Path, x => x) ?? [];
        
        if (response?.Count > 0)
        {
            HardwareMsusLoaded?.Invoke(this, new MsuListEventArgs(response, new Dictionary<string, string>()));
        }
        
        return response;
    }

    public async Task<Msu?> UploadMsuRom(List<Msu> msus, string romFilePath, bool bootRomAfter)
    {
        if (msus.Count == 0)
        {
            logger.LogError("No MSUs selected");
            return null;
        }
        
        if (!snesConnectorService.IsConnected || !snesConnectorService.CurrentConnectorFunctionality.CanAccessFiles)
        {
            logger.LogError("Not connected to an SNES connector that can upload files");
            return null;
        }

        msuUserOptionsService.MsuUserOptions.SelectedMsus = msus.Select(x => x.Path).ToList();
        msuUserOptionsService.MsuUserOptions.OutputRomPath = romFilePath;
        msuUserOptionsService.Save();
        
        var random = new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue));
        
        var msu = msus.First();
        if (msus.Count > 1)
        {
            msu = msus.Random(random)!;
        }

        if (!msu.IsHardwareMsu)
        {
            logger.LogError("MSU {Path} is not a hardware MSU", msu.Path);
            return null;
        }

        var cts = new CancellationTokenSource();

        var fileInfo = new FileInfo(romFilePath);
        var extension = fileInfo.Extension;
        var targetPath = msu.Path.Replace(".msu", extension, StringComparison.OrdinalIgnoreCase);

        if (!snesConnectorService.UploadFile(new SnesUploadFileRequest()
            {
                LocalFilePath = romFilePath,
                TargetFilePath = targetPath,
                OnComplete = () => cts.Cancel()
            }))
        {
            return null;
        }
            
        while (!cts.Token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.25), cts.Token)
                .ContinueWith(tsk => tsk.Exception == default, CancellationToken.None);
        }

        if (!bootRomAfter)
        {
            return msu;
        }

        cts = new CancellationTokenSource();

        if (!snesConnectorService.BootRom(new SnesBootRomRequest()
            {
                Path = targetPath,
                OnComplete = () => cts.Cancel()
            }))
        {
            return null;
        }
        
        while (!cts.Token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.25), cts.Token)
                .ContinueWith(tsk => tsk.Exception == default, CancellationToken.None);
        }

        return msu;
    }

    public Msu RefreshMsu(string path)
    {
        if (!_hardwareFileCache.TryGetValue(path, out var files) || !_hardwareMsuList.ContainsKey(path))
        {
            throw new InvalidOperationException($"Hardware msu {path} could not be found to refresh");
        }

        _hardwareMsuList.Remove(path);
        var newMsu = msuLookupService.LoadHardwareMsu(files.msuFile, files.pcmFiles);
        _hardwareMsuList.Add(path, newMsu);
        HardwareMsusChanged?.Invoke(this, new MsuListEventArgs(_hardwareMsuList.Values.ToList(), new Dictionary<string, string>()));
        return newMsu;
    }

    public List<Msu> Msus => _hardwareMsuList.Values.ToList();


    public event EventHandler<MsuListEventArgs>? HardwareMsusLoaded;
    
    public event EventHandler<MsuListEventArgs>? HardwareMsusChanged;

    private bool IsMsuFile(string fileName)
    {
        return fileName.EndsWith(".msu", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".pcm", StringComparison.OrdinalIgnoreCase);
    }

    private List<Msu> ProcessFileList(List<SnesFile> files)
    {
        var snesMsus = files.Where(x => x.Name.EndsWith(".msu", StringComparison.OrdinalIgnoreCase));

        var toReturn = new List<Msu>();

        foreach (var snesMsu in snesMsus)
        {
            try
            {
                var baseFileName = snesMsu.Name.Replace(".msu", "", StringComparison.OrdinalIgnoreCase);
                var pcmFiles = files.Where(x =>
                    x.ParentName == snesMsu.ParentName && x.Name.StartsWith(baseFileName) &&
                    x.Name.EndsWith(".pcm", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                toReturn.Add(msuLookupService.LoadHardwareMsu(snesMsu, pcmFiles));
                _hardwareFileCache[snesMsu.FullPath] = (snesMsu, pcmFiles);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error converting MSU {Path} from hardware", snesMsu.FullPath);
            }
        }
    
        return toReturn;
    }
}