using MSURandomizerLibrary.MSUTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSURandomizerLibrary
{
    public class MSU
    {
        public required string Name { get; init; }
        public required string MSUPath { get; init; }
        public required Dictionary<int, string> PCMFiles { get; init; }
        public string TypeName => Type?.Name ?? "Unknown";
        public MSUType? Type { get; init; }
    }
}
