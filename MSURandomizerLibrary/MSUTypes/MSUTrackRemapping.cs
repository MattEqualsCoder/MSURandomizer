using System;
using System.Collections.Generic;
using System.Linq;

namespace MSURandomizerLibrary.MSUTypes;

public class MSUTrackRemapping
{
    public required string OriginalTrackNumbers { get; set; }
    public required string RemappedTrackNumbers { get; set; }
    public bool OnlyAddIfMissing { get; set; }

    public List<int> OriginalTrackNumbersList => OriginalTrackNumbers.Split(",", StringSplitOptions.TrimEntries).Select(int.Parse).ToList();
    public List<int> RemappedTrackNumbersList => RemappedTrackNumbers.Split(",", StringSplitOptions.TrimEntries).Select(int.Parse).ToList();
}