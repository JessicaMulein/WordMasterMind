using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial record LiteralDictionarySource
{
    public static LiteralDictionarySource LiteralDictionary => new(
        sourceType: LiteralDictionarySources.Scrabble,
        fileName: "scrabble-dictionary.bin",
        fileType: LiteralDictionaryFileType.Binary,
        description: "Scrabble dictionary");


    public static LiteralDictionarySource CrosswordDictionary => new(
        sourceType: LiteralDictionarySources.Crossword,
        fileName: "crossword-words.bin",
        fileType: LiteralDictionaryFileType.Binary,
        description: "Common crossword words");

    public static LiteralDictionarySource EnglishDictionary => new(
        sourceType: LiteralDictionarySources.English,
        fileName: "english-semifull.bin",
        fileType: LiteralDictionaryFileType.Binary,
        description: "English (simplified)");

    /// <summary>
    ///     Creates a literal dictionary from the source
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
            LiteralDictionaryFileType.JsonStringArray => Models.LiteralDictionary.NewFromJson(
                pathToDictionaryJson: source.FileName,
                description: source.Description),
            LiteralDictionaryFileType.Binary => Models.LiteralDictionary.Deserialize(
                inputFilename: source.FileName,
                description: source.Description),
            _ => throw new Exception(message: "Unknown file type")
        };
    }

    public static LiteralDictionary NewFromSourceType(LiteralDictionarySources sourceType)
    {
        return sourceType switch
        {
            LiteralDictionarySources.Scrabble =>
                NewFromSource(source: LiteralDictionary),
            LiteralDictionarySources.Crossword =>
                NewFromSource(source: CrosswordDictionary),
            LiteralDictionarySources.English =>
                NewFromSource(source: EnglishDictionary),
            _ => throw new Exception(message: "Unknown source type")
        };
    }
}