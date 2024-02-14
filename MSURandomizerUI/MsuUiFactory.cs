using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using MSURandomizerUI.Controls;
using MSURandomizerUI.Models;

namespace MSURandomizerUI;

internal class MsuUiFactory : IMsuUiFactory
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
        var msuTypeNames = _msuTypeService.MsuTypes.OrderBy(x => x.DisplayName).Select(x => x.DisplayName).ToList();
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

    public bool OpenMsuWindow(SelectionMode selectionMode, bool asDialog, out MsuUserOptions selectedOptions)
    {
        var window = _serviceProvider.GetRequiredService<MsuWindow>();
        
        window.Show(selectionMode, asDialog);

        selectedOptions = window.DataContext;

        if (asDialog)
        {
            return window.DialogResult ?? false;
        }
        else
        {
            return true;
        }
    }

    public MsuMonitorWindow OpenMsuMonitorWindow(MsuSelectorRequest request)
    {
        var window = _serviceProvider.GetRequiredService<MsuMonitorWindow>();
        window.Show(request);
        return window;
    }

    public MsuMonitorWindow OpenMsuMonitorWindow(Msu msu)
    {
        var window = _serviceProvider.GetRequiredService<MsuMonitorWindow>();
        window.Show(msu);
        return window;
    }

    public MsuMonitorWindow OpenMsuMonitorWindow()
    {
        var options = _msuUserOptionsService.MsuUserOptions;
        
        if (string.IsNullOrEmpty(options.OutputMsuType))
        {
            _logger.LogError("No output MSU type selected");
            throw new InvalidOperationException("No output MSU type selected");
        }
        
        var msus = _msuLookupService.Msus
            .Where(x => options.SelectedMsus?.Contains(x.Path) == true)
            .ToList();
        
        if (!msus.Any())
        {
            _logger.LogError("No valid MSUs selected");
            throw new InvalidOperationException("No valid MSUs selected");
        }
        
        var msuType = _msuTypeService.GetMsuType(options.OutputMsuType);

        if (msuType == null)
        {
            _logger.LogError("Invalid MSU type");
            throw new InvalidOperationException("Invalid MSU type");
        }
        
        var outputPath = !string.IsNullOrEmpty(options.OutputFolderPath) 
            ? Path.Combine(options.OutputFolderPath, $"{options.Name}.msu") 
            : options.OutputRomPath;

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            _logger.LogError("No output path");
            throw new InvalidOperationException("No output path");
        }

        return OpenMsuMonitorWindow(new MsuSelectorRequest()
        {
            Msus = msus,
            OutputMsuType = msuType,
            OutputPath = outputPath,
            AvoidDuplicates = options.AvoidDuplicates,
            ShuffleStyle = options.MsuShuffleStyle,
            OpenFolder = false
        });
    }

}