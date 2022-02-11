using System.Collections.Immutable;
using System.Net.Http.Json;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Exceptions;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Models;

public class GameStateMachine : IGameStateMachine
{
    private static readonly Dictionary<LiteralDictionarySource, IEnumerable<int>> ValidWordLengthsBySource = new();
    private readonly int? _wordLength;
    private bool _hardMode;
    private Action<GameStateMachine, StateChangedEventArgs>? OnStateChange;

    public GameStateMachine()
    {
        this.DictionarySourceType = LiteralDictionarySourceType.Scrabble;
        this.Game = null;
        this.LiteralDictionary = null;
        this.State = GameState.Rules;
        this.PreviousState = GameState.Rules;
        this._wordLength = Constants.StandardLength;
        this.OnStateChange = null;
        this.ValidWordLengths = ImmutableArray<int>.Empty;
    }

    public bool NightMode { get; set; }

    public bool HardMode
    {
        get => (this.Game is not null && this.Game.HardMode) || this._hardMode;
        set
        {
            if (this.State is GameState.Playing)
                throw new HardModeLockedException();
            if (this.Game is not null)
                this.Game.HardMode = value;
            this._hardMode = value;
        }
    }

    /// <summary>
    /// The engine doesn't have a sense of a partially submitted attempt.
    /// This keeps the state until an attempt is made
    /// </summary>
    public string CurrentAttemptString { get; set; } = string.Empty;

    public string CurrentAttemptLetter(int letterIndex)
        => (letterIndex >= CurrentAttemptString.Length || letterIndex < 0 ? Constants.EmptyChar : CurrentAttemptString[letterIndex])
            .ToString()
            .ToLowerInvariant();

    public WordMasterMindGame? Game { get; private set; }

    public HttpClient? HttpClient { get; set; }

    public LiteralDictionarySourceType DictionarySourceType { get; set; }

    public LiteralDictionary? LiteralDictionary { get; private set; }

    public int? WordLength { get; set; }

    public IEnumerable<int> ValidWordLengths { get; private set; }

    public async Task ChangeStateAsync(GameState newState)
    {
        var currentStateBeingLeft = this.State;
        if (newState == currentStateBeingLeft) return;

        switch (newState)
        {
            case GameState.Rules:
                break;
            case GameState.Settings:
                break;
            case GameState.SourceSelection:
                break;
            case GameState.LengthSelection:
                var lengthSource = LiteralDictionarySource.FromSourceType(sourceType: this.DictionarySourceType);
                this.ValidWordLengths = await this.GetValidLengthsForSource(lengthSource: lengthSource);
                break;
            case GameState.Playing:
                await this.ResumeOrRestartGame(leavingState: currentStateBeingLeft);
                // this sets its own state, do not exit the switch
                return;
            case GameState.GameOver:
                // should only transition to GameOver when the game object says so
                if (currentStateBeingLeft is not GameState.Playing)
                    throw new Exception(message: "GameOver state can only be entered from Playing state");

                if (this.Game is null) throw new Exception(message: "Unexpected game state");

                if (!this.Game.GameOver) throw new Exception(message: "Game is not over!");

                this.Game = null;
                break;
            default:
                throw new Exception(message: $"Unknown game state: {this.State}");
        }

        this.SetState(
            leavingState: currentStateBeingLeft,
            newState: newState);
    }

    public GameState PreviousState { get; private set; }
    public GameState State { get; private set; }

    private async Task ResumeOrRestartGame(GameState leavingState)
    {
        /* example scenario:
        //   User visits rules page the first time and continues to start a game
        // PreviousState = Rules (default)
        // leavingState = Rules (coming from Rules for the first time, PreviousState is invalid, but set to Rules)
        // newState = Playing

        // example scenario:
        //   User visits rules page while playing and returns to the game
        // PreviousState = Playing
        // leavingState = Rules
        // newState = Playing
        */

        if (this.PreviousState is GameState.Playing)
        {
            // simply resume the game and make it visible in the ui in the state blocks
            this.SetState(
                leavingState: leavingState,
                newState: GameState.Playing);
            return;
        }

        // if not coming back from playing, start a new game
        // if a game was previously started, it will be replaced
        // StartNewGame will set the state to Playing
        await this.StartNewGame(
            leavingState: leavingState);
    }

    private async Task StartNewGame(GameState leavingState)
    {
        if (this._wordLength is null)
            throw new Exception(message: "Cannot start playing without a word length");

        var playingSource = LiteralDictionarySource.FromSourceType(sourceType: this.DictionarySourceType);
        var sourceData = await (this.HttpClient ?? throw new InvalidOperationException())
            .GetByteArrayAsync(requestUri: $"/dictionaries/{this._wordLength}-{playingSource.FileName}");

        this.LiteralDictionary =
            LiteralDictionary.NewFromSource(
                source: playingSource,
                sourceData: sourceData);

        this.Game = new WordMasterMindGame(
            minLength: this._wordLength.Value,
            maxLength: this._wordLength.Value,
            hardMode: this._hardMode,
            literalDictionary: this.LiteralDictionary,
            secretWord: null);

        this.SetState(
            leavingState: leavingState,
            newState: GameState.Playing);
    }

    private void SetState(GameState leavingState, GameState newState)
    {
        this.PreviousState = leavingState;
        this.State = newState;
        this.OnStateChange?.Invoke(
            arg1: this,
            arg2: new StateChangedEventArgs(
                previousState: leavingState,
                newState: newState));
    }

    /// <summary>
    ///     Memoize the lengths of words that can be played in the given dictionary source
    /// </summary>
    /// <param name="lengthSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    private async Task<IEnumerable<int>> GetValidLengthsForSource(LiteralDictionarySource lengthSource)
    {
        if (ValidWordLengthsBySource.ContainsKey(key: lengthSource))
            return ValidWordLengthsBySource[key: lengthSource];

        // get valid word lengths
        var lengths = await (this.HttpClient ?? throw new InvalidOperationException())
            .GetFromJsonAsync<IEnumerable<int>>(
                requestUri: $"dictionaries/{lengthSource.FileName}-lengths.json");
        ValidWordLengthsBySource[key: lengthSource] = lengths ??
                                                      throw new Exception(
                                                          message:
                                                          $"Could not get valid word lengths for source {lengthSource}");
        return lengths;
    }
}