using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibraryTests;

[NonParallelizable]
public class TrackDisplayTests
{
    [Test]
    public void TestHorizontalTrack()
    {
        var track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        var text = track.GetDisplayText(TrackDisplayFormat.Horizontal);
        Assert.That(text, Is.EqualTo("Album Name - Song Name (Artist Name)"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.Horizontal);
        Assert.That(text, Is.EqualTo("Album Name - Song Name"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.Horizontal);
        Assert.That(text, Is.EqualTo("Song Name (Artist Name)"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.Horizontal);
        Assert.That(text, Is.EqualTo("Song Name from MSU Name by MSU Creator"));
    }
    
    [Test]
    public void TestVerticalTrack()
    {
        var track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        var text = track.GetDisplayText(TrackDisplayFormat.Vertical);
        Assert.That(text, Is.EqualTo("MSU: MSU Name by MSU Creator\r\n" +
                                     "Album: Album Name\r\n" +
                                     "Song: Song Name\r\n" +
                                     "Artist: Artist Name"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.Vertical);
        Assert.That(text, Is.EqualTo("MSU: MSU Name by MSU Creator\r\n" +
                                     "Song: Song Name"));
    }
    
    [Test]
    public void TestHorizontalWithMsuTrack()
    {
        var track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        var text = track.GetDisplayText(TrackDisplayFormat.HorizonalWithMsu);
        Assert.That(text, Is.EqualTo("Album Name: Song Name - Artist Name (MSU: MSU Name by MSU Creator)"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.HorizonalWithMsu);
        Assert.That(text, Is.EqualTo("Song Name - Artist Name (MSU: MSU Name by MSU Creator)"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.HorizonalWithMsu);
        Assert.That(text, Is.EqualTo("Album Name: Song Name (MSU: MSU Name by MSU Creator)"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.HorizonalWithMsu);
        Assert.That(text, Is.EqualTo("Song Name (MSU: MSU Name by MSU Creator)"));
    }
    
    [Test]
    public void TestSentenceStyle()
    {
        var track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        var text = track.GetDisplayText(TrackDisplayFormat.SentenceStyle);
        Assert.That(text, Is.EqualTo("Song Name by Artist Name from album Album Name from MSU Pack MSU Name by MSU Creator"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.SentenceStyle);
        Assert.That(text, Is.EqualTo("Song Name by Artist Name from MSU Pack MSU Name by MSU Creator"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.SentenceStyle);
        Assert.That(text, Is.EqualTo("Song Name from album Album Name from MSU Pack MSU Name by MSU Creator"));
    }
    
    [Test]
    public void TestSpeechStyle()
    {
        var track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        var text = track.GetDisplayText(TrackDisplayFormat.SpeechStyle);
        Assert.That(text, Is.EqualTo("Song Name; by Artist Name; from album Album Name; from MSU Pack MSU Name; by MSU Creator"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Artist = "Artist Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.SpeechStyle);
        Assert.That(text, Is.EqualTo("Song Name; by Artist Name; from MSU Pack MSU Name; by MSU Creator"));
        
        track = new Track()
        {
            TrackName = "Track Name",
            SongName = "Song Name",
            Album = "Album Name",
            MsuName = "MSU Name",
            MsuCreator = "MSU Creator",
            Msu = new Msu()
            {
                Name = "MSU Object Name",
                Creator = "MSU Object Creator"
            },
            OriginalMsu = new Msu()
            {
                Name = "Original MSU Name",
                Creator = "Original MSU Creator"
            }
        };

        text = track.GetDisplayText(TrackDisplayFormat.SpeechStyle);
        Assert.That(text, Is.EqualTo("Song Name; from album Album Name; from MSU Pack MSU Name; by MSU Creator"));
    }
}