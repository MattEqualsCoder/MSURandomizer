using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Models;

/// <summary>
/// Response from the MSU Selector Service for creating MSUs
/// </summary>
public class MsuSelectorResponse
{
    /// <summary>
    /// The generated MSU
    /// </summary>
    public Msu? Msu { get; init; }
    
    /// <summary>
    /// The collection of generated MSUs
    /// </summary>
    public ICollection<Msu>? Msus { get; init; }
    
    /// <summary>
    /// If the request was successful
    /// </summary>
    public bool Successful { get; init; }
    
    /// <summary>
    /// Error message from the request
    /// </summary>
    public string? Message { get; set; }
}