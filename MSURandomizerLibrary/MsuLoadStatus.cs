namespace MSURandomizerLibrary;

/// <summary>
/// Status for the MSU Lookup service
/// </summary>
public enum MsuLoadStatus
{
    /// <summary>
    /// Default - Has not started loading yet
    /// </summary>
    Default,
    
    /// <summary>
    /// MSUs are loaded
    /// </summary>
    Loaded,
    
    /// <summary>
    /// MSUs are currently loading
    /// </summary>
    Loading
}