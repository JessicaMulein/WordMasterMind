namespace WordMasterMind.Models;

public static class WordMasterMindPlayer
{
    private const string StandardFirstAttempt = "aeiousnthrdlcmzyxwvfjqg";

    private static void UpdateAttemptMemory(ref char[] currentWordStatus, ref List<char> mustIncludeLetters, in IEnumerable<AttemptDetail> attemptDetails)
    {
        var position = 0;
        foreach (var attemptDetail in attemptDetails)
        {
            if (attemptDetail.PositionCorrect) currentWordStatus[position] = attemptDetail.Letter;
            if (attemptDetail.LetterCorrect)
            {
                if (!mustIncludeLetters.Contains(attemptDetail.Letter))
                {
                    mustIncludeLetters.Add(attemptDetail.Letter);
                }
            }
            position++;
        }
    }

    private static void AttemptAndUpdateMemory(in WordMasterMind mastermind, ref char[] currentWordStatus, ref List<char> mustIncludeLetters, string wordAttempt)
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

        // ReSharper disable once StringLiteralTypo
        if (mastermind.WordLength > StandardFirstAttempt.Length)
        {
            throw new Exception("We were not prepared for this. The scrabble dictionary goes to 16 letters.");
        }

        var mustIncludeLetters = new List<char>();
        var triedWords = new List<string>();
        // try the first try
        AttemptAndUpdateMemory(
            mastermind: mastermind,
            currentWordStatus: ref currentWordStatus,
            mustIncludeLetters: ref mustIncludeLetters,
            wordAttempt: StandardFirstAttempt[..mastermind.WordLength]);

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
                wordAttempt: StandardFirstAttempt[..mastermind.WordLength]);
        }

        return mastermind.Solved;
    }
}