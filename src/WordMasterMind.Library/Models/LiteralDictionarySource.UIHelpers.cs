namespace WordMasterMind.Library.Models;

public partial record LiteralDictionarySource
{
    public string FileTypeString => FileTypeToString(fileType: this.FileType);

    public string SourceTypeString => SourceTypeToString(sourceType: this.SourceType);
}