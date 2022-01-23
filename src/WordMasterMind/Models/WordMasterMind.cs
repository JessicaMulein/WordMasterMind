using System.Diagnostics;
using System.Net;
using System.Text;
using WordMasterMind.Exceptions;
using WordMasterMind.Helpers;

namespace WordMasterMind.Models;

public class WordMasterMind
{
    private const string GreenEmoji = "&#129001;";
    private const string YellowEmoji = "&#129000;";
    private const string BlackEmoji = "&#11035;";

    /// <summary>
    ///     Collection of attempts
    /// </summary>
    private readonly IEnumerable<AttemptDetail>[] _attempts;

    /// <summary>
    ///     Current word being guessed. Randomly selected from the Scrabble dictionary.
    /// </summary>
    private readonly string _secretWord;

    /// <summary>
    ///     if a previous attempt had a letter in the correct position, future attempts must have the same letter in the
    ///     correct position
    /// </summary>
    public readonly bool HardMode;

    /// <summary>
    ///     How many attempts are allowed before the game is over
    /// </summary>
    public readonly int MaxAttempts;

    /// <summary>
    ///     Scrabble Dictionary to use
    /// </summary>
    public readonly ScrabbleDictionary ScrabbleDictionary;

    /// <summary>
    ///     Length of the word to be guessed
    /// </summary>
    public readonly int WordLength;

    public WordMasterMind(int minLength, int maxLength, bool hardMode = false,
        ScrabbleDictionary? scrabbleDictionary = null, string? secretWord = null)
    {
        this.Solved = false;
        this.CurrentAttempt = 0;
        this.HardMode = hardMode;
        this.ScrabbleDictionary = scrabbleDictionary ??
                                  new ScrabbleDictionary(); // use the provided dictionary, or use the default one which is stored locally
        this._secretWord = secretWord ?? this.ScrabbleDictionary.GetRandomWord(minLength: minLength,
            maxLength: maxLength);

        Debug.Assert(condition: this._secretWord != null,
            message: nameof(this._secretWord) + " is null");
        this.WordLength = this._secretWord.Length;
        if (this.WordLength > maxLength || this.WordLength < minLength)
            throw new ArgumentException(message: "Secret word must be between minLength and maxLength");

        if (!this.ScrabbleDictionary.IsWord(word: this._secretWord))
            throw new ArgumentException(message: "Secret word must be a valid word in the Scrabble dictionary");

        this.MaxAttempts = GetMaxAttemptsForLength(length: this.WordLength);
        this._attempts = new IEnumerable<AttemptDetail>[this.MaxAttempts];
    }

    /// <summary>
    ///     Debug flag allows revealing the secret word
    /// </summary>
    private static bool IsDebug => UnitTestDetector.IsRunningInUnitTest;

    /// <summary>
    ///     Gets the current attempt number
    /// </summary>
    public int CurrentAttempt { get; private set; }

    /// <summary>
    ///     Gets the attempts so far
    /// </summary>
    public IEnumerable<IEnumerable<AttemptDetail>> Attempts => this._attempts.Take(count: this.CurrentAttempt);

    public bool Solved { get; private set; }

    public string SecretWord
    {
        get
        {
            if (!IsDebug) throw new DebugModeException(paramName: nameof(this.SecretWord));
            return this._secretWord;
        }
    }

    public string AttemptHistoryEmojiString
    {
        get
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < this.CurrentAttempt; i++)
                stringBuilder.Append(
                    value: string.Concat(values: this._attempts[i].Select(selector: a => a.ToString())));

            return stringBuilder.ToString();
        }
    }

    private static string GetEmojiFromConst(in string constValue)
    {
        return WebUtility.HtmlDecode(value: constValue);
    }

    public static string GetEmojiFromAttemptDetail(in AttemptDetail attemptDetail)
    {
        var emojiColor = BlackEmoji;
        if (attemptDetail.PositionCorrect) emojiColor = GreenEmoji;

        else if (attemptDetail.LetterCorrect) emojiColor = YellowEmoji;

        return GetEmojiFromConst(constValue: emojiColor);
    }

    public static int GetMaxAttemptsForLength(int length, bool hardMode = false)
    {
        return length + 1 + (hardMode ? 1 : 0);
    }

    public IEnumerable<AttemptDetail> Attempt(string wordAttempt)
    {
        if (this.Solved || this.CurrentAttempt >= this.MaxAttempts) throw new GameOverException(solved: this.Solved);

        if (this.WordLength != wordAttempt.Length)
            throw new InvalidWordLengthException();

        if (!this.ScrabbleDictionary.IsWord(word: wordAttempt))
            throw new NotInDictionaryException();

        var currentAttempt = 0;
        var attempt = wordAttempt
            .ToUpperInvariant()
            .Select(
                selector: c => new AttemptDetail(
                    letter: c,
                    letterCorrect: this._secretWord.Contains(value: c),
                    positionCorrect: this._secretWord[index: currentAttempt++] == c)).ToArray();

        if (this.HardMode && this.CurrentAttempt > 1)
        {
            // if a previous attempt had a letter in the correct position, future attempts must have the same letter in the correct position
            var lockedLetters = new bool[this.WordLength];
            for (var i = 0; i < this.CurrentAttempt; i++)
            {
                var letterIndex = 0;
                foreach (var attemptDetail in this._attempts[i])
                    if (attemptDetail.LetterCorrect && attemptDetail.PositionCorrect)
                        lockedLetters[letterIndex++] = true;
            }

            // now check the current attempt for locked letters
            for (var i = 0; i < wordAttempt.Length; i++)
                if (lockedLetters[i] && attempt[i].Letter != this._secretWord[index: i])
                    throw new Exception(message: "You cannot change a letter that is in the correct position");
        }

        if (wordAttempt == this._secretWord) this.Solved = true;

        this._attempts[this.CurrentAttempt++] = attempt;
        return attempt;
    }

    public static string AttemptToEmojiString(IEnumerable<AttemptDetail> attemptDetails)
    {
        var stringBuilder = new StringBuilder();
        foreach (var attemptDetail in attemptDetails)
            stringBuilder.Append(value: GetEmojiFromAttemptDetail(attemptDetail: attemptDetail));

        stringBuilder.Append(value: '\n');

        return stringBuilder.ToString();
    }
}