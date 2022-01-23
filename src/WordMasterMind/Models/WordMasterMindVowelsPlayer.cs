namespace WordMasterMind.Models;

public static class WordMasterMindPlayer
{
    private static void UpdateAttemptMemory(ref char[] currentWordStatus, ref List<char> mustIncludeLetters,
        in IEnumerable<AttemptDetail> attemptDetails)
    {
        var position = 0;
        foreach (var attemptDetail in attemptDetails)
        {
            if (attemptDetail.PositionCorrect) currentWordStatus[position] = attemptDetail.Letter;
            if (attemptDetail.LetterCorrect)
                if (!mustIncludeLetters.Contains(item: attemptDetail.Letter))
                    mustIncludeLetters.Add(item: attemptDetail.Letter);
            position++;
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

    public static bool ComputerGuess(WordMasterMind mastermind, int maximumDictionaryLookupAttemptsPerTry = 1000)
    {
        if (mastermind.Solved) throw new Exception(message: "You have already solved this word!");

        if (mastermind.CurrentAttempt >= mastermind.MaxAttempts)
            throw new Exception(message: "You have reached the maximum number of attempts");

        var currentWordStatus = new char[mastermind.WordLength];
        var mustIncludeLetters = new List<char>();
        var triedWords = new List<string>();
        while (!mastermind.Solved)
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