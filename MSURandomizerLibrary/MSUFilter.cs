using System.ComponentModel;

namespace MSURandomizerLibrary;

/// <summary>
/// Filter type for finding matching MSUs
/// </summary>
public enum MSUFilter
{
    [Description("Compatible MSUs")]
    Compatible,
    
    [Description("Exact Match")]
    Exact,
    
    [Description("All")]
    All
}