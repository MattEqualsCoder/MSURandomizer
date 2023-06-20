using System.Collections.Generic;
using System.Linq;

namespace MSURandomizerLibrary.MSUTypes
{
    public class MSUType
    {
        public required string Name { get; set; }
        public required int MinTrackCount { get; set; }
        public required int MaxTrackCount { get; set; }
        public required IEnumerable<int> RequiredTracks { get; set; }
        public IEnumerable<int>? IllegalTracks { get; set; }
        public IEnumerable<int>? RequiredLoopTracks { get; set; }
        public IEnumerable<int>? RequiredNonLoopTracks { get; set; }
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

            if (IllegalTracks != null)
            {
                foreach (var illegalTrack in IllegalTracks)
                {
                    if (pcmFiles.ContainsKey(illegalTrack)) return false;
                }
            }
            
            return true;
        }

        public bool MatchesOnPCMLoops(Dictionary<int, string> pcmFiles)
        {
            if (RequiredLoopTracks != null)
            {
                foreach (var track in RequiredLoopTracks)
                {
                    if (pcmFiles.ContainsKey(track) && !MSURandomizerService.DoesPCMLoop(pcmFiles[track])) return false;
                }
            }
            
            if (RequiredNonLoopTracks != null)
            {
                foreach (var track in RequiredNonLoopTracks)
                {
                    if (pcmFiles.ContainsKey(track) && MSURandomizerService.DoesPCMLoop(pcmFiles[track])) return false;
                }
            }

            return true;
        }
    }

}
