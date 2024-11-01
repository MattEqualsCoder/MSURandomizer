using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using MsuRandomizerMessenger;

namespace MSURandomizerLibrary.Messenger;

internal class MsuMessageSender(ILogger<MsuMessageSender> logger) : IMsuMessageSender
{
    private MsuRandomizerMessenger.Messenger.MessengerClient? _client;
    private string? _currentUrl;
    private DateTime? _lastWriteTime;

    private string? GetCurrentUrl()
    {
        if (!File.Exists(Shared.GrpcUrlFile)) return _currentUrl;
        var fileInfo = new FileInfo(Shared.GrpcUrlFile);
        if (_lastWriteTime == fileInfo.LastWriteTimeUtc) return _currentUrl;
        _lastWriteTime = fileInfo.LastWriteTimeUtc;
        return File.ReadAllText(Shared.GrpcUrlFile);
    }
    
    private MsuRandomizerMessenger.Messenger.MessengerClient? GetClient()
    {
        var url = GetCurrentUrl();
        if (string.IsNullOrEmpty(url)) return null;
        if (url == _currentUrl) return _client;
        var channel = GrpcChannel.ForAddress(url);
        _client = new MsuRandomizerMessenger.Messenger.MessengerClient(channel);
        _currentUrl = url;
        logger.LogInformation("Using grpc client {Url}", _currentUrl);
        return _client;
    }

    public async Task SendTrackChangedAsync(Track track)
    {
        var client = GetClient();
        if (client == null) return;
        await client.TrackPlayedAsync(new TrackPlayedRequest()
        {
            MsuName = track.MsuName ?? track.OriginalMsu?.DisplayName ?? string.Empty,
            MsuCreator = track.MsuCreator ?? track.OriginalMsu?.DisplayCreator ?? string.Empty,
            TrackName = track.TrackName,
            TrackNumber = track.Number,
            SongName = track.SongName,
            AlbumName = track.DisplayAlbum ?? string.Empty,
            ArtistName = track.DisplayArtist ?? string.Empty,
            Url = track.DisplayUrl ?? string.Empty
        });
    }

    public async Task SendMsuGenerated(Msu msu)
    {
        var client = GetClient();
        if (client == null) return;
        await client.MsuGeneratedAsync(new MsuGeneratedRequest()
        {
            MsuName = msu.Name,
            MsuCreator = msu.Creator ?? string.Empty,
            MsuPath = msu.Path,
            MsuType = msu.MsuTypeName
        });
    }
}