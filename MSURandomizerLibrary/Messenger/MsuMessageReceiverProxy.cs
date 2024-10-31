using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MsuRandomizerMessenger;

namespace MSURandomizerLibrary.Messenger;

internal class MsuMessageReceiverProxy
{
    public event EventHandler<MsuTrackChangedEventArgs>? TrackChanged;
    public event EventHandler<MsuGeneratedEventArgs>? MsuGenerated;
    
    internal void OnTrackChanged(TrackPlayedRequest request)
    {
        var track = new Track(request.TrackName, request.TrackNumber, request.SongName, "", request.ArtistName,
            request.AlbumName)
        {
            MsuName = request.MsuName,
            MsuCreator = request.ArtistName,
        };
        
        TrackChanged?.Invoke(this, new MsuTrackChangedEventArgs(track));
    }

    internal void OnMsuGenerated(MsuGeneratedRequest request)
    {
        var msuData = new MsuBasicDetails()
        {
            Name = request.MsuName,
            Creator = request.MsuCreator,
            Path = request.MsuPath,
            MsuTypeName = request.MsuType,
        };
        
        MsuGenerated?.Invoke(this, new MsuGeneratedEventArgs(msuData));
    }
}