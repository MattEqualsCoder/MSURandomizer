using System.IO;
using MSURandomizerLibrary.Models;

namespace MSURandomizerLibrary.Services;

/// <summary>
/// Service for initializing required services from the app settings
/// </summary>
public interface IMsuRandomizerInitializationService
{
    /// <summary>
    /// Initializes required MSU randomizer services
    /// </summary>
    /// <param name="request">Details used for initializing required services</param>
    public void Initialize(MsuRandomizerInitializationRequest request);
}