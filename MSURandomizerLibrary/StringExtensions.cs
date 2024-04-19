namespace MSURandomizerLibrary;

public static class StringExtensions
{
    public static string ExpandSpecialFolders(this string text)
    {
        text = text.Replace("%LocalAppData%",
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        
        if (!OperatingSystem.IsWindows())
        {
            text = text.Replace("\\", "/");
        }

        return text;
    }
}