using MSURandomizerLibrary.Configs;

namespace MSURandomizerLibrary.Messenger;

internal class MsuMessageSenderNoOp : IMsuMessageSender
{
    public Task SendTrackChangedAsync(Track track)
    {
        return Task.CompletedTask;
    }

    public Task SendMsuGenerated(Msu msu)
    {
        return Task.CompletedTask;
    }
}