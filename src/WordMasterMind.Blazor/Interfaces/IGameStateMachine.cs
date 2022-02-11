using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Interfaces;

public interface IGameStateMachine
{
    /// <summary>
    ///     Most recent state
    /// </summary>
    public GameState PreviousState { get; }

    /// <summary>
    ///     Current state
    /// </summary>
    public GameState State { get; }

    /// <summary>
    ///     Whether to use the daily word generator or a random word
    /// </summary>
    public bool DailyWord { get; set; }

    /// <summary>
    ///     Whether night mode is turned on in the UI
    /// </summary>
    public bool NightMode { get; set; }

    /// <summary>
    ///     Whether hard mode is enabled
    /// </summary>
    public bool HardMode { get; set; }

    /// <summary>
    ///     Current mastermind instance- only available in playing state.
    /// </summary>
    public WordMasterMindGame? Game { get; }

    /// <summary>
    ///     The engine doesn't have a sense of a partially submitted attempt.
    ///     This keeps the state until an attempt is made
    /// </summary>
    public string CurrentAttemptString { get; set; }

    /// <summary>
    ///     The currently selected dictionary source. Defaults to Collins Scrabble.
    /// </summary>
    public LiteralDictionarySourceType DictionarySourceType { get; set; }

    /// <summary>
    ///     Currently selected word length
    /// </summary>
    public int? WordLength { get; set; }

    /// <summary>
    ///     Provides an http client with the appropriate host/port for SPA
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    ///     Gets the letter at the (0-indexed) specified position.
    /// </summary>
    /// <param name="letterIndex"></param>
    /// <returns></returns>
    public string CurrentAttemptLetter(int letterIndex);

    /// <summary>
    ///     Dictionary object for the currently selected dictionary source
    /// </summary>
    public Task<LiteralDictionary> GetLiteralDictionary();

    /// <summary>
    ///     Gets valid word lengths for the currently selected dictionary source
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<int>> GetDictionaryWordLengths();

    /// <summary>
    ///     Attempts to change to the requested state and fires events when successful
    /// </summary>
    /// <param name="newState"></param>
    /// <returns></returns>
    public Task ChangeStateAsync(GameState newState);
}