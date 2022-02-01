using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Models;

public class GameStateMachine : IGameStateMachine
{
    private GameState _gameState;
    private bool newGame = true;

    public GameStateMachine()
    {
        this.DictionarySource = null;
        this.Game = null;
        this.LiteralDictionary = null;
        this.State = GameState.Rules;
        this.WordLength = Constants.StandardLength;
    }

    public bool HardMode { get; private set; }
    public WordMasterMindGame? Game { get; private set; }
    public LiteralDictionarySources? DictionarySource { get; }
    public LiteralDictionary? LiteralDictionary { get; }
    public int? WordLength { get; }

    public GameState State
    {
        get => this._gameState;
        set
        {
            var oldState = this._gameState;
            if (this.newGame)
            {
                this.newGame = false;
                oldState = GameState.Rules;
            }

            switch (oldState)
            {
                case GameState.Rules:
                    if (value is not GameState.SourceSelection && value is not GameState.Rules)
                        throw new Exception(message: "SourceSelection state can only be entered from Rules state");
                    break;
                case GameState.SourceSelection:
                    if (value != GameState.LengthSelection)
                        throw new Exception(
                            message: "LengthSelection state can only be entered from SourceSelection state");

                    if (this.DictionarySource is null)
                        throw new Exception(message: "Cannot enter LengthSelection state without a dictionary source");
                    break;
                case GameState.LengthSelection:
                    if (value != GameState.Playing)
                        throw new Exception(message: "Playing state can only be entered from LengthSelection state");

                    if (this.WordLength is null)
                        throw new Exception(message: "Cannot start playing without a word length");

                    this.Game = new WordMasterMindGame(
                        minLength: this.WordLength.Value,
                        maxLength: this.WordLength.Value,
                        hardMode: this.HardMode,
                        literalDictionary: this.LiteralDictionary,
                        secretWord: null);
                    break;
                case GameState.Playing:
                    if (value != GameState.GameOver)
                        throw new Exception(message: "GameOver state can only be entered from Playing state");

                    if (this.Game is null) throw new Exception(message: "Unexpected game state");

                    if (!this.Game.GameOver) throw new Exception(message: "Game is not over!");

                    break;
                case GameState.GameOver:
                    if (value != GameState.Rules)
                        throw new Exception(message: "Rules state can only be entered from GameOver state");

                    if (this.Game is null) throw new Exception(message: "Unexpected game state");

                    this.Game = null;

                    break;
                default:
                    throw new Exception(message: $"Unknown game state: {this._gameState}");
            }

            this._gameState = value;
            this.OnStateChanged(
                oldState: oldState,
                newState: value);
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