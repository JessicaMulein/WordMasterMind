using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial record LiteralDictionarySource
{
    public string FileTypeString => FileTypeToString(fileType: this.FileType);

    public string SourceTypeString => SourceTypeToString(sourceType: this.SourceType);

    private static string FileTypeToString(LiteralDictionaryFileType fileType)
    {
        return fileType switch
        {
            LiteralDictionaryFileType.TextWithNewLines => nameof(LiteralDictionaryFileType.TextWithNewLines),
            LiteralDictionaryFileType.JsonStringArray => nameof(LiteralDictionaryFileType.JsonStringArray),
            LiteralDictionaryFileType.Binary => nameof(LiteralDictionaryFileType.Binary),
            _ => throw new Exception(message: "Unknown file type"),
        };
    }

    public static LiteralDictionaryFileType StringToFileType(string fileType)
    {
        return Enum.Parse<LiteralDictionaryFileType>(
            value: fileType,
            ignoreCase: true);
    }

    private static string SourceTypeToString(LiteralDictionarySourceType sourceType)
    {
        return sourceType switch
        {
            LiteralDictionarySourceType.Scrabble => nameof(LiteralDictionarySourceType.Scrabble),
            LiteralDictionarySourceType.Crossword => nameof(LiteralDictionarySourceType.Crossword),
            LiteralDictionarySourceType.English => nameof(LiteralDictionarySourceType.English),
            LiteralDictionarySourceType.Other => nameof(LiteralDictionarySourceType.Other),
            _ => throw new Exception(message: "Unknown source type"),
        };
    }

    public static LiteralDictionarySourceType StringToSourceType(string sourceType)
    {
        return Enum.Parse<LiteralDictionarySourceType>(
            value: sourceType,
            ignoreCase: true);
    }
}