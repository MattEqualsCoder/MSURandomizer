namespace MSURandomizerLibrary.Messenger;

internal static class Shared
{
    public static string GrpcUrlFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSURandomizer", "grpc.txt");
}