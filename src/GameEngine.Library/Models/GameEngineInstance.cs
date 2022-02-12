using GameEngine.Library.Enumerations;
using GameEngine.Library.Exceptions;
using GameEngine.Library.Helpers;

namespace GameEngine.Library.Models;

public partial class GameEngineInstance
{
    private readonly List<char> _foundLetters;

    /// <summary>
    ///     Current word being guessed. Randomly selected from the Scrabble dictionary.
    /// </summary>
    private readonly string _secretWord;

    private readonly bool[] _solvedLetters;

    /// <summary>
    ///     Scrabble Dictionary to use
    /// </summary>
    public readonly LiteralDictionary LiteralDictionary;

    /// <summary>
    ///     Source being used to generate the secret word.
    /// </summary>
    public readonly LiteralDictionarySourceType LiteralDictionarySourceType;

    private bool _hardMode;

    /// <summary>
    /// </summary>
    /// <param name="minLength">The minimum word length the computer will use from the dictionary</param>
    /// <param name="maxLength">The maximum word length the computer will use from the dictionary. May equal the minimum.</param>
    /// <param name="hardMode">
    ///     When enabled, found letters must be in subsequent guesses, and solved positions must remain
    ///     solved.
    /// </param>
    /// <param name="literalDictionary">Provide a proper LiteralDictionary object or null</param>
    /// <param name="secretWord">Optionally force the secret word to be this value</param>
    /// <param name="basePath">Must be provided if literalDictionary is not specified</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidLengthException"></exception>
    /// <exception cref="NotInDictionaryException"></exception>
    public GameEngineInstance(int minLength, int maxLength, bool hardMode = false,
        LiteralDictionary? literalDictionary = null, string? secretWord = Constants.ComputerSelectedWord,
        string? basePath = null, bool dailyWordWhenComputer = true)
    {
        this.Solved = false;
        this.CurrentAttempt = 0;
        this._hardMode = hardMode;
        // use the provided dictionary, or use the default one which is stored locally
        this.LiteralDictionary = literalDictionary ??
                                 LiteralDictionary.NewFromSourceType(
                                     sourceType: LiteralDictionarySourceType.Scrabble,
                                     basePath: basePath ??
                                               throw new ArgumentNullException(paramName: nameof(basePath)));
        if (secretWord is not null &&
            (secretWord.Length < minLength ||
             secretWord.Length > maxLength) ||
            minLength <= 0)
            throw new InvalidLengthException(minLength: minLength,
                maxLength: maxLength);

        this._secretWord = (secretWord switch
        {
            null when dailyWordWhenComputer => DailyWordGenerator.WordOfTheDay(
                    date: null,
                    length: this.LiteralDictionary.RandomLength(
                        minLength: minLength,
                        maxLength: maxLength),
                    dictionary: this.LiteralDictionary,
                    basePath: basePath),
            null => this.LiteralDictionary
                .GetRandomWord(
                    minLength: minLength,
                    maxLength: maxLength),
            _ => secretWord,
        }).ToUpperInvariant();

        this.WordLength = this._secretWord.Length;

        if (this.WordLength > maxLength || this.WordLength < minLength)
            throw new InvalidLengthException(minLength: minLength,
                maxLength: maxLength);

        if (!this.LiteralDictionary.IsWord(word: this._secretWord))
            throw new NotInDictionaryException();

        this.MaxAttempts = GetMaxAttemptsForLength(
            length: this.WordLength);
        this._attempts = new AttemptDetails[this.MaxAttempts];
        this._solvedLetters = new bool[this.WordLength];
        this._foundLetters = new List<char>();
    }

    /// <summary>
    ///     if a previous attempt had a letter in the correct position, future attempts must have the same letter in the
    ///     correct position
    /// </summary>
    public bool HardMode
    {
        get => this._hardMode;
        set
        {
            if (this.CurrentAttempt > 0)
                throw new InvalidOperationException(message: "Cannot change hard mode after the first attempt");

            this._hardMode = value;
        }
    }

    public IEnumerable<bool> SolvedLetters => DuplicateArray(array: this._solvedLetters);

    public IEnumerable<char> FoundLetters => this._foundLetters.ToArray();

    /// <summary>
    ///     Debug flag allows revealing the secret word
    /// </summary>
    private static bool IsDebug => UnitTestDetector.IsRunningInUnitTest;

    public bool Solved { get; private set; }

    public bool GameOver => this.Solved || this.CurrentAttempt >= this.MaxAttempts;

    public string SecretWord
    {
        get
        {
            if (!this.Solved && !IsDebug) return string.Empty;
            return this._secretWord;
        }
    }

    private static IEnumerable<bool> DuplicateArray(in bool[] array)
    {
        var copy = new bool[array.Length];
        Array.Copy(
            sourceArray: array,
            destinationArray: copy,
            length: array.Length);
        return copy;
    }

    public char[] SolvedLettersAsChars(char filler = Constants.EmptyChar)
    {
        var solvedLetters = new char[this.WordLength];
        for (var i = 0; i < this.WordLength; i++)
            solvedLetters[i] = this._solvedLetters[i] ? this._secretWord[index: i] : filler;

        return solvedLetters;
    }
}