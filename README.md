# MSU Randomizer

A cross-platform multi-game MSU randomizer originally created for the [SMZ3 Cas' Randomizer](https://github.com/Vivelin/SMZ3Randomizer). Select which MSUs you want as possibilities, then either have it shuffle them all together or have it pick a random one for you!

![311780120-48b71042-85da-47f9-bec9-7a8b6034b4f0](https://github.com/MattEqualsCoder/MSURandomizer/assets/63823784/0fe5c230-1247-44d9-b1d3-84775ba97fed)

## Primary Features

- **Support for over 20 games and randomizers** - Randomize A Link to the Past, Super Metroid, SMZ3, Donkey King Country, Super Mario World, and others! Built on configs managed by [Minnie Trethewey](https://github.com/miketrethewey).
- **Auto detects MSUs** - Simply give the MSU Randomizer the parent folder for all of your MSUs, and it'll automatically detect all MSUs and try to determine what type of MSU it is. Each supported game can have its own parent MSU directory.
- **Apply or randomize MSUs in a variety of ways** - Select a specific MSU and apply it to a folder, have the application pick one for you randomly, or shuffle a bunch of MSUs together, either once or repeatedly! There are even shuffle styles for how to shuffle the tracks together. Using hardware? Connect to your device and pick from MSUs added to the SD card.
- **Compile and display MSU info** - MSU providers can create [YAML files](Docs/yaml.md) to package along with their MSUs. This will allow the MSU Randomizer to pull information about the MSU and the songs. For ALttP and SMZ3, it can even display the currently playing song or output the info to a txt file for displaying in a stream setup.

  ![311783793-36e83d5f-df74-42ea-94ed-d0c53ccd638f](https://github.com/MattEqualsCoder/MSURandomizer/assets/63823784/95548550-e9e6-4de1-8bcf-616178c1ca3f)

## Setup

### Windows
- Download the latest SetupWin exe file from the [GitHub Releases](https://github.com/MattEqualsCoder/MSURandomizer/releases) page.
- Run the setup, and it should automatically install the application and all required dependencies.

### Linux
- Download the latest AppImage file from the [GitHub Releases](https://github.com/MattEqualsCoder/MSURandomizer/releases) page.
- Place the AppImage in the desired folder and make executable.

### Mac

- Install [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Download the latest Mac zip file from the [GitHub Releases](https://github.com/MattEqualsCoder/MSURandomizer/releases) page.
- In the terminal, go to the folder and execute the command `xattr -dr com.apple.quarantine MSURandomizer.app`

## Additional Features

- **Alt tracks** - If an MSU has additional tracks provided in a particular format, the MSU rando can include those tracks when shuffling. You can even contrl the usage of alt tracks per MSU pack.
- **Favorite or alter frequency** - Have an MSU you'd like to hear more or less often? Either favorite the MSU to make it easier to filter, or make it appear twice as often or half as often as other MSUs.
- **Compile detailed MSU information** - MSU providers can create [YAML files](Docs/yaml.md) to package along with their MSUs. This will allow the MSU Randomizer to pull information about the MSU a
- **Convert MSUs of different types** - Combine ALttP and Super Metroid MSUs into a single SMZ3 MSU, or use an SMZ3 MSU for ALttP or Super Metroid! Or, if you have an old SMZ3 MSU built around the old Conn patch or the original SMZ3 Cas' MSU support, let the MSU Randomizer update them to match the new built-in MSU support for SMZ3.
- **Copyright safe tracks** - Mark MSUs or even individual songs as copyright safe or unsafe so that you can shuffle them without concern of copyright strikes.

## Troubleshooting

Having problems? You can view the [support document](Docs/support.md) for more assistance. If you are still running into problems, please feel free to [post an Issue on GitHub](https://github.com/MattEqualsCoder/MSURandomizer/issues).

## Randomizer Libraries

Interested in using the MSU Randomizer libraries in your own .net based application? Lookup MattEqualsCoder.MSURandomizer.Library and MattEqualsCoder.MSURandomizer.UI on nuget to use them in your project!

- [MSU Randomizer Library Documentation](./MSURandomizerLibrary/README.md)

## Credits & Thanks

- [Vivelin](https://vivelin.net/) for the [SMZ3 Cas' Randomizer](https://github.com/Vivelin/SMZ3Randomizer), from which I borrowed some code snippets here and there.
- [PinkKittyRose](https://www.twitch.tv/pinkkittyrose) for testing this and [the_betus](https://www.twitch.tv/the_betus) for playing it so enthusiastically for me to keep working on it.
- [Minnie Trethewey](https://github.com/miketrethewey) for creating all of the JSON files that are used for the MSU Randomizer.
- All the MSU creators that made enough MSUs to make this worth it.
