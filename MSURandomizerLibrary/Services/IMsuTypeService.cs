using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public interface IMsuTypeService
{
    public void LoadMsuTypes(string directory);

    public IReadOnlyCollection<MsuType> MsuTypes { get; }
}