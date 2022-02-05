using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial record LiteralDictionarySource
{
    public string FileTypeString => FileTypeToString(fileType: this.FileType);

    public string SourceTypeString => SourceTypeToString(sourceType: this.SourceType);

    private static string FileTypeToString(LiteralDictionaryFileType fileType)
    {
        return Enum.GetName(
            enumType: typeof(LiteralDictionaryFileType),
            value: fileType) ?? throw new InvalidOperationException("Unknown file type");
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