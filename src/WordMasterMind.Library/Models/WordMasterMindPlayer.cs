using WordMasterMind.Library.Exceptions;

namespace WordMasterMind.Library.Models;

public static class WordMasterMindPlayer
{
    public static readonly IEnumerable<string> FiveLetterStrategies = new[]
    {
        "ADIEU",
        "AROSE",
        "ARISE",
        "IDEAL",
        "LINTY",
        "RAISE",
        "SOARE",
        "SOUTH",
    };

    public static string RandomFiveLetterStrategy
    {
        get
        {
            var rnd = new Random();
            var strategies = FiveLetterStrategies.ToArray();
            return strategies[rnd.Next(strategies.Length - 1)];
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
    /// <returns></returns>
    public static string ComputerGuessWord(WordMasterMindGame mastermind,
        int turn,
        int maximumDictionaryLookupAttemptsPerTry = 1000,
        IEnumerable<string>? excludeWords = null,
        IEnumerable<char>? mustIncludeLetters = null,
        bool noStrategy = false)
    {
        if (turn == 1 && mastermind.WordLength == 5 && !noStrategy)
            return RandomFiveLetterStrategy;

        return mastermind.LiteralDictionary.FindWord(
            knownCharacters: mastermind.SolvedLettersAsChars,
            maxIterations: maximumDictionaryLookupAttemptsPerTry,
            skipWords: excludeWords,
            mustIncludeLetters: mustIncludeLetters);
    }

    /// <summary>
    ///     Attempts to solve the current puzzle from whatever turn it is on.
    /// </summary>
    /// <param name="mastermind"></param>
    /// <param name="turns"></param>
    /// <param name="maximumDictionaryLookupAttemptsPerTry"></param>
    /// <param name="noStrategy"></param>
    /// <returns></returns>
    /// <exception cref="GameOverException"></exception>
    public static bool AttemptComputerSolve(WordMasterMindGame mastermind, int turns = -1,
        int maximumDictionaryLookupAttemptsPerTry = 1000, bool noStrategy = false)
    {
        if (mastermind.Solved || mastermind.CurrentAttempt >= mastermind.MaxAttempts)
            throw new GameOverException(solved: mastermind.Solved);

        var mustIncludeLetters = new List<char>();
        var triedWords = new List<string>();
        var turn = 1;
        while (!mastermind.Solved && (turns == -1 || turns-- > 0))
        {
            var computerGuess = ComputerGuessWord(
                turn: turn++,
                mastermind: mastermind,
                maximumDictionaryLookupAttemptsPerTry: maximumDictionaryLookupAttemptsPerTry,
                excludeWords: triedWords,
                mustIncludeLetters: mustIncludeLetters,
                noStrategy: noStrategy);

            triedWords.Add(item: computerGuess);

            var attempt = mastermind.Attempt(wordAttempt: computerGuess);

            // add all matched letters to the mustIncludeLetters list
            // future word guesses must include these letters
            attempt.Details
                .Where(predicate: d => d.LetterCorrect)
                .ToList()
                .ForEach(action: d =>
                {
                    mustIncludeLetters.Add(item: d.Letter);
                });
        }

        return mastermind.Solved;
    }
}