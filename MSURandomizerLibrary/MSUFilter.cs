using System.ComponentModel;

namespace MSURandomizerLibrary;

public enum MSUFilter
{
    [Description("Compatible MSUs")]
    Compatible,
    
    [Description("Exact Match")]
    Exact,
    
    [Description("All")]
    All
}