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

    public WordMasterMindGame(int minLength, int maxLength, bool hardMode = false,
        ScrabbleDictionary? scrabbleDictionary = null, string? secretWord = null)
    {
        Solved = false;
        CurrentAttempt = 0;
        HardMode = hardMode;
        ScrabbleDictionary = scrabbleDictionary ??
                                  new ScrabbleDictionary(); // use the provided dictionary, or use the default one which is stored locally
        _secretWord = (secretWord ?? ScrabbleDictionary.GetRandomWord(minLength: minLength,
            maxLength: maxLength)).ToUpperInvariant();

        Debug.Assert(condition: _secretWord != null,
            message: nameof(_secretWord) + " is null");

        WordLength = _secretWord.Length;

        if (WordLength > maxLength || WordLength < minLength)
            throw new InvalidLengthException(minLength: minLength,
                maxLength: maxLength);

        if (!ScrabbleDictionary.IsWord(word: _secretWord))
            throw new NotInDictionaryException();

        MaxAttempts = GetMaxAttemptsForLength(
            length: WordLength,
            hardMode: HardMode);
        _attempts = new IEnumerable<AttemptDetail>[MaxAttempts];
        _solvedLetters = new bool[WordLength];
    }

    public bool[] SolvedLetters
    {
        get
        {
            var copy = new bool[WordLength];
            Array.Copy(
                sourceArray: _solvedLetters,
                destinationArray: copy,
                length: WordLength);
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
    public IEnumerable<IEnumerable<AttemptDetail>> Attempts => _attempts.Take(count: CurrentAttempt);

    public bool Solved { get; private set; }

    public bool GameOver => Solved || CurrentAttempt >= MaxAttempts;

    public string SecretWord
    {
        get
        {
            if (!Solved && !IsDebug) throw new DebugModeException(paramName: nameof(SecretWord));
            return _secretWord;
        }
    }

    public string AttemptHistoryEmojiString
    {
        get
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < CurrentAttempt; i++)
            {
                stringBuilder.Append(
                    value: string.Concat(values: _attempts[i].Select(selector: a => a.ToString())));
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
        var emojiColor = Constants.BlackEmoji;
        if (attemptDetail.PositionCorrect) emojiColor = Constants.GreenEmoji;

        else if (attemptDetail.LetterCorrect) emojiColor = Constants.YellowEmoji;

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
        if (GameOver) throw new GameOverException(solved: Solved);

        if (WordLength != wordAttempt.Length)
            throw new InvalidAttemptLengthException();

        wordAttempt = wordAttempt.ToUpperInvariant();

        if (!ScrabbleDictionary.IsWord(word: wordAttempt))
            throw new NotInDictionaryException();

        // countAttemptLetterIndex is incremented each time the selector is fired, eg each letter
        var currentAttemptLetterIndex = 0;
        // the attempt hasn't been registered in the count yet
        _attempts[CurrentAttempt] = wordAttempt
            .Select(
                selector: c => new AttemptDetail(
                    letterPosition: currentAttemptLetterIndex,
                    letter: c,
                    letterCorrect: _secretWord.Contains(value: c),
                    positionCorrect: _secretWord[index: currentAttemptLetterIndex++] == c)).ToArray();

        // update solved letters array
        for (var i = 0; i < WordLength; i++)
            _solvedLetters[i] |= _attempts[CurrentAttempt].ElementAt(index: i).PositionCorrect;

        // the attempt hasn't been registered in the count yet, checking for being at least second turn
        if (HardMode && CurrentAttempt >= 1)
            // if a previous attempt had a letter in the correct position, future attempts must have the same letter in the correct position
            foreach (var attemptDetail in _attempts[CurrentAttempt])
                /* if the letter has been previously solved and the letter has
                 * been changed from the secret word, throw the HardModeException
                 */
                if (
                    _solvedLetters[attemptDetail.LetterPosition] &&
                    attemptDetail.Letter != _secretWord[index: attemptDetail.LetterPosition])
                    throw new HardModeException();

        // if we haven't thrown an exception due to hard mode, and the word is the secret word, we've solved it
        if (_secretWord.Equals(value: wordAttempt)) Solved = true;

        // return the current attempt's record and advance the counter
        return _attempts[CurrentAttempt++];
    }
}