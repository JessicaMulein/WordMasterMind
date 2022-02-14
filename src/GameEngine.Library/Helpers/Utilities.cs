using System.Globalization;
using static System.Threading.Thread;

namespace GameEngine.Library.Helpers;

public static class Utilities
{
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