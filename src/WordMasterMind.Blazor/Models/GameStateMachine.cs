using Microsoft.AspNetCore.Components;
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
    private GameState _gameState;
    private bool _hardMode;

    public GameStateMachine()
    {
        this._dictionarySourceType = null;
        this.Game = null;
        this.LiteralDictionary = null;
        this._gameState = GameState.Rules;
        this._wordLength = Constants.StandardLength;
    }

    [Inject] private HttpClient HttpClient { get; set; }

    public bool NightMode { get; set; }

    public bool HardMode
    {
        get => this._hardMode;
        set
        {
            if (this._gameState is GameState.Playing)
                throw new InvalidOperationException(message: "Cannot change hard mode while playing");
            this._hardMode = value;
        }
    }

    public WordMasterMindGame? Game { get; private set; }

    public LiteralDictionarySourceType? DictionarySourceType
    {
        get => this._dictionarySourceType;
        set => this._dictionarySourceType = value;
    }

    public LiteralDictionary? LiteralDictionary { get; private set; }

    public int? WordLength { get; set; }

    public GameState State
    {
        get => this._gameState;
        set
        {
            // leaving oldState
            var oldState = this._gameState;
            // ReSharper disable once InlineTemporaryVariable
            var newState = value;
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

                    this.LiteralDictionary =
                        LiteralDictionary.NewFromSourceType(
                            sourceType: this._dictionarySourceType.Value,
                            basePath: null,
                            httpClient: this.HttpClient);
                    break;
                case GameState.Playing:
                    if (oldState is not GameState.LengthSelection)
                        throw new Exception(message: "Playing state can only be entered from LengthSelection state");

                    if (this._wordLength is null)
                        throw new Exception(message: "Cannot start playing without a word length");

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
                    throw new Exception(message: $"Unknown game state: {this._gameState}");
            }

            this._gameState = newState;
            this.OnStateChanged(
                oldState: oldState,
                newState: newState);
        }
    }

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