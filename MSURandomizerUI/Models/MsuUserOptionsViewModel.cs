using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerUI.Models;

internal class MsuUserOptionsViewModel : ViewModel
{

    public MsuUserOptionsViewModel()
    {
    }
    
    public MsuUserOptionsViewModel(MsuUserOptions options)
    {
        _promptOnUpdate = options.PromptOnUpdate;
        _promptOnPreRelease = options.PromptOnPreRelease;
        _sniAddress = options.SnesConnectorSettings.SniAddress;
        _usb2SnesAddress = options.SnesConnectorSettings.Usb2SnesAddress;
        _luaAddress = options.SnesConnectorSettings.LuaAddress;
        _trackDisplayFormat = options.TrackDisplayFormat;
        _msuCurrentSongOutputFilePath = options.MsuCurrentSongOutputFilePath;
    }

    public void UpdateSettings(MsuUserOptions options)
    {
        options.PromptOnUpdate = _promptOnUpdate;
        options.PromptOnPreRelease = _promptOnPreRelease;
        options.SnesConnectorSettings.SniAddress = _sniAddress;
        options.SnesConnectorSettings.Usb2SnesAddress = _usb2SnesAddress;
        options.SnesConnectorSettings.LuaAddress = _luaAddress;
        options.TrackDisplayFormat = _trackDisplayFormat;
        options.MsuCurrentSongOutputFilePath = _msuCurrentSongOutputFilePath;
    }

    private bool _promptOnUpdate;

    public bool PromptOnUpdate
    {
        get => _promptOnUpdate;
        set => SetField(ref _promptOnUpdate, value);
    }
    
    private bool _promptOnPreRelease;

    public bool PromptOnPreRelease
    {
        get => _promptOnPreRelease;
        set => SetField(ref _promptOnPreRelease, value);
    }
    
    private string _sniAddress = "";

    public string SniAddress
    {
        get => _sniAddress;
        set => SetField(ref _sniAddress, value);
    }
    
    private string _usb2SnesAddress = "";

    public string Usb2SnesAddress
    {
        get => _usb2SnesAddress;
        set => SetField(ref _usb2SnesAddress, value);
    }
    
    private string _luaAddress = "";

    public string LuaAddress
    {
        get => _luaAddress;
        set => SetField(ref _luaAddress, value);
    }
    
    private string? _msuCurrentSongOutputFilePath;

    public string? MsuCurrentSongOutputFilePath
    {
        get => _msuCurrentSongOutputFilePath;
        set => SetField(ref _msuCurrentSongOutputFilePath, value);
    }
    
    private TrackDisplayFormat _trackDisplayFormat;

    public TrackDisplayFormat TrackDisplayFormat
    {
        get => _trackDisplayFormat;
        set => SetField(ref _trackDisplayFormat, value);
    }
}