using System.Text.Json;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

internal class MsuCacheService : IMsuCacheService
{
    private const int CurrentCacheVersion = 3;
    private readonly IMsuTypeService _msuTypeService;
    private readonly MsuUserOptions _msuUserOptions;
    private readonly ILogger<MsuCacheService> _logger;

    private MsuLookupCache _cache = new()
    {
        CacheVersion = CurrentCacheVersion
    };
    
    private string _cachePath = "";
    private bool _hasUpdated;
    private bool _initialized;

    public MsuCacheService(IMsuTypeService msuTypeService, ILogger<MsuCacheService> logger, MsuUserOptions msuUserOptions)
    {
        _msuTypeService = msuTypeService;
        _logger = logger;
        _msuUserOptions = msuUserOptions;
    }

    public void Initialize(string cachePath)
    {
        if (Directory.Exists(cachePath))
        {
#if DEBUG
            _cachePath = Path.Combine(cachePath, "msu_cache_debug.json");
#else
            _cachePath = Path.Combine(cachePath, "msu_cache.json");
#endif
        }

        _initialized = true;

        if (File.Exists(_cachePath))
        {
            _logger.LogInformation("Loading MSU Cache from {Path}", _cachePath);
            var text = File.ReadAllText(_cachePath);
            var tempCache = JsonSerializer.Deserialize<MsuLookupCache>(text) ?? new MsuLookupCache();
            if (tempCache.CacheVersion == CurrentCacheVersion)
            {
                _cache = tempCache;
            }
            else
            {
                _logger.LogInformation("Cache version out of date.");
            }
        }
        
        _logger.LogInformation("Loaded {Count} MSUs from Cache", _cache.Data.Count);
    }

    public Msu? Get(string msuPath, string yamlHash, ICollection<string> pcmFiles)
    {
        if (!_cache.Data.TryGetValue(msuPath, out var cacheEntry)) return null;
        if (cacheEntry.YamlHash != yamlHash) return null;
        var pcmFileString = string.Join(";", pcmFiles.Order());
        if (cacheEntry.PcmFileList != pcmFileString) return null;

        var msu = cacheEntry.MsuData;
        
        msu.MsuType = _msuTypeService.GetMsuType(cacheEntry.MsuTypeName);
        msu.Settings = _msuUserOptions.GetMsuSettings(msuPath);

        foreach (var track in msu.Tracks)
        {
            track.Msu = msu;
            track.OriginalMsu = msu;
        }

        return msu;
    }

    public void Put(Msu msu, string yamlHash, ICollection<string> pcmFiles, bool saveCache)
    {
        _cache.Data[msu.Path] = new MsuLookupCacheEntry()
        {
            MsuData = msu,
            YamlHash = yamlHash,
            MsuTypeName = msu.MsuTypeName,
            PcmFileList = string.Join(";", pcmFiles.Order())
        };

        _hasUpdated = true;

        if (saveCache)
        {
            Save();
        }
    }

    public void Remove(string msuPath, bool saveCache)
    {
        _cache.Data.TryRemove(msuPath, out _);

        _hasUpdated = true;
        
        if (saveCache)
        {
            Save();
        }
    }

    public void Save()
    {
        if (!_initialized || !_hasUpdated) return;
        try
        {
            var text = JsonSerializer.Serialize(_cache);
            File.WriteAllText(_cachePath, text);
            _logger.LogInformation("Saved {Count} MSUs to Cache", _cache.Data.Count);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to save to cache");
        }
    }
}