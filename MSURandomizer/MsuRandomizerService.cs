using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MsuRandomizer;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Services;

namespace MSURandomizer;

public class MsuRandomizerService
{
    public static MsuRandomizerService Instance = null!;

    private IMsuSelectorService _msuSelectorService;

    private IMsuTypeService _msuTypeService;

    public MsuRandomizerService(IMsuSelectorService msuSelectorService, IMsuTypeService msuTypeService)
    {
        _msuSelectorService = msuSelectorService;
        _msuTypeService = msuTypeService;
        Instance = this;
    }

    public Msu? PickRandomMsu(ICollection<Msu> msus, MsuType type, MsuRandomizerOptions options)
    {
        if (!msus.Any())
        {
            var response = MessageBox.Show("No MSUs selected",
                "MSU Randomizer", MessageBoxButtons.OK);
            return null;
        }
        
        var path = "";
        path = !string.IsNullOrEmpty(options.OutputFolderPath) ? Path.Combine(options.OutputFolderPath, $"{options.Name}.msu") : options.OutputRomPath;
        if (string.IsNullOrEmpty(path)) return null;
        return _msuSelectorService.PickRandomMsu(msus, type, path);
    }
    
    public Msu? CreateShuffledMsu(ICollection<Msu> msus, MsuType type, MsuRandomizerOptions options)
    {
        if (!msus.Any())
        {
            var response = MessageBox.Show("No MSUs selected",
                "MSU Randomizer", MessageBoxButtons.OK);
            return null;
        }
        
        var path = "";
        path = !string.IsNullOrEmpty(options.OutputFolderPath) ? Path.Combine(options.OutputFolderPath, $"{options.Name}.msu") : options.OutputRomPath;
        if (string.IsNullOrEmpty(path)) return null;
        return _msuSelectorService.CreateShuffledMsu(msus, type, path);
    }

    public MsuType? GetMsuType(string name)
    {
        return _msuTypeService.MsuTypes.FirstOrDefault(x => x.Name == name);
    }
}