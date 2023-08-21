using System.ComponentModel;

namespace MSURandomizerLibrary;

/// <summary>
/// Filter type for finding matching MSUs
/// </summary>
public enum MsuFilter
{
    /// <summary>
    /// If any MSUs that can be converted to the selected MSU type can be found
    /// </summary>
    [Description("Compatible MSUs")]
    Compatible,
    
    /// <summary>
    /// If only exact matches should be found
    /// </summary>
    [Description("Exact Match")]
    Exact,
    
    /// <summary>
    /// If all MSUs should be displayed
    /// </summary>
    [Description("All")]
    All
}