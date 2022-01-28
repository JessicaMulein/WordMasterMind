using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

/// <summary>
///     Single source of dictionaries for the game.
/// </summary>
public record LiteralDictionarySource
{
    public readonly string Description;
    public readonly string FileName;
    public readonly LiteralDictionaryFileType FileType;
    public readonly LiteralDictionarySources SourceType;

    public LiteralDictionarySource(LiteralDictionarySources sourceType, string fileName,
        LiteralDictionaryFileType fileType, string description)
    {
        this.SourceType = sourceType;
        this.FileName = fileName;
        this.FileType = fileType;
        this.Description = description;
    }

    public static LiteralDictionarySource ScrabbleDictionary => new(
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

    public static IEnumerable<LiteralDictionarySource> Sources => new[]
    {
        ScrabbleDictionary,
        CrosswordDictionary,
        EnglishDictionary
    };
}