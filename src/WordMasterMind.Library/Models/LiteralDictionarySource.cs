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
    public readonly LiteralDictionarySources SourceType;

    private LiteralDictionarySource(LiteralDictionarySources sourceType, string fileName,
        LiteralDictionaryFileType fileType, string description)
    {
        this.SourceType = sourceType;
        this.FileName = fileName;
        this.FileType = fileType;
        this.Description = description;
    }

    public static IEnumerable<LiteralDictionarySource> Sources => new[]
    {
        LiteralDictionary,
        CrosswordDictionary,
        EnglishDictionary
    };
}