using MsuRandomizer;

namespace MSURandomizer;

public class MsuRandomizerOptionsFactory
{
    private static MsuRandomizerOptions? _options { get; set; }

    public MsuRandomizerOptions GetOptions()
    {
        if (_options != null) return _options;
        _options = MsuRandomizerOptions.LoadOptions();
        return _options;
    }
}