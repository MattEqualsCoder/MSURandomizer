using System.Collections.Generic;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Models;

/// <summary>
/// Request object for the MSU Selector service for creating MSUs
/// </summary>
public class MsuSelectorRequest
{
    /// <summary>
    /// The MSU to selected
    /// </summary>
    public Msu? Msu { get; set; }
    
    /// <summary>
    /// The path of the MSU to be selected
    /// </summary>
    public string? MsuPath { get; set; }
    
    /// <summary>
    /// The MSUs that should be randomly selected between
    /// </summary>
    public ICollection<Msu>? Msus { get; set; }
    
    /// <summary>
    /// The paths of the MSUs that should be randomly selected between
    /// </summary>
    public ICollection<string>? MsuPaths;
    
    /// <summary>
    /// The MSU type to save the generated MSU as
    /// </summary>
    public MsuType? OutputMsuType { get; set; }
    
    /// <summary>
    /// The name of the MSU type to save the generated MSU as
    /// </summary>
    public string? OutputMsuTypeName { get; set; }
    
    /// <summary>
    /// The output path of the saved MSU
    /// </summary>
    public string? OutputPath { get; set; }
    
    /// <summary>
    /// If the folder should be emptied or not before creating the MSU 
    /// </summary>
    public bool EmptyFolder { get; set; }
    
    /// <summary>
    /// If the MSU folder should be opened after generation
    /// </summary>
    public bool? OpenFolder { get; set; }
    
    /// <summary>
    /// The previous MSU for pulling the last tracks from if a track can't be saved
    /// </summary>
    public Msu? PrevMsu { get; set; }
    
    /// <summary>
    /// If duplicate tracks should be avoided
    /// </summary>
    public bool? AvoidDuplicates { get; set; }
    
    /// <summary>
    /// If only copyright safe tracks should be used
    /// </summary>
    public MsuCopyrightSafety MsuCopyrightSafety { get; set; }
    
    /// <summary>
    /// The shuffling style
    /// </summary>
    public MsuShuffleStyle ShuffleStyle { get; set; }

    /// <summary>
    /// The chance of the chaos shuffler changing track type
    /// </summary>
    public int ChaosShuffleChance { get; set; } = 5;
    
    /// <summary>
    /// The current track being played
    /// </summary>
    public Track? CurrentTrack { get; set; }
}