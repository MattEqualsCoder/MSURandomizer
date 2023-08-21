namespace MSURandomizerLibrary;

/// <summary>
/// Randomization style for generating MSUs
/// </summary>
public enum MsuRandomizationStyle
{
    /// <summary>
    /// Pick a single random MSU
    /// </summary>
    Single,
    
    /// <summary>
    /// Shuffle tracks from multiple MSUs into one
    /// </summary>
    Shuffled,
    
    /// <summary>
    /// Shuffle tracks from multiple MSUs into one, repeating every minute
    /// </summary>
    Continuous
}