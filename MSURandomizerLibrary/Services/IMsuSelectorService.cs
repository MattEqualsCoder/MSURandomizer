using System.Collections.Generic;
using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary.Services;

public interface IMsuSelectorService
{
    public Msu AssignMsu(Msu msu, MsuType msuType, string outputPath);

    public Msu PickRandomMsu(ICollection<Msu> msus, MsuType msuType, string outputPath);

    public Msu CreateShuffledMsu(ICollection<Msu> msus, MsuType msuType, string outputPath, Msu? prevMsu = null);

    public Msu ConvertMsu(Msu msu, MsuType msuType);

    public Msu SaveMsu(Msu msu, string outputPath, Msu? prevMsu = null);

    public ICollection<Msu> ConvertMsus(ICollection<Msu> msus, MsuType msuType);
}