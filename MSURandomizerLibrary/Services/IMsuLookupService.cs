﻿using MSURandomizerLibrary.Configs;
using SnesConnectorLibrary.Responses;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for looking up MSUs
/// </summary>
public interface IMsuLookupService
{

    /// <summary>
    /// Loads all MSUs based on the App Settings
    /// </summary>
    /// <returns>A collection of all MSUs found</returns>
    public IReadOnlyCollection<Msu> LookupMsus();
    
    /// <summary>
    /// Instructs the UI that some MSU display information has been updated
    /// </summary>
    public void RefreshMsuDisplay();

    /// <summary>
    /// Loads all MSUs within a directory and all subdirectories
    /// </summary>
    /// <param name="defaultDirectory">The directory to search for MSUs</param>
    /// <param name="msuDirectories">A collection of folders for specific msu types</param>
    /// <param name="ignoreCache">If what was previously stored in the cache should be ignored</param>
    /// <returns>A collection of all MSUs found</returns>
    public IReadOnlyCollection<Msu> LookupMsus(string? defaultDirectory, Dictionary<string, string>? msuDirectories = null, bool ignoreCache = false);

    /// <summary>
    /// Loads a specific MSU and all of its tracks
    /// </summary>
    /// <param name="msuPath">The path of the MSU to load</param>
    /// <param name="msuTypeFilter">Used to try to ensure that the msu is loaded as the given msu type or a compatible
    /// msu type. If not provided, it will try to determine the best fit of MSU out of all list types.</param>
    /// <param name="saveToCache">If the msu details should be automatically saved to the cache file.</param>
    /// <param name="ignoreCache">If what was previously stored in the cache should be ignored</param>
    /// <param name="forceLoad">Force load, even if it was generated by the MSU Randomizer</param>
    /// <param name="parentDirectory">The path in which the MSU was looked for under</param>
    /// <returns>The MSU object with details of the MSU and its found tracks</returns>
    public Msu? LoadMsu(string msuPath, MsuType? msuTypeFilter = null, bool saveToCache = true, bool ignoreCache = false, bool forceLoad = false, string parentDirectory = "");

    /// <summary>
    /// Creates an MSU from files on hardware such as an FxPakPro
    /// </summary>
    /// <param name="snesMsu">The hardware file for the MSU itself</param>
    /// <param name="hardwarePcmFiles">The PCM files on the hardware</param>
    /// <returns>The generated MSU</returns>
    public Msu LoadHardwareMsu(SnesFile snesMsu, IEnumerable<SnesFile> hardwarePcmFiles);

    /// <summary>
    /// The collection of loaded MSUs
    /// </summary>
    public IReadOnlyCollection<Msu> Msus { get; }
    
    /// <summary>
    /// The current status of the MSU loading
    /// </summary>
    public MsuLoadStatus Status { get; set; }
    
    /// <summary>
    /// Event fired off when the MSU lookup starts
    /// </summary>
    public event EventHandler? OnMsuLookupStarted;
    
    /// <summary>
    /// Event fired off when the MSU lookup is completed
    /// </summary>
    public event EventHandler<MsuListEventArgs>? OnMsuLookupComplete;

    /// <summary>
    /// Function to reload an MSU with the latest settings
    /// </summary>
    /// <param name="msu">The MSU that was updated</param>
    public void RefreshMsu(Msu msu);

    /// <summary>
    /// Retrieves MSUs that match the given paths
    /// </summary>
    /// <param name="paths">The paths to search for</param>
    /// <returns>The collection of found MSUS</returns>
    public ICollection<Msu> GetMsusByPath(ICollection<string>? paths);
    
    /// <summary>
    /// Retrieves a MSU that matches the given path
    /// </summary>
    /// <param name="path">The path to search for</param>
    /// <returns>The found MSU</returns>
    public Msu? GetMsuByPath(string? path);
    
    /// <summary>
    /// A dictionary containing all of the MSUs that had issues loading
    /// </summary>
    public IDictionary<string, string> Errors { get; }
}