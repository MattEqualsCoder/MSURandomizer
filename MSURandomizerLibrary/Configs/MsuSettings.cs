using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

/// <summary>
/// User overrides for an MSU
/// </summary>
public class MsuSettings
{
    /// <summary>
    /// Constructor
    /// </summary>
    public MsuSettings()
    {
        MsuPath = "";
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">The path to the MSU</param>
    public MsuSettings(string path)
    {
        MsuPath = path;
    }
    
    /// <summary>
    /// The path to the MSU these settings are for
    /// </summary>
    public string MsuPath { get; set; }
    
    /// <summary>
    /// MSU type to use as an override
    /// </summary>
    public string? MsuTypeName { get; set; }
    
    /// <summary>
    /// Setting for whether to automatically pick alt tracks or not
    /// </summary>
    public AltOptions AltOption { get; set; }
    
    /// <summary>
    /// The name of the MSU if no MSU details file is found
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The creator of the MSU if no MSU details file is found
    /// </summary>
    public string? Creator { get; set; }
    
    /// <summary>
    /// If this is a favorite or not
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// How frequent this MSU should come up
    /// </summary>
    public ShuffleFrequency ShuffleFrequency { get; set; }
    
    /// <summary>
    /// If the user confirmed that this is an unknown MSU
    /// </summary>
    public bool IsUserUnknownMsu { get; set; }

    /// <summary>
    /// If this is not a favorite
    /// </summary>
    [YamlIgnore]
    public bool IsNotFavorite => IsFavorite != true;

    /// <summary>
    /// If this is to be shuffled the default amount
    /// </summary>
    [YamlIgnore]
    public bool IsDefaultFrequency => ShuffleFrequency == ShuffleFrequency.Default;
    
    /// <summary>
    /// If this is to be shuffled twice as much
    /// </summary>
    [YamlIgnore]
    public bool IsMoreFrequent => ShuffleFrequency == ShuffleFrequency.MoreFrequent;
    
    /// <summary>
    /// If this is to be shuffled half as much
    /// </summary>
    [YamlIgnore]
    public bool IsLessFrequent => ShuffleFrequency == ShuffleFrequency.LessFrequent;
    
    /// <summary>
    /// The MSU Type object that was specified by the user
    /// </summary>
    [YamlIgnore]
    public MsuType? MsuType { get; set; }

    /// <summary>
    /// If there are any specified settings
    /// </summary>
    [YamlIgnore]
    public bool HasSettings => !string.IsNullOrEmpty(MsuTypeName) 
                                 || !string.IsNullOrEmpty(Name) 
                                 || !string.IsNullOrEmpty(Creator) 
                                 || AltOption != AltOptions.Randomize
                                 || IsFavorite
                                 || ShuffleFrequency != ShuffleFrequency.Default
                                 || IsUserUnknownMsu;
}