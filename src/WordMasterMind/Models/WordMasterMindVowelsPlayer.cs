namespace WordMasterMind.Models;

public static class WordMasterMindPlayer
{
    public static bool ComputerGuess(WordMasterMind mastermind, int maximumDictionaryLookupAttemptsPerTry = 1000)
    {
        if (mastermind.Solved) throw new Exception(message: "You have already solved this word!");

        if (mastermind.CurrentAttempt >= mastermind.MaxAttempts)
            throw new Exception(message: "You have reached the maximum number of attempts");

        var currentWordStatus = new char[mastermind.WordLength];

        // ReSharper disable once StringLiteralTypo
        var firstTry = "aeiousnthrdlcmzyxwvfjqg";
        if (mastermind.WordLength > firstTry.Length)
        {
            throw new Exception("We were not prepared for this. The scrabble dictionary goes to 16 letters.");
        }
        var triedWords = new List<string>();
        // try the first try
        var attempt = mastermind.Attempt(wordAttempt: firstTry.Substring(startIndex: 0,
            length: mastermind.WordLength));
        var position = 0;
        foreach (var attemptDetail in attempt)
        {
            if (attemptDetail.LetterCorrect) currentWordStatus[position] = attemptDetail.Letter;
            position++;
        }

        while (!mastermind.Solved)
        {
            var computerGuess = mastermind.ScrabbleDictionary.FindWord(
                knownCharacters: currentWordStatus,
                maxIterations: maximumDictionaryLookupAttemptsPerTry,
                skipWords: triedWords);
            triedWords.Add(item: computerGuess);
            attempt = mastermind.Attempt(wordAttempt: computerGuess);

            position = 0;
            foreach (var attemptDetail in attempt)
            {
                if (attemptDetail.LetterCorrect) currentWordStatus[position] = attemptDetail.Letter;
                position++;
            }
        }

        return mastermind.Solved;
    }
}