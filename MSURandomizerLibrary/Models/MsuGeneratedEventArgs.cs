using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Models;

/// <summary>
/// Arguments for when a MSU has been generated
/// </summary>
/// <param name="msu">The generated MSU</param>
public class MsuGeneratedEventArgs(MsuBasicDetails msu) : EventArgs
{
    /// <summary>
    /// The generated MSU
    /// </summary>
    public MsuBasicDetails Msu => msu;
}