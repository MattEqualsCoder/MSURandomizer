using System.Text;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary;

internal class TrackDisplayTextBuilder(Track track)
{
    private readonly List<string> _parts = new();

    public TrackDisplayTextBuilder AddAlbum(string format)
    {
        if (!string.IsNullOrWhiteSpace(track.DisplayAlbum))
        {
            _parts.Add(string.Format(format, track.DisplayAlbum));
        }

        return this;
    }
    
    public TrackDisplayTextBuilder AddArtist(string format)
    {
        if (!string.IsNullOrWhiteSpace(track.DisplayArtist))
        {
            _parts.Add(string.Format(format, track.DisplayArtist));
        }

        return this;
    }
    
    public TrackDisplayTextBuilder AddSongName(string format)
    {
        if (!string.IsNullOrWhiteSpace(track.SongName))
        {
            _parts.Add(string.Format(format, track.SongName));
        }

        return this;
    }
    
    public TrackDisplayTextBuilder AddMsuNameAndCreator(string format)
    {
        var name = track.GetMsuName();
        if (!string.IsNullOrWhiteSpace(name))
        {
            _parts.Add(string.Format(format, name));
        }

        return this;
    }
    
    public TrackDisplayTextBuilder AddMsuName(string format)
    {
        if (!string.IsNullOrWhiteSpace(track.MsuName))
        {
            _parts.Add(string.Format(format, track.MsuName));
        }

        return this;
    }
    
    public TrackDisplayTextBuilder AddMsuCreator(string format, string? fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(track.MsuCreator))
        {
            _parts.Add(string.Format(format, track.MsuCreator));
        }

        return this;
    }
    
    public TrackDisplayTextBuilder AddOriginalTrackName(string format, string? fallback = "")
    {
        if (!string.IsNullOrEmpty(track.OriginalTrackName) && track.OriginalTrackName != track.TrackName)
        {
            _parts.Add(string.Format(format, track.OriginalTrackName));
        }

        return this;
    }

    public string ToString(string divider) => string.Join(divider, _parts);

    public override string ToString() => ToString(" ");
}