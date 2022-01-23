using WordMasterMind.Exceptions;

namespace WordMasterMind.Models;

public static class WordMasterMindPlayer
{
    private static void UpdateAttemptMemory(ref char[] currentWordStatus, ref List<char> mustIncludeLetters,
        in IEnumerable<AttemptDetail> attemptDetails)
    {
        foreach (var attemptDetail in attemptDetails)
        {
            if (!attemptDetail.LetterCorrect) continue;
            if (attemptDetail.PositionCorrect) currentWordStatus[attemptDetail.LetterPosition] = attemptDetail.Letter;
            if (!mustIncludeLetters.Contains(item: attemptDetail.Letter))
                mustIncludeLetters.Add(item: attemptDetail.Letter);
        }
    }

    private static void AttemptAndUpdateMemory(in WordMasterMind mastermind, ref char[] currentWordStatus,
        ref List<char> mustIncludeLetters, string wordAttempt)
    {
        var attempt = mastermind.Attempt(wordAttempt: wordAttempt).ToArray();
        UpdateAttemptMemory(
            currentWordStatus: ref currentWordStatus,
            mustIncludeLetters: ref mustIncludeLetters,
            attemptDetails: attempt);
    }

    public static bool ComputerGuess(WordMasterMind mastermind, int turns = -1,
        int maximumDictionaryLookupAttemptsPerTry = 1000)
    {
        if (mastermind.Solved || mastermind.CurrentAttempt >= mastermind.MaxAttempts)
            throw new GameOverException(solved: mastermind.Solved);

        var currentWordStatus = new char[mastermind.WordLength];
        var mustIncludeLetters = new List<char>();
        var triedWords = new List<string>();
        while (!mastermind.Solved && (turns == -1 || turns-- > 0))
        {
            var computerGuess = mastermind.ScrabbleDictionary.FindWord(
                knownCharacters: currentWordStatus,
                maxIterations: maximumDictionaryLookupAttemptsPerTry,
                skipWords: triedWords);
            triedWords.Add(item: computerGuess);
            AttemptAndUpdateMemory(
                mastermind: mastermind,
                currentWordStatus: ref currentWordStatus,
                mustIncludeLetters: ref mustIncludeLetters,
                wordAttempt: computerGuess);
        }

        return mastermind.Solved;
    }
}