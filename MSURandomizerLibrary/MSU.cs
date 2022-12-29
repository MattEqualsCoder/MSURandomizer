using MSURandomizerLibrary.MSUTypes;
using System.Collections.Generic;

namespace MSURandomizerLibrary;

/// <summary>
/// An MSU that was found in the MSU directory
/// </summary>
public class MSU
{
    public required string FileName { get; init; }
    public required string MSUPath { get; init; }
    public required Dictionary<int, string> PCMFiles { get; init; }
    public string TypeName => Type?.Name ?? "Unknown";
    public MSUType? Type { get; init; }
    public required string FolderName { get; init; }
}