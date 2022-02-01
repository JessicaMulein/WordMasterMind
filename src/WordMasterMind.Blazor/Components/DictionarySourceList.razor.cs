using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class DictionarySourceList
{
    public DictionarySourceList(IGameStateMachine gameStateMachineMachine, string name = "sourcesList")
    {
        this.Name = name;
    }

    public string Name { get; set; }
    public static IEnumerable<LiteralDictionarySource> Sources => LiteralDictionarySource.Sources;
}