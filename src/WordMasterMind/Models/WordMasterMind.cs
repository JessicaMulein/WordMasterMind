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

    private readonly bool[] _solvedLetters;

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
        this._secretWord = (secretWord ?? this.ScrabbleDictionary.GetRandomWord(minLength: minLength,
            maxLength: maxLength)).ToUpperInvariant();

        Debug.Assert(condition: this._secretWord != null,
            message: nameof(this._secretWord) + " is null");

        this.WordLength = this._secretWord.Length;

        if (this.WordLength > maxLength || this.WordLength < minLength)
            throw new InvalidLengthException(minLength: minLength,
                maxLength: maxLength);

        if (!this.ScrabbleDictionary.IsWord(word: this._secretWord))
            throw new NotInDictionaryException();

        this.MaxAttempts = GetMaxAttemptsForLength(
            length: this.WordLength,
            hardMode: this.HardMode);
        this._attempts = new IEnumerable<AttemptDetail>[this.MaxAttempts];
        this._solvedLetters = new bool[this.WordLength];
    }

    public bool[] SolvedLetters
    {
        get
        {
            var copy = new bool[this.WordLength];
            Array.Copy(
                sourceArray: this._solvedLetters,
                destinationArray: copy,
                length: this.WordLength);
            return copy;
        }
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
            {
                stringBuilder.Append(
                    value: string.Concat(values: this._attempts[i].Select(selector: a => a.ToString())));
                stringBuilder.Append(value: '\n');
            }

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

    public static string AttemptToEmojiString(IEnumerable<AttemptDetail> attemptDetails)
    {
        var stringBuilder = new StringBuilder();
        foreach (var attemptDetail in attemptDetails)
            stringBuilder.Append(value: GetEmojiFromAttemptDetail(attemptDetail: attemptDetail));

        stringBuilder.Append(value: '\n');

        return stringBuilder.ToString();
    }

    public static int GetMaxAttemptsForLength(int length, bool hardMode)
    {
        return length + (hardMode ? 2 : 1);
    }

    public IEnumerable<AttemptDetail> Attempt(string wordAttempt)
    {
        if (this.Solved || this.CurrentAttempt >= this.MaxAttempts) throw new GameOverException(solved: this.Solved);

        if (this.WordLength != wordAttempt.Length)
            throw new InvalidAttemptLengthException();

        wordAttempt = wordAttempt.ToUpperInvariant();

        if (!this.ScrabbleDictionary.IsWord(word: wordAttempt))
            throw new NotInDictionaryException();

        // countAttemptLetterIndex is incremented each time the selector is fired, eg each letter
        var currentAttemptLetterIndex = 0;
        // the attempt hasn't been registered in the count yet
        this._attempts[this.CurrentAttempt] = wordAttempt
            .Select(
                selector: c => new AttemptDetail(
                    letterPosition: currentAttemptLetterIndex,
                    letter: c,
                    letterCorrect: this._secretWord.Contains(value: c),
                    positionCorrect: this._secretWord[index: currentAttemptLetterIndex++] == c)).ToArray();

        // update solved letters array
        for (var i = 0; i < this.WordLength; i++)
            this._solvedLetters[i] |= this._attempts[this.CurrentAttempt].ElementAt(index: i).PositionCorrect;

        // the attempt hasn't been registered in the count yet, checking for being at least second turn
        if (this.HardMode && this.CurrentAttempt >= 1)
            // if a previous attempt had a letter in the correct position, future attempts must have the same letter in the correct position
            foreach (var attemptDetail in this._attempts[this.CurrentAttempt])
                /* if the letter has been previously solved and the letter has
                 * been changed from the secret word, throw the HardModeException
                 */
                if (
                    this._solvedLetters[attemptDetail.LetterPosition] &&
                    attemptDetail.Letter != this._secretWord[index: attemptDetail.LetterPosition])
                    throw new HardModeException();

        // if we haven't thrown an exception due to hard mode, and the word is the secret word, we've solved it
        if (this._secretWord.Equals(value: wordAttempt)) this.Solved = true;

        // return the current attempt's record and advance the counter
        return this._attempts[this.CurrentAttempt++];
    }
}