using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class GameStateMachine
{
    public GameStateMachine()
    {
        this.Game = null;
        this.State = GameState.Rules;
        this.WordLength = Constants.StandardLength;
        this.DictionarySource = null;
        this.LiteralDictionary = null;
    }

    public WordMasterMindGame? Game { get; private set; }
    public LiteralDictionarySources? DictionarySource { get; private set; }
    public int? WordLength { get; private set; }
    public LiteralDictionary? LiteralDictionary { get; private set; }
    public GameState State { get; private set; }
}