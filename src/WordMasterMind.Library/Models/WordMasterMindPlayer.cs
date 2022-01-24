using WordMasterMind.Library.Exceptions;

namespace WordMasterMind.Library.Models;

public static class WordMasterMindPlayer
{
    /// <summary>
    ///     The computer strategy starts with adieu if the game is 5 letters and it is not specifically disabled
    /// </summary>
    /// <param name="mastermind"></param>
    /// <param name="turn"></param>
    /// <param name="maximumDictionaryLookupAttemptsPerTry"></param>
    /// <param name="excludeWords"></param>
    /// <param name="mustIncludeLetters"></param>
    /// <param name="noAdieu"></param>
    /// <returns></returns>
    public static string ComputerGuessWord(WordMasterMindGame mastermind,
        int turn,
        int maximumDictionaryLookupAttemptsPerTry = 1000,
        IEnumerable<string>? excludeWords = null,
        IEnumerable<char>? mustIncludeLetters = null,
        bool noAdieu = false)
    {
        if (turn == 1 && mastermind.WordLength == 5 && !noAdieu)
            return "adieu".ToUpperInvariant();

        return mastermind.WordDictionaryDictionary.FindWord(
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
    /// <param name="noAdieu"></param>
    /// <returns></returns>
    /// <exception cref="GameOverException"></exception>
    public static bool AttemptComputerSolve(WordMasterMindGame mastermind, int turns = -1,
        int maximumDictionaryLookupAttemptsPerTry = 1000, bool noAdieu = false)
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
                noAdieu: noAdieu);

            triedWords.Add(item: computerGuess);

            var attempt = mastermind.Attempt(wordAttempt: computerGuess);

            // add all matched letters to the mustIncludeLetters list
            // future word guesses must include these letters
            attempt.Details
                .Where(predicate: d => d.LetterCorrect)
                .ToList().ForEach(action: d => { mustIncludeLetters.Add(item: d.Letter); });
        }

        return mastermind.Solved;
    }
}