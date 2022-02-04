using System.Diagnostics;
using System.Net;
using System.Text;
using WordMasterMind.Library.Exceptions;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Library.Models;

public class WordMasterMindGame
{
    /// <summary>
    ///     Collection of attempts
    /// </summary>
    private readonly AttemptDetails[] _attempts;

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
    ///     Scrabble Dictionary to use
    /// </summary>
    public readonly LiteralDictionary LiteralDictionary;

    /// <summary>
    ///     How many attempts are allowed before the game is over
    /// </summary>
    public readonly int MaxAttempts;

    /// <summary>
    ///     Length of the word to be guessed
    /// </summary>
    public readonly int WordLength;

    public WordMasterMindGame(int minLength, int maxLength, bool hardMode = false,
        LiteralDictionary? literalDictionary = null, string? secretWord = null)
    {
        this.Solved = false;
        this.CurrentAttempt = 0;
        this.HardMode = hardMode;
        this.LiteralDictionary = literalDictionary ??
                                 new LiteralDictionary(); // use the provided dictionary, or use the default one which is stored locally
        this._secretWord = (secretWord ?? this.LiteralDictionary.GetRandomWord(minLength: minLength,
            maxLength: maxLength)).ToUpperInvariant();

        Debug.Assert(condition: this._secretWord != null,
            message: nameof(this._secretWord) + " is null");

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
    public IEnumerable<AttemptDetails> Attempts => this._attempts[..this.CurrentAttempt];

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

    public string AttemptHistoryEmojiString
    {
        get
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < this.CurrentAttempt; i++)
            {
                stringBuilder.Append(
                    value: string.Concat(values: this._attempts[i].Details.Select(selector: a => a.ToString())));
                stringBuilder.Append(value: '\n');
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    ///     Game score. Higher is better.
    /// </summary>
    public int Score
    {
        get
        {
            // two points per attempt under maximum
            var score = 2 * (this.MaxAttempts - this.CurrentAttempt);
            // three points if the first time a letter is used, it is in the correct position
            // one point for each new letter out of place
            // one more point for a previously guessed letter when it is in the correct position
            var newLetters = new List<char>();
            foreach (var turn in this._attempts)
            foreach (var attemptDetail in turn.Details)
            {
                var newLetter = !newLetters.Contains(item: attemptDetail.Letter);
                if (attemptDetail.LetterCorrect && newLetter)
                {
                    score += 3;
                    newLetters.Add(item: attemptDetail.Letter);
                }
                else if (attemptDetail.LetterPresent && newLetter)
                {
                    score += 1;
                    newLetters.Add(item: attemptDetail.Letter);
                }
                else if (attemptDetail.LetterCorrect && !newLetter)
                {
                    score += 1;
                }
            }

            return score;
        }
    }

    public char[] SolvedLettersAsChars
    {
        get
        {
            var solvedLetters = new char[this.WordLength];
            for (var i = 0; i < this.WordLength; i++)
                solvedLetters[i] = this._solvedLetters[i] ? this._secretWord[index: i] : ' ';

            return solvedLetters;
        }
    }

    private static string GetEmojiFromConst(in string constValue)
    {
        return WebUtility.HtmlDecode(value: constValue);
    }

    public static string GetEmojiFromAttemptDetail(in AttemptDetail attemptDetail)
    {
        var emojiColor = Constants.BlackEmoji;
        if (attemptDetail.LetterCorrect) emojiColor = Constants.GreenEmoji;

        else if (attemptDetail.LetterPresent) emojiColor = Constants.YellowEmoji;

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

    public static int GetMaxAttemptsForLength(int length)
    {
        return length + 1;
    }

    public AttemptDetails Attempt(string wordAttempt)
    {
        if (this.GameOver) throw new GameOverException(solved: this.Solved);

        if (this.WordLength != wordAttempt.Length)
            throw new InvalidAttemptLengthException();

        wordAttempt = wordAttempt.ToUpperInvariant();

        if (!this.LiteralDictionary.IsWord(word: wordAttempt))
            throw new NotInDictionaryException();

        // countAttemptLetterIndex is incremented each time the selector is fired, eg each letter
        var currentAttemptLetterIndex = 0;
        // the attempt hasn't been registered in the count yet
        this._attempts[this.CurrentAttempt] = new AttemptDetails(
            attemptNumber: this.CurrentAttempt + 1,
            details: wordAttempt
                .Select(
                    selector: c => new AttemptDetail(
                        letterPosition: currentAttemptLetterIndex,
                        letter: c,
                        letterPresent: this._secretWord.Contains(value: c),
                        letterCorrect: this._secretWord[index: currentAttemptLetterIndex++] == c)).ToArray());

        // update solved letters array
        for (var i = 0; i < this.WordLength; i++)
            this._solvedLetters[i] |= this._attempts[this.CurrentAttempt].Details.ElementAt(index: i).LetterCorrect;

        // the attempt hasn't been registered in the count yet, checking for being at least second turn
        if (this.HardMode && this.CurrentAttempt >= 1)
            // if a previous attempt had a letter in the correct position, future attempts must have the same letter in the correct position
            foreach (var attemptDetail in this._attempts[this.CurrentAttempt])
                /* if the letter has been previously solved and the letter has
                 * been changed from the secret word, throw the HardModeException
                 */
                if (this._solvedLetters[attemptDetail.LetterPosition] &&
                    attemptDetail.Letter != this._secretWord[index: attemptDetail.LetterPosition])
                    throw new HardModeException();

        // if we haven't thrown an exception due to hard mode, and the word is the secret word, we've solved it
        if (this._secretWord.Equals(value: wordAttempt)) this.Solved = true;

        // return the current attempt's record and advance the counter
        return this._attempts[this.CurrentAttempt++];
    }
}