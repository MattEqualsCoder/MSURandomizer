using System.Collections.Generic;
using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary.Services;

public interface IMsuDetailsService
{
    public Msu? LoadMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlPath);
    public void SaveMsuDetails(Msu msu, string outputPath);
}