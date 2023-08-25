# MSU Randomizer

A multi game MSU randomizer originally created for the [SMZ3 Cas' Randomizer](https://github.com/Vivelin/SMZ3Randomizer). Select which MSUs you want as possibilities, then either have it shuffle them all together or have it pick a random one for you!

![image](https://github.com/MattEqualsCoder/MSURandomizer/assets/63823784/ee1f36bb-e0cb-4c28-b1e1-fbb08d8caa9b)

## Features

- **Support for over 20 games and randomizers** - Randomize A Link to the Past, Super Metroid, SMZ3, Donkey King Country, Super Mario World, and others! Built on configs managed by [Minnie Trethewey](https://github.com/miketrethewey).
- **Auto detects MSUs** - Simply give the MSU Randomizer the parent folder for all of your MSUs, and it'll automatically detect all MSUs and try to determine what type of MSU it is. Each supported game can have its own parent MSU directory.
- **Assign MSUs to a rom or folder** - Apply a specific MSU to a rom or copy it to a folder, or let the MSU randomizer pick a random one for you!
- **Create shuffled MSUs** - Select multiple MSUs and let the MSU randomizer create a shuffled mix of songs pulling from each of the selected MSUs. Each track will be used in its proper location, so no boss tracks for dungeon themes!
- **Continously shuffled MSUs** - Tired of listening to the same Hyrule Field track an entire run? Use the continuously shuffled feature to allow the MSU randomizer to change all of the songs every minute! (The current playing track will not be swapped out.)
- **Convert MSUs of different types** - Combine ALttP and Super Metroid MSUs into a single SMZ3 MSU, or use an SMZ3 MSU for ALttP or Super Metroid! Or, if you have an old SMZ3 MSU built around the old Conn patch or the original SMZ3 Cas' MSU support, let the MSU Randomizer update them to match the new built-in MSU support for SMZ3.
- **Compile detailed MSU information** - MSU providers can create [YAML files](Docs/yaml.md) to package along with their MSUs. This will allow the MSU Randomizer to pull information about the MSU such as its name and creator as well as track details such as song names and artists, then create a YAML file with the outputted MSU details so that you (or any application) can view the songs picked.
- **Alternative track support** - Sometimes you just have too many ideas for a song! MSU creators can package in alternative pcm files for tracks which will be picked up and randomly selected instead of the default ones. The YAML detail files even allows for adding information for each of the alt tracks so that the MSU Randomizer will know which one was picked and save the relevant info to the output YAML file.
- **Favorite MSU packs** - Favorite MSU packs so that they appear at the top of the list. You can also filter on just the MSU packs you've favorited!

## Setup

Download the latest exe installer from the [GitHub Releases](https://github.com/MattEqualsCoder/MSURandomizer/releases) page. It should automatically install all required dependencies.

## Planned Features

- Possible built-in support for determining which track is currently playing.

## Troubleshooting

Having problems? You can view the [support document](Docs/support.md) for more assistance. If you are still running into problems, please feel free to [post an Issue on GitHub](https://github.com/MattEqualsCoder/MSURandomizer/issues).

## Randomizer Libraries

Interested in using the MSU Randomizer libraries in your own .net based application? Lookup MattEqualsCoder.MSURandomizer.Library and MattEqualsCoder.MSURandomizer.UI on nuget to use them in your project!

- [MSU Randomizer Library Documentation](./MSURandomizerLibrary/README.md)
- [MSU Randomizer UI Documentation](./MSURandomizerUI/README.md)

## Credits & Thanks

- [Vivelin](https://vivelin.net/) for the [SMZ3 Cas' Randomizer](https://github.com/Vivelin/SMZ3Randomizer), from which I borrowed some code snippets here and there.
- [PinkKittyRose](https://www.twitch.tv/pinkkittyrose) for testing this and [the_betus](https://www.twitch.tv/the_betus) for playing it so enthusiastically for me to keep working on it.
- [Minnie Trethewey](https://github.com/miketrethewey) for creating all of the JSON files that are used for the MSU Randomizer.
- All the MSU creators that made enough MSUs to make this worth it.
