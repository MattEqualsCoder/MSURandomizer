# MSU YAML Documentation
MSU creators can provide YAML files along with their MSUs to provide various details such as the name & creator of the MSU, the desired MSU type of the pack, and detailed track information. These details will be used by the MSU Randomizer to better display the MSU in the MSU list, create a YAML file that compiles all of the randomly selected tracks for viewing by the user, and in the future can even be used for displaying the current song being played.

## What is YAML?

YAML (aka *Yet Another Markup Language* or *YAML Ain't Markup Language*) is a language for storing data. It's meant to be easier and more readible than other data storage language such as XML and JSON. While fully teaching YAML is outside of the scope of this document, here is a basic example:

```YAML
name: Matt
interests:
 - name: programming
   interest_level: medium
 - name: randomizers
   interest_level: high
```

Some basics to keep in mind for YAML:

 - Spacing at the start of each line is very important. Any time you have an object, all of its properties need to have the same spacing. If you starting an object that is inside of another object, you will need to indent.
 - Some characters are not allowed in strings. If you have any of the following characters in text, you may need to surround the entire text with quotes ("): :, {, }, [, ], ,, &, *, #, ?, |, -, <, >, =, !, %, @, \
 - Dashes (-) indicate lists/arrays. Each item within an array will need to have a dash in front of it.
 - Pounds/hashtags (#) can be used for comments. Anything after them on a line will be ignored and can be used for readability.

 ## Basic Format

 YAML comes in two basic formats: one for ALttP/Super Metroid/SMZ3 and one for all other MSUs. The main difference is that ALttP/Super Metroid/SMZ3 are made special to make the track listing a bit cleaner.

 ### SMZ3 Example
 ```YAML
# Template with all 102 tracks with Zelda tracks first
pack_name: My SMZ3 MSU Pack
pack_author: Matt
artist: 
album: 
tracks:
  # Track Number: 01
  zelda_title:
    name: The Best Song
    artist: Me
    album: My Best OST
  # Track Number: 02
  light_world:
    name: The Brightest of Worlds
    artist: Me
    album: My Second Best OST
 ```
The top portion is for information that is global, such as the MSU name/author and things like the artist and album which will be used if nothing is specified for the tracks. Following that is all of the tracks and their details. In these, the name of the song in A Link to the Past is used as a key to denote each track (zelda_title, light_world, etc.)

### Generic Example
```YAML
# Generic template for all MSU types outside of Zelda, Super Metroid, and SMZ3
pack_name: My Generic MSU Pack
pack_author: Matt
artist: 
album: 
tracks:
- track_number: 1
  name: The Best Song
  artist: Me
  album: My Best OST
- track_number: 2
  name: The Brightest of Worlds
  artist: Me
  album: My Second Best OST
```
The first few lines work the same, containing global and shared information. The tracks, however, are an array/list of tracks that use the track_number field to help map which details go to which song. Make sure these track numbers match the numbers of the pcm file. For example, if your msu is dkc-msu.msu, then dkc-msu-5.msu will match the track details where track_number: 5.

## Field Descriptions

### Global Fields

* pack_name - The name of the MSU pack for displaying in the MSU list
* pack_author - The creator of the MSU pack for displaying in the MSU list
* pack_version - Number to represent which version of the pack the user has downloaded
* msu_type - The name of the MSU type to force for the MSU if it is being picked up incorrectly by default
* artist - The default artist for the songs if they aren't specified in each track
* album - The default album for all of the songs if they aren't specified in each track
* url - The default url to obtain all of the songs if they aren't specified in each track

### Track Fields

* track_number - The number of the PCM file for this track (ignored for ALTTP/SM/SMZ3 YAML files)
* track_name - The name of the track (ignored and is for reference only)
* name - The name of the song
* artist - The name of the artist of the song
* album - The name of the album with the song on it
* url - The url to obtain the song or album
* alts - A list of all of the possible alternative songs for this track (see Alternative Songs below)
* path - The relative path to the file (assuming it's not currently the main pcm file) to determine alt tracks
* file_length - The number of bytes in the file to determine alt tracks
* hash - A sha-1 hash of the file to determine alt tracks

## Steps for MSU Creators

Are you an MSU creator looking to make a YAML file? Here are the steps you'll want to follow:

1. Using one of the templates below, create a YAML file for your MSU. The file should be named the same as your MSU, only with .yml instead of .msu as the file extension. For example, if your msu is my-alttpr-msu.msu, then the YAML file will be my-alttpr-msu.yml. If you have a msupcm++ tracks JSON file, you can also use the [MSU Json Converter](https://msu.celestialrealm.net/index.html), which will convert that JSON into YAML, pulling in all fields. Note that not all fields can be pulled over.
2. Fill out all of the fields using the editor of your choice. I'd recommend [Visual Studio Code](https://code.visualstudio.com/).
3. Open the MSU Randomizer, making sure your MSU is in a directory it'll detect. Upon start up you may get a popup alerting you of an issue with your MSU. Not all of the errors are *super* helpful, but it'll hopefully direct you to the line number with the issue.
4. When you get no errors on loading the YAML file, right click on the MSU in the list and click the option to view its details. Scan through all the songs and make sure the information seems correct.

## Examples & Templates

The following are templates you can use to start editing for your MSU.

* [A Link to the Past](Examples/z3_all_tracks.yml)
* [Super Metroid](Examples/sm_all_tracks.yml)
* [SMZ3 Combo Randomizer (all tracks)](Examples/smz3_zelda_first_all_tracks.yml)
* [SMZ3 Combo Randomizer (base tracks)](Examples/smz3_zelda_first_required_tracks.yml)
* [SMZ3 Classic (Metroid first - all tracks)](Examples/smz3_metroid_first_all_tracks.yml)
* [SMZ3 Classic (Metroid first - base tracks)](Examples/smz3_metroid_first_required_tracks.yml)
* [Generic](Examples/generic.yml)


## Forcing a specific MSU type

Sometimes the MSU Randomizer may have issues automatically detecting the MSU type. While this is usually not a problem if you include all tracks, sometimes if you skip some tracks it may confuse it with another type. In order to solve this, in the global settings, there is an msu_type property. The msu_type property needs to match the meta->name property in the tracks.json file for the different MSU type configs [located here in the ALttpMSUShuffer repository](https://github.com/MattEqualsCoder/ALttPMSUShuffler/tree/release/resources/snes).

## Alternative Songs

If you ever have multiple songs which you think fit for a track, you can include them as alternate tracks which will be picked up automatically by the MSU Randomizer. If no YAML file is provided, it will detect pcm files that have the same starting path as the main pcm file but with a non-numeric value following the number. For example, if the main pcm file is my-alttpr-msu-5.msu, it will detect tracks such as my-alttpr-msu-5_alt.pcm, my-alttpr-msu-5(alt).pcm, etc.

If a YAML file is included, however, you need to include the alt tracks in the alt section for a track, like in the following example:

```YAML
pack_name: Ocarina of Time MSU
pack_author: MattEqualsCoder
pack_version: 2
artist: Koji Kondo
album: Ocarina of Time OST
tracks:
  item_room:
    name: Great Fairy’s Fountain
    file_length: 3625964
    hash: 152AD6CDCE693ACD62222AC40809F704424B066D
    path: smz3-oot-3_original.pcm
    alts:
      - name: Garden of Venus
        artist: RebeccaETripp
        album: OCRemix
        file_length: 21038184
        hash: FF1F7D5F318BC1536424B066D25D0A7DD6D87D87
        path: alts/smz3-oot-3_alt1.pcm
        url: https://ocremix.org/remix/OCR03924
      - name: La Gran Fuente de Salsa
        artist: MrKyle
        album: OCRemix
        file_length: 45654486
        hash: 62ADF55CEAD8BCFABCF3DE3441D6D8762FADEEFF
        path: alts/smz3-oot-3_alt2.pcm
        url: https://ocremix.org/remix/OCR03296
```

In the above example, the item room has the base track (Great Fairy's Fountain) and two alt tracks (Garden of Venus and La Gran Fuente de Salsa). Each version has the following fields, each of them are required for alt tracks

* path - This is the relative path to the track in comparison to the folder of the MSU. That means it can be in the main folder alongside the regular pcm file, or in a subdirectory. Note that this is required even for the base track. This is so that the MSU Randomizer will know where to look for tracks that have been swapped out manually or via script.
* file_length - This is the number of bytes in the file.
* hash - This is a SHA-1 hash of the file contents.

You can determine the file length and hash by using the following commands in PowerShell:

```PowerShell
(Get-Item "smz3_oot-1.pcm").Length
(Get-FileHash "smz3_oot-1.pcm" -Algorithm SHA1).Hash
```

With all these details, when the MSU Randomizer picks alt tracks, it'll know the proper song name, artist, and album.