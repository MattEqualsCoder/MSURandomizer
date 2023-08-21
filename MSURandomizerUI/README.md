# MSU Randomizer UI Library

This is a library that is built off of the [MSU Randomizer Library](../MSURandomizerLibrary/README.md) which can be included in other .NET projects with the UI elements for the MSU Randomizer.

## Instructions

### Step 1. Follow the MSU Randomizer Library instructions

You'll need to follow the setup instructions for the [MSU Randomizer Library](../MSURandomizerLibrary/README.md) first as this the MSU Randomizer Library is dependent on it.

### Step 2. Add Services to the Service Collection

Add the UI services to your service collection as well.

```charp
_host = Host.CreateDefaultBuilder(e.Args)
.ConfigureLogging(...})
.ConfigureServices(services =>
{
    services.AddMsuRandomizerServices();
    services.AddMsuRandomizerUIServices();
})
.Start();
```

### Step 3. Use the IMsuUiFactory

Create the IMsuUiFactory and use it to create the MSU list to add to a window, launch the MSU Randomizer window, or more.

```csharp
_host.Services.GetRequiredService<IMsuUiFactory>().OpenMsuWindow(SelectionMode.Multiple, false, out _);
```