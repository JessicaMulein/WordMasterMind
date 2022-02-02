using System.Text.Json;
using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial class LiteralDictionary
{
    /// <summary>
    ///     This constructor creates a list of words from a JSON file with an array of strings containing the words
    ///     It will get passed through FillDictionary and then the standard constructor
    /// </summary>
    /// <param name="pathToDictionaryJson"></param>
    public static LiteralDictionary NewFromJson(string pathToDictionaryJson, string? description = null)
    {
        return new LiteralDictionary(
            words: JsonSerializer
                .Deserialize<string[]>(
                    json: File.ReadAllText(path: pathToDictionaryJson),
                    options: JsonSerializerOptions) ?? throw new InvalidOperationException(),
            description: description);
    }

    /// <summary>
    ///     Creates a literal dictionary from a source
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static LiteralDictionary NewFromSource(LiteralDictionarySource source)
    {
        return source.FileType switch
        {
            LiteralDictionaryFileType.TextWithNewLines => new LiteralDictionary(
                words: File.ReadLines(path: source.FileName),
                description: source.Description),
            LiteralDictionaryFileType.JsonStringArray => NewFromJson(
                pathToDictionaryJson: source.FileName,
                description: source.Description),
            LiteralDictionaryFileType.Binary => Deserialize(
                inputFilename: source.FileName,
                description: source.Description),
            _ => throw new Exception(message: "Unknown file type"),
        };
    }

    /// <summary>
    ///     Creates a literal dictionary from a source type
    /// </summary>
    /// <param name="sourceType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static LiteralDictionary NewFromSourceType(LiteralDictionarySourceType sourceType)
    {
        return sourceType switch
        {
            LiteralDictionarySourceType.Scrabble =>
                NewFromSource(source: LiteralDictionarySource.ScrabbleDictionarySource),
            LiteralDictionarySourceType.Crossword =>
                NewFromSource(source: LiteralDictionarySource.CrosswordDictionarySource),
            LiteralDictionarySourceType.English =>
                NewFromSource(source: LiteralDictionarySource.EnglishDictionarySource),
            _ => throw new Exception(message: "Unknown source type"),
        };
    }
}