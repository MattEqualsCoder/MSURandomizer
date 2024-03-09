using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;

namespace MSURandomizerLibrary.Services;

internal class MsuHardwareService(
    ISnesConnectorService snesConnectorService,
    IMsuLookupService msuLookupService,
    ILogger<MsuHardwareService> logger) : IMsuHardwareService
{
    public async Task<List<Msu>?> GetMsusFromDevice()
    {
        var cts = new CancellationTokenSource();
        List<Msu>? response = null;

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
        
        return response;
    }

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
                    x.Name.EndsWith(".pcm", StringComparison.OrdinalIgnoreCase));
                toReturn.Add(msuLookupService.LoadHardwareMsu(snesMsu, pcmFiles));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error converting MSU {Path} from hardware", snesMsu.FullPath);
            }
        }
    
        return toReturn;
    }
}