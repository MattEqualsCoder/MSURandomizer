using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MsuRandomizerLibrary.Configs;

namespace MsuRandomizerLibrary.Services;

public interface IMsuLookupService
{
    /// <summary>
    /// Loads all MSUs within a directory and all subdirectories
    /// </summary>
    /// <param name="directory">The directory to search for MSUs</param>
    /// <returns>An enumerable list of all MSUs found</returns>
    public IReadOnlyCollection<Msu> LookupMsus(string directory);

    /// <summary>
    /// Loads a specific MSU and all of its tracks
    /// </summary>
    /// <param name="msuPath">The path of the MSU to load</param>
    /// <param name="msuType">The MSU type to load the MSU as, if applicable. If not provided, it will try to determine
    /// the best fit of MSU based on list of PCMs found for the MSU.</param>
    /// <returns>The MSU object with details of the MSU and its found tracks</returns>
    public Msu? LoadMsu(string msuPath, MsuType? msuType = null);

    public IReadOnlyCollection<Msu> Msus { get; }
    
    public event EventHandler<MsuLookupEventArgs>? OnMsuLookupComplete;
}