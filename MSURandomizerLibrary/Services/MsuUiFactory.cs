using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.UI;

namespace MSURandomizerLibrary.Services;

public class MsuUiFactory : IMsuUiFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMsuUserOptionsService _msuUserOptionsService;
    private readonly IMsuTypeService _msuTypeService;
    private readonly IMsuLookupService _msuLookupService;
    private readonly ILogger<MsuUiFactory> _logger;

    public MsuUiFactory(IServiceProvider serviceProvider, IMsuUserOptionsService msuUserOptionsService, IMsuTypeService msuTypeService, IMsuLookupService msuLookupService, ILogger<MsuUiFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _msuUserOptionsService = msuUserOptionsService;
        _msuTypeService = msuTypeService;
        _msuLookupService = msuLookupService;
        _logger = logger;
    }

    public MsuList CreateMsuList(SelectionMode selectionMode = SelectionMode.Multiple)
    {
        var userOptions = _msuUserOptionsService.MsuUserOptions;
        var msuType = _msuTypeService.GetMsuType(userOptions.OutputMsuType) ?? _msuTypeService.MsuTypes.First();
        return CreateMsuList(msuType, userOptions.Filter, selectionMode, userOptions.SelectedMsus);
    }
    
    public MsuList CreateMsuList(MsuType msuType, MsuFilter msuFilter, SelectionMode selectionMode,
        ICollection<string>? selectedMsuPaths = null)
    {
        var viewModel = _serviceProvider.GetRequiredService<MsuListViewModel>();
        if (viewModel == null)
        {
            throw new InvalidOperationException();
        }
        viewModel.MsuType = msuType;
        viewModel.MsuFilter = msuFilter;
        viewModel.SelectionMode = selectionMode;
        if (selectedMsuPaths != null)
        {
            viewModel.SelectedMsuPaths = selectedMsuPaths;
        }

        var msuList = _serviceProvider.GetRequiredService<MsuList>();
        msuList.SetDataContext(viewModel);
        return msuList;
    }

    public bool OpenUserSettingsWindow()
    {
        var window = _serviceProvider.GetRequiredService<MsuUserSettingsWindow>();
        if (window.ShowDialog() != true) return false;
        _msuUserOptionsService.Save();
        return true;
    }
    
    public void OpenContinousShuffleWindow()
    {
        var window = _serviceProvider.GetRequiredService<MsuContinuousShuffleWindow>();
        if (window.ShowDialog() != true) return;
        _msuUserOptionsService.Save();
    }

    public void OpenMsuDetailsWindow(Msu msu)
    {
        var msuTypeNames = _msuTypeService.MsuTypes.OrderBy(x => x.Name).Select(x => x.Name).ToList();
        msuTypeNames.Insert(0, _msuTypeService.GetMsuTypeName(null));
        var viewModel = new MsuDetailsViewModel(msu, msuTypeNames);
        var window = _serviceProvider.GetRequiredService<MsuDetailsWindow>();
        window.ViewModel = viewModel;
        if (window.ShowDialog() != true) return;
        var msuType = _msuTypeService.GetMsuType(viewModel.MsuTypeName);
        viewModel.ApplyChanges(msu, msuType);
        _msuUserOptionsService.SaveMsuSettings(msu);
        _msuLookupService.RefreshMsu(msu);
    }
}