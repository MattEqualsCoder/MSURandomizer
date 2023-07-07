using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public interface IMsuSelectorService
{
    public Msu AssignMsu(Msu msu, MsuType msuType, string outputPath);

    public Msu PickRandomMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, bool emptyFolder = true);

    public Msu CreateShuffledMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, Msu? prevMsu = null, bool emptyFolder = true, bool avoidDuplicates = true);

    public Msu ConvertMsu(Msu msu, MsuType msuType);

    public Msu SaveMsu(Msu msu, string outputPath, Msu? prevMsu = null);

    public ICollection<Msu> ConvertMsus(ICollection<Msu> msus, MsuType msuType);
}