using System.Collections.Generic;
using System.Windows.Controls;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public interface IMsuUiFactory
{
    public MsuList? CreateMsuList(MsuType msuType, MsuFilter msuFilter, SelectionMode selectionMode,
        ICollection<string>? selectedMsuPaths = null);
}