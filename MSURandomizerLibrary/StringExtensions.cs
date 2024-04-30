namespace MSURandomizerLibrary;

/// <summary>
/// Extensions functions for strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts certain placeholder text with special folder paths
    /// </summary>
    /// <param name="text">The text to change</param>
    /// <returns>The updated text</returns>
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