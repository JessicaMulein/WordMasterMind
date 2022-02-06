using System.Net.Http.Json;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Models;

public class GameStateMachine : IGameStateMachine
{
    private readonly int? _wordLength;
    private LiteralDictionarySourceType? _dictionarySourceType;
    private bool _hardMode;

    public GameStateMachine()
    {
        this._dictionarySourceType = null;
        this.Game = null;
        this.LiteralDictionary = null;
        this.State = GameState.Rules;
        this._wordLength = Constants.StandardLength;
    }

    public bool NightMode { get; set; }

    public bool HardMode
    {
        get => this._hardMode;
        set
        {
            if (this.State is GameState.Playing)
                throw new InvalidOperationException(message: "Cannot change hard mode while playing");
            this._hardMode = value;
        }
    }

    public WordMasterMindGame? Game { get; private set; }

    public HttpClient? HttpClient { get; set; }

    public LiteralDictionarySourceType? DictionarySourceType
    {
        get => this._dictionarySourceType;
        set => this._dictionarySourceType = value;
    }

    public LiteralDictionary? LiteralDictionary { get; private set; }

    public int? WordLength { get; set; }

    public IEnumerable<int> ValidWordLengths { get; private set; }

    public async Task SetStateAsync(GameState newState)
    {
        // leaving oldState
        var oldState = this.State;
        // ReSharper disable once InlineTemporaryVariable
        if (newState == oldState) return;

        switch (newState)
        {
            case GameState.Rules:
                break;
            case GameState.SourceSelection:
                if (oldState is not GameState.Rules && oldState is not GameState.LengthSelection)
                    throw new Exception(
                        message: "SourceSelection state can only be entered from Rules or LengthSelection states");

                break;
            case GameState.LengthSelection:
                if (oldState is not GameState.SourceSelection)
                    throw new Exception(
                        message: "LengthSelection state can only be entered from SourceSelection state");

                if (this._dictionarySourceType is null)
                    throw new Exception(message: "Cannot enter LengthSelection state without a dictionary source");

                var lengthSource = LiteralDictionarySource.FromSourceType(sourceType: this._dictionarySourceType.Value);

                // get valid word lengths
                var lengths = await (this.HttpClient ?? throw new InvalidOperationException()).GetFromJsonAsync<IEnumerable<int>>(
                    requestUri: $"dictionaries/{lengthSource.FileName}-lengths.json");
                this.ValidWordLengths = lengths ?? throw new InvalidOperationException();
                break;
            case GameState.Playing:
                if (oldState is not GameState.LengthSelection)
                    throw new Exception(message: "Playing state can only be entered from LengthSelection state");

                if (this._wordLength is null)
                    throw new Exception(message: "Cannot start playing without a word length");

                if (this._dictionarySourceType is null)
                    throw new Exception(message: "Cannot start playing without a dictionary source");

                var playingSource = LiteralDictionarySource.FromSourceType(sourceType: this._dictionarySourceType.Value);
                var sourceData =  await (this.HttpClient ?? throw new InvalidOperationException())
                    .GetByteArrayAsync(requestUri: $"/dictionaries/{this._wordLength}-{playingSource.FileName}");

                this.LiteralDictionary =
                    LiteralDictionary.NewFromSource(
                        source: playingSource,
                        sourceData: sourceData);

                this.Game = new WordMasterMindGame(
                    minLength: this._wordLength.Value,
                    maxLength: this._wordLength.Value,
                    hardMode: this.HardMode,
                    literalDictionary: this.LiteralDictionary,
                    secretWord: null);
                break;
            case GameState.GameOver:
                if (oldState is not GameState.Playing)
                    throw new Exception(message: "GameOver state can only be entered from Playing state");

                if (this.Game is null) throw new Exception(message: "Unexpected game state");

                if (!this.Game.GameOver) throw new Exception(message: "Game is not over!");

                this.Game = null;
                break;
            case GameState.Settings:
                break;
            default:
                throw new Exception(message: $"Unknown game state: {this.State}");
        }

        this.State = newState;
        this.OnStateChanged(
            oldState: oldState,
            newState: newState);
    }

    public GameState State { get; private set; }

    public event Action OnStateChange;

    private void NotifyStateChanged()
    {
        this.OnStateChange?.Invoke();
    }

    public void OnStateChanged(GameState oldState, GameState newState)
    {
        this.NotifyStateChanged();
    }
}