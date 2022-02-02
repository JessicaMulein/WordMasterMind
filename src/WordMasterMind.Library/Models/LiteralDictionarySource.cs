using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

/// <summary>
///     Single source of dictionaries for the game.
/// </summary>
public partial record LiteralDictionarySource
{
    public readonly string Description;
    public readonly string FileName;
    public readonly LiteralDictionaryFileType FileType;
    public readonly LiteralDictionarySourceType SourceTypeType;

    private LiteralDictionarySource(LiteralDictionarySourceType sourceTypeType, string fileName,
        LiteralDictionaryFileType fileType, string description)
    {
        this.SourceTypeType = sourceTypeType;
        this.FileName = fileName;
        this.FileType = fileType;
        this.Description = description;
    }

    public static IEnumerable<LiteralDictionarySource> Sources => new[]
    {
        ScrabbleDictionarySource,
        CrosswordDictionarySource,
        EnglishDictionarySource,
    };
}