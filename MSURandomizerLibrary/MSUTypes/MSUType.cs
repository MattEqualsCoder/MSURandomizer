using System.Collections.Generic;

namespace MSURandomizerLibrary.MSUTypes
{
    public class MSUType
    {
        public required string Name { get; set; }
        public required int MinTrackCount { get; set; }
        public required int MaxTrackCount { get; set; }
        public required IEnumerable<int> RequiredTracks { get; set; }
        public IEnumerable<MSUTrackRemapping>? Remaps { get; set; }
        public IDictionary<int, int>? Pairs { get; set; }
        public IEnumerable<MSUConversion>? Conversions { get; set; }

        public bool Matches(Dictionary<int, string> pcmFiles)
        {
            if (pcmFiles.Count < MinTrackCount || pcmFiles.Count > MaxTrackCount) return false;
            foreach (var requiredTrack in RequiredTracks)
            {
                if (!pcmFiles.ContainsKey(requiredTrack)) return false;
            }
            return true;
        }
    }

}
