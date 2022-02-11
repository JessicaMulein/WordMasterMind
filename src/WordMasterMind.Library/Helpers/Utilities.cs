using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using static System.Threading.Thread;

namespace WordMasterMind.Library.Helpers;

public static class Utilities
{
    /// <summary>
    ///     Gets the path to the test project's root directory.
    /// </summary>
    public static string GetTestRoot(string? fileName = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var startupPath = Path.GetDirectoryName(path: assembly.Location);
        Debug.Assert(condition: startupPath != null,
            message: nameof(startupPath) + " != null");
        var pathItems = startupPath.Split(separator: Path.DirectorySeparatorChar);
        var pos = pathItems.Reverse().ToList().FindIndex(match: x => string.Equals(a: "bin",
            b: x));
        var basePath = string.Join(separator: Path.DirectorySeparatorChar.ToString(),
            values: pathItems.Take(count: pathItems.Length - pos - 1));
        return fileName is null
            ? basePath
            : Path.Combine(path1: basePath,
                path2: fileName);
    }

    public static string GetWebRoot(string? fileName = null)
    {
        return Path.Combine(
            path1: Directory.GetCurrentDirectory(),
            path2: "wwwroot",
            path3: fileName ?? string.Empty);
    }

    public static int HumanizeIndex(int index)
    {
        return index + 1;
    }

    public static string NumberToOrdinal(int number, CultureInfo? culture = null)
    {
        var numberString = number.ToString(
            format: "#,0",
            provider: culture ?? CurrentThread.CurrentCulture);
        // Examine the last 2 digits.
        var lastDigits = number % 100;
        // If the last digits are 11, 12, or 13, use th. Otherwise:
        if (lastDigits is >= 11 and <= 13) return $"{numberString}th";
        // Check the last digit.
        return (lastDigits % 10) switch
        {
            1 => $"{numberString}st",
            2 => $"{numberString}nd",
            3 => $"{numberString}rd",
            _ => $"{numberString}th",
        };
    }
}