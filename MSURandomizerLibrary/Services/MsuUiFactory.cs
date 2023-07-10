using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
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
        var msuType = _msuTypeService.GetMsuType(userOptions.OutputMsuType);
        if (msuType == null)
        {
            msuType = _msuTypeService.MsuTypes.First();
        }

        return CreateMsuList(msuType, userOptions.Filter, selectionMode, userOptions.SelectedMsus);
    }
    
    public MsuList CreateMsuList(MsuType msuType, MsuFilter msuFilter, SelectionMode selectionMode,
        ICollection<string>? selectedMsuPaths = null)
    {
        var viewModel = _serviceProvider.GetService<MsuListViewModel>();
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
        return new MsuList(viewModel, _logger);
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
}