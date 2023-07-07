using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public interface IMsuDetailsService
{
    public Msu? LoadMsuDetails(MsuType msuType, string msuPath, string msuDirectory, string msuBaseName, string yamlPath);
    public void SaveMsuDetails(Msu msu, string outputPath);
}