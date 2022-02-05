using System.Diagnostics;
using System.Reflection;

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
            path1: System.IO.Directory.GetCurrentDirectory(),
            path2: "wwwroot",
            path3: fileName ?? string.Empty);
    }
}