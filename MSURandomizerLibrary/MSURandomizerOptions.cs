using System.Collections.Generic;
using MSURandomizerLibrary.MSUTypes;

namespace MSURandomizerLibrary
{
    public class MSURandomizerOptions
    {
        public string? Directory { get; set; }
        public string Name { get; set; } = "RandomizedMSU";
        public string? OutputType { get; set; }
        public MSUFilter Filter { get; set; } = MSUFilter.Compatible;
        public bool AvoidDuplicates { get; set; }
        public bool OpenFolderOnCreate { get; set; }
        public List<string>? SelectedMSUs { get; set; }
        public string? CreatedMSUPath { get; set; }
    }
}
