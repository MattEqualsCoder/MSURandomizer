using System.Collections.Generic;
using System.Windows.Controls;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.UI;

namespace MSURandomizerLibrary.Services;

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
}