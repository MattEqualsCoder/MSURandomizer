using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public interface IMsuLookupService
{
    public IEnumerable<Msu> LookupMsus(string directory);
}