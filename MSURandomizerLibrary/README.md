# MSU Randomizer Library

This is a library which can be included in other .NET projects for looking up MSUs and randomly selecting them or shuffling the tracks.

## Services

- **IMsuRandomizerInitializationService** - Service used to initialize the most critical services for loading the app settings and MSU types.
- **IMsuAppSettingsService** - Used to load the MSU Randomizer app settings for global options.
- **IMsuUserOptionsService** - Used to load and save options for the user, such as the MSU directories and MSU overrides.
- **IMsuTypeService** - Loads MSU types from a folder of JSON configs or a single JSON config.
- **IMsuLookupService** - Scans folders for MSUs and automatically determines the correct MSU type for them.
- **IMsuDetailsService** - Loads or saves the MSU details for a given MSU from YAML or JSON.
- **IMsuSelectorService** - Used for randomly selecting/shuffling MSUs and saving them.
- **IMsuUiFactory** - Factory for generating UI elements to display to the user.