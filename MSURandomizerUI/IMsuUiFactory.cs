using System.Collections.Generic;
using System.Windows.Controls;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerUI.Controls;

namespace MSURandomizerUI;

/// <summary>
/// Factory for creating UI elements to display
/// </summary>
public interface IMsuUiFactory
{
    /// <summary>
    /// Creates a MSU list control for displaying all of the loaded MSUs
    /// </summary>
    /// <param name="selectionMode">If the user can select one or multiple MSUs at a time</param>
    /// <returns>The generated MSU list control</returns>
    public MsuList CreateMsuList(SelectionMode selectionMode = SelectionMode.Multiple);

    /// <summary>
    /// Creates a MSU list control for displaying all of the loaded MSUs
    /// </summary>
    /// <param name="msuType">The MSU type to filter by</param>
    /// <param name="msuFilter">How precise the MSU type filtering should be</param>
    /// <param name="selectionMode">If the user can select one or multiple MSUs at a time</param>
    /// <param name="selectedMsuPaths">The list of paths the user perviously selected</param>
    /// <returns>The generated MSU list control</returns>
    public MsuList CreateMsuList(MsuType msuType, MsuFilter msuFilter, SelectionMode selectionMode,
        ICollection<string>? selectedMsuPaths = null);

    /// <summary>
    /// Displays the user settings window and saves the results if they were modified
    /// </summary>
    /// <returns>If the window was modified</returns>
    public bool OpenUserSettingsWindow();

    /// <summary>
    /// Displays the window that will reshuffle the MSU on an interval
    /// </summary>
    public void OpenContinousShuffleWindow();

    /// <summary>
    /// Opens the MSU details window for a given MSU
    /// </summary>
    /// <param name="msu">The MSU details to open the window for</param>
    public void OpenMsuDetailsWindow(Msu msu);

    /// <summary>
    /// Opens the MSU Randomizer main window
    /// </summary>
    /// <param name="selectionMode">Whether the MSU list should be in single or multi select mode</param>
    /// <param name="asDialog">If the window should be opened as a dialog window</param>
    /// <param name="selectedOptions">The object of the opens selected by the user</param>
    /// <returns>True if the user accepted the dialog, false otherwise</returns>
    public bool OpenMsuWindow(SelectionMode selectionMode, bool asDialog, out MsuUserOptions selectedOptions);

    /// <summary>
    /// Opens the MSU Monitor for continuously shuffling a request
    /// </summary>
    /// <param name="request">The request to use for reshuffling</param>
    /// <returns>The opened MSU Monitor Window</returns>
    public MsuMonitorWindow OpenMsuMonitorWindow(MsuSelectorRequest request);

    /// <summary>
    /// Opens the MSU Monitor for displaying the current song
    /// </summary>
    /// <param name="msu">The MSU to use for the song information</param>
    /// <returns>The opened MSU Monitor Window</returns>
    public MsuMonitorWindow OpenMsuMonitorWindow(Msu msu);

    /// <summary>
    /// Opens the MSU Monitor using the user options
    /// </summary>
    /// <returns>The opened MSU Monitor Window</returns>
    public MsuMonitorWindow OpenMsuMonitorWindow();
}