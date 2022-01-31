using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class DictionarySourceList
{
    public string Name { get; set; }
    public static IEnumerable<LiteralDictionarySource> Sources => LiteralDictionarySource.Sources;

    public DictionarySourceList(string name = "sourcesList")
    {
        Name = name;
    }
}