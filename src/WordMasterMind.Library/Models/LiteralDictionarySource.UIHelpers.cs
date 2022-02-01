using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial record LiteralDictionarySource
{
    public string FileTypeString => FileTypeToString(fileType: this.FileType);

    public string SourceTypeString => SourceTypeToString(fileType: this.FileType);

    private static string FileTypeToString(LiteralDictionaryFileType fileType)
    {
        return fileType switch
        {
            LiteralDictionaryFileType.TextWithNewLines => nameof(LiteralDictionaryFileType.TextWithNewLines),
            LiteralDictionaryFileType.JsonStringArray => nameof(LiteralDictionaryFileType.JsonStringArray),
            LiteralDictionaryFileType.Binary => nameof(LiteralDictionaryFileType.Binary),
            _ => throw new Exception(message: "Unknown file type")
        };
    }

    public static LiteralDictionaryFileType StringToFileType(string fileType)
    {
        return Enum.Parse<LiteralDictionaryFileType>(
            value: fileType,
            ignoreCase: true);
    }

    private static string SourceTypeToString(LiteralDictionaryFileType fileType)
    {
        return fileType switch
        {
            LiteralDictionaryFileType.TextWithNewLines => nameof(LiteralDictionaryFileType.TextWithNewLines),
            LiteralDictionaryFileType.JsonStringArray => nameof(LiteralDictionaryFileType.JsonStringArray),
            LiteralDictionaryFileType.Binary => nameof(LiteralDictionaryFileType.Binary),
            _ => throw new Exception(message: "Unknown file type")
        };
    }

    public static LiteralDictionaryFileType StringToSourceType(string fileType)
    {
        return Enum.Parse<LiteralDictionaryFileType>(
            value: fileType,
            ignoreCase: true);
    }
}