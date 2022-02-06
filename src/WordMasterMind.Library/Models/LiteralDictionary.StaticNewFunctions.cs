using System.Text.Json;
using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial class LiteralDictionary
{
    public static IEnumerable<string> JsonToWords(string jsonText)
    {
        return JsonSerializer
            .Deserialize<string[]>(
                json: jsonText,
                options: JsonSerializerOptions) ?? throw new InvalidOperationException();
    }

    /// <summary>
    ///     This constructor creates a list of words from a JSON file with an array of strings containing the words
    ///     It will get passed through FillDictionary and then the standard constructor
    /// </summary>
    /// <param name="jsonText"></param>
    public static LiteralDictionary NewFromJson(LiteralDictionarySourceType sourceType, string jsonText,
        string? description = null)
    {
        return new LiteralDictionary(
            sourceType: sourceType,
            words: JsonToWords(
                jsonText: jsonText),
            description: description);
    }

    /// <summary>
    ///     Creates a literal dictionary from a source
    /// </summary>
    /// <param name="source"></param>
    /// <param name="basePath"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static LiteralDictionary NewFromSource(LiteralDictionarySource source, string basePath)
    {
        var fileName = Path.Combine(path1: basePath,
            path2: source.FileName);
        return source.FileType switch
        {
            LiteralDictionaryFileType.TextWithNewLines => new LiteralDictionary(
                sourceType: source.SourceType,
                words: File.ReadLines(path: fileName),
                description: source.Description),
            LiteralDictionaryFileType.JsonStringArray => NewFromJson(
                sourceType: source.SourceType,
                jsonText: File.ReadAllText(path: fileName),
                description: source.Description),
            LiteralDictionaryFileType.Binary => Deserialize(
                sourceType: source.SourceType,
                inputStream: OpenFileForRead(
                    fileName: fileName),
                description: source.Description),
            _ => throw new Exception(message: "Unknown file type"),
        };
    }

    /// <summary>
    ///     Creates a literal dictionary from a source type
    /// </summary>
    /// <param name="sourceType"></param>
    /// <param name="basePath"></para>
    ///     <returns></returns>
    ///     <exception cref="Exception"></exception>
    public static LiteralDictionary NewFromSourceType(LiteralDictionarySourceType sourceType, string basePath)
    {
        return NewFromSource(
            source: LiteralDictionarySource.FromSourceType(sourceType: sourceType),
            basePath: basePath);
    }
}