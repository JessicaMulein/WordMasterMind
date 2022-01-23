using WordMasterMind.Helpers;

namespace WordMasterMind.Models;

public class WordMasterMind
{
    /// <summary>
    ///     Collection of attempts
    /// </summary>
    private readonly IEnumerable<(char letter, bool letterCorrect, bool positionCorrect)>[] _attempts;

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

    public WordMasterMind(int minLength, int maxLength, int maxAttempts, bool hardMode = false,
        ScrabbleDictionary? scrabbleDictionary = null, string? secretWord = null)
    {
        Solved = false;
        CurrentAttempt = 0;
        HardMode = hardMode;
        MaxAttempts = maxAttempts;
        scrabbleDictionary ??=
            new ScrabbleDictionary(); // use the provided dictionary, or use the default one which is stored locally
        _attempts = new IEnumerable<(char letter, bool letterCorrect, bool positionCorrect)>[maxAttempts];
        _secretWord = secretWord ?? scrabbleDictionary.GetRandomWord(minLength: minLength, maxLength: maxLength);

        if (_secretWord.Length > maxLength || _secretWord.Length < minLength)
            throw new ArgumentException(message: "Secret word must be between minLength and maxLength");

        if (!scrabbleDictionary.IsWord(word: _secretWord))
            throw new ArgumentException(message: "Secret word must be a valid word in the Scrabble dictionary");
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
    public IEnumerable<IEnumerable<(char letter, bool letterCorrect, bool positionCorrect)>> Attempts =>
        _attempts.Take(count: CurrentAttempt);

    public bool Solved { get; private set; }

    public string SecretWord
    {
        get
        {
            if (!IsDebug) throw new Exception(message: "Secret word is only available in debug mode");
            return _secretWord;
        }
    }

    public IEnumerable<(char letter, bool letterCorrect, bool positionCorrect)> Attempt(string wordAttempt)
    {
        if (Solved) throw new Exception(message: "You have already solved this word!");

        if (CurrentAttempt >= MaxAttempts)
            throw new Exception(message: "You have reached the maximum number of attempts");

        if (_secretWord.Length != wordAttempt.Length)
            throw new ArgumentException(message: "Word length does not match secret word length");

        var currentAttempt = 0;
        var attempt = wordAttempt
            .ToUpperInvariant()
            .Select(
                selector: c => (
                    letter: c,
                    letterCorrect: _secretWord.Contains(value: c),
                    positionCorrect: _secretWord[index: currentAttempt++] == c)).ToArray();

        if (HardMode && CurrentAttempt > 1)
        {
            // if a previous attempt had a letter in the correct position, future attempts must have the same letter in the correct position
            var lockedLetters = new bool[_secretWord.Length];
            for (var i = 0; i < CurrentAttempt; i++)
            {
                var letterIndex = 0;
                foreach (var (letter, letterCorrect, positionCorrect) in _attempts[i])
                    if (letterCorrect && positionCorrect)
                        lockedLetters[letterIndex++] = true;
            }

            // now check the current attempt for locked letters
            for (var i = 0; i < wordAttempt.Length; i++)
                if (lockedLetters[i] && attempt[i].letter != _secretWord[index: i])
                    throw new Exception(message: "You cannot change a letter that is in the correct position");
        }

        if (wordAttempt == _secretWord) Solved = true;

        _attempts[CurrentAttempt++] = attempt;
        return attempt;
    }
}