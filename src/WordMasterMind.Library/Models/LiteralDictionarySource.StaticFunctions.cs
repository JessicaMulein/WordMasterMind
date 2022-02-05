using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial record LiteralDictionarySource
{
    public static LiteralDictionarySource ScrabbleDictionarySource => new(
        sourceType: LiteralDictionarySourceType.Scrabble,
        fileName: "collins-scrabble.bin",
        fileType: LiteralDictionaryFileType.Binary,
        description: "Collins Scrabble dictionary");


    public static LiteralDictionarySource CrosswordDictionarySource => new(
        sourceType: LiteralDictionarySourceType.Crossword,
        fileName: "crossword-words.bin",
        fileType: LiteralDictionaryFileType.Binary,
        description: "Common crossword words");

    public static LiteralDictionarySource EnglishDictionarySource => new(
        sourceType: LiteralDictionarySourceType.English,
        fileName: "english-semifull.bin",
        fileType: LiteralDictionaryFileType.Binary,
        description: "English (simplified)");
}