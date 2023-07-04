using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary.Services;

public interface IMsuSettingsService
{
    public void InitializeSettingsService(string path);

    public void UpdateMsuSettings(Msu msu, MsuSettings settings);

    public MsuSettings GetMsuSettings(Msu msu);
    
    public MsuSettings GetMsuSettings(string msuPath);
}