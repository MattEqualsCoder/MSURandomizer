# MSU Randomizer Library

This is a library which can be included in other .NET projects for looking up MSUs and randomly selecting them or shuffling the tracks.

## Instructions

### Step 1. Create an settings file

The application uses a YAML settings file to manage base application settings. You can find the base YAML file [here](../MSURandomizerLibrary/settings.yaml). These are loaded by default, and you provide another YAMl file with overrides. You can view all possible settings [here](../MSURandomizerLibrary/Configs/MsuAppSettings.cs).

### Step 2. Add Services to the Service Collection

The MSU Randomizer utilizes DependencyInjection. You'll need to configure a logger, then add both the UI and regular Library services to your Service Collection.

```charp
_host = Host.CreateDefaultBuilder(e.Args)
.ConfigureLogging(...})
.ConfigureServices(services =>
{
    services.AddMsuRandomizerServices();
})
.Start();
```

### Step 3. Initialize the MSU Randomizer Library

You can use the IMsuRandomizerInitialization service to setup all required services. Create a MsuRandomizerInitializationRequest, giving it either a stream or path to your settings YAML file from step 1, then call the IMsuRandomizerInitializationService's Initialize function.

```csharp
var settingsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MSURandomizer.settings.yaml");
if (settingsStream == null)
{
    throw new InvalidOperationException("Missing RandomizerSettings stream");
}

var msuInitializationRequest = new MsuRandomizerInitializationRequest
{
    MsuAppSettingsStream = settingsStream,
};

_host.Services.GetRequiredService<IMsuRandomizerInitializationService>().Initialize(msuInitializationRequest);
```

### Step 4. Use any of the serviices as you'd like

Once setup, you can use any of the MSU Randomizer library services to look up MSUs, randomize MSUs, and more.

- **IMsuRandomizerInitializationService** - Service used to initialize the most critical services for loading the app settings and MSU types.
- **IMsuAppSettingsService** - Used to load the MSU Randomizer app settings for global options.
- **IMsuUserOptionsService** - Used to load and save options for the user, such as the MSU directories and MSU overrides.
- **IMsuTypeService** - Loads MSU types from a folder of JSON configs or a single JSON config.
- **IMsuLookupService** - Scans folders for MSUs and automatically determines the correct MSU type for them.
- **IMsuDetailsService** - Loads or saves the MSU details for a given MSU from YAML or JSON.
- **IMsuSelectorService** - Used for randomly selecting/shuffling MSUs and saving them.
- **IMsuUiFactory** - Factory for generating UI elements to display to the user.