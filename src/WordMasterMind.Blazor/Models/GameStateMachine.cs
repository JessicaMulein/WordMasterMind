using System.Net.Http.Json;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Exceptions;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Models;

/// <summary>
///     Singleton (service dependency injection enforced) that keeps the current UI state and a running instance of the
///     game in the appropriate state(s).
/// </summary>
public class GameStateMachine : IGameStateMachine
{
    private static readonly Dictionary<LiteralDictionarySource, IEnumerable<int>> ValidWordLengthsBySource = new();
    private readonly int? _wordLength;

    /// <summary>
    ///     Whether hard mode is enabled (or will be enabled when the game is started).
    ///     Reflects live state when there is a game playing.
    /// </summary>
    private bool _hardMode;

    /// <summary>
    ///     OnStateChange callback event
    /// </summary>
    private readonly Action<GameStateMachine, StateChangedEventArgs>? OnStateChange;

    public GameStateMachine()
    {
        this.DictionarySourceType = LiteralDictionarySourceType.Scrabble;
        this.Game = null;
        this.State = GameState.Rules;
        this.PreviousState = GameState.Rules;
        this._wordLength = Constants.StandardLength;
        this.OnStateChange = null;
    }

    /// <summary>
    ///     Whether hard mode is enabled (or will be enabled when the game is started).
    ///     Reflects live state when there is a game playing.
    /// </summary>
    public bool HardMode
    {
        get => this.Game is not null && this.Game.HardMode || this._hardMode;
        set
        {
            // try to update game state.
            // if playing- this will fail

            if (this.State is GameState.Playing)
                throw new HardModeLockedException();

            if (this.Game is not null)
            {
                try
                {
                    this.Game.HardMode = value;
                }
                catch (HardModeLockedException _)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    Console.WriteLine(value: $"Unexpected exception ({e.GetType()}): {e.Message}");
                }

                return;
            }

            // update offline/between-game copy
            this._hardMode = value;
        }
    }

    /// <summary>
    ///     The engine doesn't have a sense of a partially submitted attempt.
    ///     This keeps the state until an attempt is made
    /// </summary>
    public string CurrentAttemptString { get; set; } = string.Empty;

    public string CurrentAttemptLetter(int letterIndex)
    {
        return (letterIndex >= this.CurrentAttemptString.Length || letterIndex < 0
                ? Constants.EmptyChar
                : this.CurrentAttemptString[index: letterIndex])
            .ToString()
            .ToLowerInvariant();
    }

    public WordMasterMindGame? Game { get; private set; }

    /// <summary>
    ///     Whether to use the daily word generator or a random word
    /// </summary>
    public bool DailyWord { get; set; } = true;

    /// <summary>
    ///     Provides an http client with the appropriate host/port for SPA
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    ///     Whether night mode is turned on in the UI
    /// </summary>
    public bool NightMode { get; set; } = true;

    /// <summary>
    ///     The currently selected dictionary source. Defaults to Collins Scrabble.
    /// </summary>
    public LiteralDictionarySourceType DictionarySourceType { get; set; }

    /// <summary>
    ///     Dictionary object for the currently selected dictionary source
    /// </summary>
    public async Task<LiteralDictionary> GetLiteralDictionary()
    {
        return await this.LiteralDictionaryFromSourceViaHttp(sourceType: this.DictionarySourceType);
    }

    /// <summary>
    ///     Currently selected word length
    /// </summary>
    public int? WordLength { get; set; }

    /// <summary>
    ///     Attempts to change to the requested state and fires events when successful
    /// </summary>
    /// <param name="newState"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Most recent state
    /// </summary>
    public GameState PreviousState { get; private set; }

    /// <summary>
    ///     Current state
    /// </summary>
    public GameState State { get; private set; }

    /// <summary>
    ///     Gets valid word lengths for the currently selected dictionary source
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<int>> GetDictionaryWordLengths()
    {
        return await this.GetValidLengthsForSource(
            lengthSource: LiteralDictionarySource.FromSourceType(
                sourceType: this.DictionarySourceType));
    }

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

    public async Task<LiteralDictionary> LiteralDictionaryFromSourceViaHttp(LiteralDictionarySourceType sourceType)
    {
        var playingSource = LiteralDictionarySource.FromSourceType(sourceType: sourceType);
        var sourceData = await (this.HttpClient ?? throw new InvalidOperationException())
            .GetByteArrayAsync(requestUri: $"/dictionaries/{this._wordLength}-{playingSource.FileName}");

        return
            LiteralDictionary.NewFromSource(
                source: playingSource,
                sourceData: sourceData);
    }

    /// <summary>
    /// </summary>
    /// <param name="leavingState"></param>
    /// <param name="secretWord">Null = ComputerSelectedWord, otherwise provide word</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task StartNewGame(GameState leavingState, string? secretWord = Constants.ComputerSelectedWord)
    {
        if (this._wordLength is null)
            throw new Exception(message: "Cannot start playing without a word length");

        var dictionary = await this.GetLiteralDictionary();
        this.Game = new WordMasterMindGame(
            minLength: this._wordLength.Value,
            maxLength: this._wordLength.Value,
            hardMode: this._hardMode,
            literalDictionary: dictionary,
            secretWord: secretWord);

        this.CurrentAttemptString = string.Empty;

        this.SetState(
            leavingState: leavingState,
            newState: GameState.Playing);
    }

    /// <summary>
    ///     Internally sets the state and fires the state changed event. Do not use directly.
    /// </summary>
    /// <param name="leavingState"></param>
    /// <param name="newState"></param>
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