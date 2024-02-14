namespace MSURandomizerLibrary;

/// <summary>
/// Enum for the frequency in which an MSU should appear when shuffling
/// </summary>
public enum ShuffleFrequency
{
    /// <summary>
    /// The default rate
    /// </summary>
    Default,
    
    /// <summary>
    /// The MSU should be half as likely to show up
    /// </summary>
    LessFrequent,
    
    /// <summary>
    /// The MSU should be twice as likely to show up
    /// </summary>
    MoreFrequent
}