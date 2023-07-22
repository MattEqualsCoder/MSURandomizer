# Support

## MSU being detected as the incorrect type

Unfortunately due to the number of MSU types with overlapping track counts and with MSUs providing a variable number of tracks, it can be hard to 100% identify every MSU as the correct MSU type. For example, an MSU for A Link to the Past can have anywhere between 1-61 tracks. An MSU for Super Metroid can have between 30-40 tracks. So if an MSU has 38 tracks, it's hard to determine if it should be for A Link to the Past or Super Metroid. The MSU Randomizer does the best it can, but it can definitely get some wrong.

If an MSU is being picked up incorrectly, you can do the following:

1. If you are an MSU creator, you can create a [YAML file](yaml.md) and add the msu_type property to it. The msu_type property needs to match the meta->name property in the tracks.json file for the different MSU type configs [located here in the ALttpMSUShuffer repository](https://github.com/MattEqualsCoder/ALttPMSUShuffler/tree/release/resources/snes).
2. You can split out MSUs into specific folders for each game/MSU type and point the MSU Randomizer to the specific folders.
3. If you aren't the MSU creator, you can right click on an MSU in the MSU list, click on the Open Details option, and then change the MSU type there.

## SuperNT/FXPak Pro support

Currently it is impossible to push directly from the MSU Randomizer to hardware, and such a feature is currently not planned. This means that the continuous shuffle feature can't be used with hardware, and any other randomization will have to be manually copied through another tool or piece of software.

## Disabling alternate tracks

If you right click on an MSU in the MSU list, click on the Open Details option, there is an option there to disable using alternate tracks. Currently there is no plan to allow you to select which tracks are used and which ones aren't.

## Vanilla music is playing instead of MSU music

There are s vouplr reasons why the vanilla music may be playing.

1. What you're playing does not support MSUs. While A Link to the Past, SMZ3 Cas' Randomizer, SMZ3 mainline (currently beta only as of July 21st, 2023), and some other randomizers have built in MSU support, other games may need patches. You can view [this thread on Zeldix](https://www.zeldix.net/t2684-alphabetical-list-every-snes-msu-1-hack) for a list of all games with MSU patches and details on how to patch them.
2. Your emulator or hardware does not support MSUs. Make sure you're using something compatible with MSUs, such as snes9x, BSNES, or FXPak Pro.

## Renaming MSUs

By default the MSU Randomizer uses the folder name as the name of the MSU if it does not have a YAML file. It can also pull MSU names from a msupcm++ json file if one is found. If that name is not sufficient, you can edit the MSU name and creator by right clicking on an MSU in the MSU list, clicking on Open Details, and typing in the text you want there.

## Having other problems?

If you run into any problems, please [post an Issue on GitHub](https://github.com/MattEqualsCoder/MSURandomizer/issues). In the issue, please upload any screenshots, details about which MSU(s) you're trying to use, and post the relevant log file (found at %localappdata%\MSURandomizer).