using GameEngine.Library.Enumerations;
using GameEngine.Library.Exceptions;

namespace GameEngine.Library.Models;

public static class GameEnginePlayer
{
    public const int DefaultTries = 1000;

    // ReSharper disable once MemberCanBePrivate.Global
    public static readonly IEnumerable<string> FiveLetterStrategies = new[]
    {
        "ADIEU",
        "ALOFT",
        "AROSE",
        "ARISE",
        "CRANE",
        "IDEAL",
        "LINTY",
        "PAUSE",
        "RAISE",
        // ReSharper disable once StringLiteralTypo
        "SOARE",
        "SOUTH",
        "WILDS",
    };

    // ReSharper disable once MemberCanBePrivate.Global
    public static string RandomFiveLetterStrategy
    {
        get
        {
            var rnd = new Random();
            var strategies = FiveLetterStrategies.ToArray();
            return strategies[rnd.Next(maxValue: strategies.Length)];
        }
    }


    /// <summary>
    ///     The computer strategy starts with adieu if the game is 5 letters and it is not specifically disabled
    /// </summary>
    /// <param name="mastermind"></param>
    /// <param name="turn"></param>
    /// <param name="maximumDictionaryLookupAttemptsPerTry"></param>
    /// <param name="excludeWords"></param>
    /// <param name="mustIncludeLetters"></param>
    /// <param name="noStrategy"></param>
    /// <param name="avoidSecretWord"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string ComputerGuessWord(GameEngineInstance mastermind,
        int turn,
        int maximumDictionaryLookupAttemptsPerTry = 1000,
        IEnumerable<string>? excludeWords = null,
        IEnumerable<char>? mustIncludeLetters = null,
        bool noStrategy = false, bool avoidSecretWord = false)
    {
        if (turn == 1 && mastermind.WordLength == 5 && !noStrategy)
            return RandomFiveLetterStrategy;

        if (avoidSecretWord)
        {
            var enumerable = (excludeWords ?? Array.Empty<string>()).ToList();
            if (!enumerable.Contains(value: mastermind.SecretWord))
            {
                enumerable.Add(item: mastermind.SecretWord);
                excludeWords = enumerable;
            }
        }

        return mastermind.LiteralDictionary.FindWords(
                regex: mastermind.SolvedLettersAsRegex(),
                wordLength: mastermind.WordLength,
                maxResults: 1,
                excludeWords: excludeWords,
                mustIncludeLetters: mustIncludeLetters)
            .First();
    }

    /// <summary>
    ///     Attempts to solve the current puzzle from whatever turn it is on.
    /// </summary>
    /// <param name="mastermind"></param>
    /// <param name="turns"></param>
    /// <param name="maximumDictionaryLookupAttemptsPerTry"></param>
    /// <param name="noStrategy"></param>
    /// <param name="avoidSecretWord">
    ///     Avoid the secret word (to do too many attempts). The computer will NEVER guess the
    ///     correct word.
    /// </param>
    /// <returns></returns>
    /// <exception cref="GameOverException"></exception>
    public static bool AttemptComputerSolve(GameEngineInstance mastermind, int turns = -1,
        int maximumDictionaryLookupAttemptsPerTry = DefaultTries, bool noStrategy = false, bool avoidSecretWord = false)
    {
        if (mastermind.Solved || mastermind.CurrentAttempt >= mastermind.MaxAttempts)
            throw new GameOverException(solved: mastermind.Solved);

        var mustIncludeLetters = new List<char>();
        var triedWords = new List<string>();
        var turn = 1;
        while (!mastermind.Solved && (turns == -1 || turns-- > 0))
        {
            var computerGuess = ComputerGuessWord(
                turn: turn,
                mastermind: mastermind,
                maximumDictionaryLookupAttemptsPerTry: maximumDictionaryLookupAttemptsPerTry,
                excludeWords: triedWords,
                mustIncludeLetters: mustIncludeLetters,
                noStrategy: noStrategy);

            if (triedWords.Contains(item: computerGuess) ||
                avoidSecretWord && computerGuess.Equals(value: mastermind.SecretWord)) continue;

            // advance the turn counter
            turn++;

            triedWords.Add(item: computerGuess);

            var attempt = mastermind.MakeAttempt(wordAttempt: computerGuess);

            // add all matched letters to the mustIncludeLetters list
            // future word guesses must include these letters
            attempt.Details
                .Where(predicate: d => d.Evaluation is LetterEvaluation.Present or LetterEvaluation.Correct)
                .ToList()
                .ForEach(action: d => { mustIncludeLetters.Add(item: d.Letter); });
        }

        return mastermind.Solved;
    }
}