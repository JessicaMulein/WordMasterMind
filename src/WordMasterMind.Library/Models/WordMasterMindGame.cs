using System.Diagnostics;
using System.Net;
using System.Text;
using WordMasterMind.Library.Enumerations;
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
                // join all of the emojis for each attempt and append
                stringBuilder.AppendLine(
                    value: string.Concat(
                        values: this._attempts[i].Details
                            .Select(selector: a => a.ToString())));

            return stringBuilder.ToString();
        }
    }

    public string PuzzleHeader
    {
        get
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(value: $"Word MasterMind W{this.WordLength}:");
            var puzzleNumber = DailyWordGenerator.PuzzleNumberForWordOfTheDay(word: this.SecretWord,
                dictionary: this.LiteralDictionary);
            if (puzzleNumber >= 0)
            {
                stringBuilder.Append(value: $"P{puzzleNumber}");
            }
            else
            {
                var wordIndex = this.LiteralDictionary.IndexForWord(word: this.SecretWord);
                stringBuilder.Append(value: $"I{wordIndex}");
            }

            stringBuilder.Append(value: $" {this.Attempts.Count()}/{this.MaxAttempts}");
            return stringBuilder.ToString();
        }
    }

    public string AttemptHistoryString => string.Concat(
        values: new[]
        {
            this.PuzzleHeader,
            Environment.NewLine,
            this.AttemptHistoryEmojiString,
            Environment.NewLine,
        });

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
                switch (attemptDetail.Evaluation)
                {
                    case LetterEvaluation.Correct:
                        score += newLetter ? 3 : 1;
                        newLetters.Add(item: attemptDetail.Letter);
                        break;
                    case LetterEvaluation.Present:
                        score += newLetter ? 1 : 0;
                        newLetters.Add(item: attemptDetail.Letter);
                        break;
                    case LetterEvaluation.Absent:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return score;
        }
    }

    public char[] SolvedLettersAsChars(char filler = ' ')
    {
        var solvedLetters = new char[this.WordLength];
        for (var i = 0; i < this.WordLength; i++)
            solvedLetters[i] = this._solvedLetters[i] ? this._secretWord[index: i] : filler;

        return solvedLetters;
    }

    private static string GetEmojiFromConst(in string constValue)
    {
        return WebUtility.HtmlDecode(value: constValue);
    }

    public static string GetEmojiFromAttemptDetail(in AttemptLetterDetail attemptLetterDetail)
    {
        var emojiColor = attemptLetterDetail.Evaluation switch
        {
            LetterEvaluation.Correct => Constants.GreenEmoji,
            LetterEvaluation.Present => Constants.YellowEmoji,
            _ => Constants.BlackEmoji,
        };

        return GetEmojiFromConst(constValue: emojiColor);
    }

    public static string AttemptToEmojiString(IEnumerable<AttemptLetterDetail> attemptDetails)
    {
        var stringBuilder = new StringBuilder();
        foreach (var attemptDetail in attemptDetails)
            stringBuilder.Append(value: GetEmojiFromAttemptDetail(attemptLetterDetail: attemptDetail));

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
                    selector: c => new AttemptLetterDetail(
                        letterPosition: currentAttemptLetterIndex,
                        letter: c,
                        evaluation: this._secretWord[index: currentAttemptLetterIndex++] == c
                            ? LetterEvaluation.Correct
                            : this._secretWord.Contains(value: c)
                                ? LetterEvaluation.Present
                                : LetterEvaluation.Absent)).ToArray());

        // update solved letters array
        for (var i = 0; i < this.WordLength; i++)
            this._solvedLetters[i] |=
                this._attempts[this.CurrentAttempt].Details.ElementAt(index: i).Evaluation is LetterEvaluation.Correct;

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