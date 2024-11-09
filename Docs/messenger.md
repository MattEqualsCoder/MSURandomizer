# MSU Messenger

Using gPRC, the MSU Randomizer can send details to other applications when a MSU has been generated or when the MSU Randomizer has detected a new track.

When a client application uses the [IMsuMessageReceiver](https://github.com/MattEqualsCoder/MSURandomizer/blob/main/MSURandomizerLibrary/Messenger/IMsuMessageSender.cs), the service will pick a random port and spin up a gPRC server. It will write the address to a file in the MSU Randomizer folder, which the MSU Randomizer application will use to send messages to.

You can find the gRPC proto file [here](https://github.com/MattEqualsCoder/MSURandomizer/blob/main/MSURandomizerLibrary/Messenger/MsuRandomizer.proto), or you can utilize it in any .net application that uses the [MSURandomizer Library nuget package](https://www.nuget.org/packages/MattEqualsCoder.MSURandomizer.Library/).

## Diagram

![MSUMessenger drawio(1)](https://github.com/user-attachments/assets/44af65c5-6e46-498f-b7a1-80e1df7e55ce)

## Basic Example

```CSharp
messageReceiver.Initialize();

messageReceiver.MsuGenerated += (sender, args) =>
{
    logger.LogInformation("MSU {Name} generated", args.Msu.Name);
};

messageReceiver.TrackChanged += (sender, args) =>
{
    logger.LogInformation("Msu track changed to: {SongName}", args.Track.GetDisplayText(TrackDisplayFormat.Horizontal));
};
```
