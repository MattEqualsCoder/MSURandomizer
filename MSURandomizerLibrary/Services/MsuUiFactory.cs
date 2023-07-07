using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Services;

public class MsuUiFactory : IMsuUiFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public MsuUiFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public MsuList? CreateMsuList(MsuType msuType, MsuFilter msuFilter, SelectionMode selectionMode,
        ICollection<string>? selectedMsuPaths = null)
    {
        var msuList = _serviceProvider.GetService<MsuList>();
        if (msuList == null)
        {
            throw new InvalidOperationException();
        }
        msuList.TargetMsuType = msuType;
        msuList.MsuFilter = msuFilter;
        msuList.SelectionMode = selectionMode;
        if (selectedMsuPaths != null)
        {
            msuList.SelectedMsuPaths = selectedMsuPaths;    
        }
        return msuList;
    }
}