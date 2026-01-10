using Grpc.Core;
using MsuRandomizerMessenger;

namespace MSURandomizerLibrary.Messenger;

internal class MsuMessageReceiverGrpcService (MsuMessageReceiverProxy proxy) : MsuRandomizerMessenger.Messenger.MessengerBase
{
    public override Task<TrackPlayedResponse> TrackPlayed(TrackPlayedRequest request, ServerCallContext context)
    {
        proxy.OnTrackChanged(request);
        return Task.FromResult(new TrackPlayedResponse());
    }

    public override Task<MsuGeneratedResponse> MsuGenerated(MsuGeneratedRequest request, ServerCallContext context)
    {
        proxy.OnMsuGenerated(request);
        return Task.FromResult(new MsuGeneratedResponse());
    }
}