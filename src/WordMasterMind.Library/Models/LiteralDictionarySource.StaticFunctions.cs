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

    public static IEnumerable<LiteralDictionarySource> Sources
        => Enum.GetValues<LiteralDictionarySourceType>()
            .Select(
                selector: FromSourceType);

    public static LiteralDictionarySource FromSourceType(LiteralDictionarySourceType sourceType)
    {
        return sourceType switch
        {
            LiteralDictionarySourceType.Scrabble => ScrabbleDictionarySource,
            LiteralDictionarySourceType.Crossword => CrosswordDictionarySource,
            LiteralDictionarySourceType.English => EnglishDictionarySource,
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(sourceType),
                actualValue: sourceType,
                message: null),
        };
    }

    private static string FileTypeToString(LiteralDictionaryFileType fileType)
    {
        return Enum.GetName(
            enumType: typeof(LiteralDictionaryFileType),
            value: fileType) ?? throw new InvalidOperationException(message: "Unknown file type");
    }

    public static LiteralDictionaryFileType StringToFileType(string fileType)
    {
        return Enum.Parse<LiteralDictionaryFileType>(
            value: fileType,
            ignoreCase: true);
    }

    private static string SourceTypeToString(LiteralDictionarySourceType sourceType)
    {
        return Enum.GetName(
            enumType: typeof(LiteralDictionarySourceType),
            value: sourceType) ?? throw new InvalidOperationException(message: "Unknown source type");
    }

    public static LiteralDictionarySourceType StringToSourceType(string sourceType)
    {
        return Enum.Parse<LiteralDictionarySourceType>(
            value: sourceType,
            ignoreCase: true);
    }
}