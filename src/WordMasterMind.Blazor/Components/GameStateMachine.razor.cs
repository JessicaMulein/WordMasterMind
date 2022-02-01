using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class GameStateMachine
{
    private GameState _gameState;

    public GameStateMachine()
    {
        this.Game = null;
        this.State = GameState.Rules;
        this.WordLength = Constants.StandardLength;
        this.DictionarySource = null;
        this.LiteralDictionary = null;
    }

    public Library.Models.WordMasterMindGame? Game { get; private set; }
    public LiteralDictionarySources? DictionarySource { get; private set; }
    public int? WordLength { get; private set; }
    public LiteralDictionary? LiteralDictionary { get; private set; }

    public GameState State
    {
        get => this._gameState;
        private set
        {
            var oldState = this._gameState;
            switch (oldState)
            {
                case GameState.Rules:
                    if (value != GameState.SourceSelection)
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
                    break;
                case GameState.Playing:
                    if (value != GameState.GameOver)
                        throw new Exception(message: "GameOver state can only be entered from Playing state");

                    if (!this.Game.GameOver) throw new Exception(message: "Game is not over!");

                    if (this.Game is null) throw new Exception(message: "Unexpected game state");
                    break;
                case GameState.GameOver:
                    if (value != GameState.Rules)
                        throw new Exception(message: "Rules state can only be entered from GameOver state");

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

    public void OnStateChanged(GameState oldState, GameState newState)
    {
    }
}