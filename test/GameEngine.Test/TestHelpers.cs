using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GameEngine.Library.Enumerations;
using GameEngine.Library.Models;

namespace GameEngine.Test;

public static class TestHelpers
{
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

    public static Dictionary<int, IEnumerable<string>> GetWordDictionaryDictFromTestRoot(LiteralDictionarySource source)
    {
        var testRoot = GetTestRoot(fileName: source.FileName);
        // this must be set to use the word generator
        DailyWordGenerator.BasePath = testRoot;
        using var stream = LiteralDictionary.OpenFileForRead(
            fileName: testRoot);
        return LiteralDictionary.DeserializeToDictionary(inputStream: stream);
    }

    public static LiteralDictionary GetWordDictionaryFromTestRoot(
        LiteralDictionarySourceType sourceType = LiteralDictionarySourceType.CollinsScrabble)
    {
        var source = LiteralDictionarySource.FromSourceType(sourceType: sourceType);
        return new LiteralDictionary(
            dictionary: GetWordDictionaryDictFromTestRoot(
                source: source),
            sourceType: source.SourceType,
            description: source.Description);
    }
}