using System.ComponentModel;

namespace MSURandomizerLibrary;

/// <summary>
/// Enum for how to output the text for a song
/// </summary>
public enum TrackDisplayFormat
{
    /// <summary>
    /// Original horizontal style: displays the current track without MSU pack
    /// info if artist information is available.
    /// </summary>
    [Description("Single line (song only): \"A Link to the Past OST - Majestic Castle (Koji Kondo)\"")]
    Horizontal,

    /// <summary>
    /// Vertical style: separate lines for separate tracks, with MSU pack info.
    /// </summary>
    [Description("Multiple lines: MSU/album/song/artist")]
    Vertical,

    /// <summary>
    /// Horizontal style: displays the current track with MSU pack info.
    /// </summary>
    [Description("Single line (including MSU): \"A Link to the Past OST: Majestic Castle - Koji Kondo (MSU: ALttP Music by SomeMSUCreator)\"")]
    HorizonalWithMsu,

    /// <summary>
    /// Expanded sentence-style: displays track and MSU pack info in a single
    /// (long) line.
    /// </summary>
    [Description("Sentence: \"Majestic Castle by Koji Kondo from album A Link to the Past OST from MSU pack ALttP Music by SomeMSUCreator\"")]
    SentenceStyle,
    
    /// <summary>
    /// Expanded sentence-style: displays track and MSU pack info in a single
    /// (long) line split apart with semi-colons for text-to-speech
    /// </summary>
    [Description("Sentence: \"Majestic Castle by Koji Kondo; from album A Link to the Past OST; from MSU pack ALttP Music; by SomeMSUCreator\"")]
    SpeechStyle,
}