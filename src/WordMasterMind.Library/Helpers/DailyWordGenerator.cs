using WordMasterMind.Library.Models;

namespace WordMasterMind.Library.Helpers;

/// <summary>
///     The idea is to seed a PRNG with a known value and then use that to generate a known sequence of words from the
///     dictionary
///     or possibly to otherwise create a hash function that turns a given calendar date into a specific word from the
///     dictionary
/// </summary>
public static class DailyWordGenerator
{
    /// <summary>
    ///     The Official WordMasterMind Word of the Day seed*
    ///     *) probably should be changed to a random value
    /// </summary>
    public const int Seed = 0xBEEF;

    /// <summary>
    ///     WordMasterMind's Birthday, Puzzle number 1 is this day
    /// </summary>
    public static readonly DateTime WordGeneratorEpoch = new(
        year: 2022,
        month: 1,
        day: 22);

    /// <summary>
    ///     Gets the puzzle number for a given date. Day 1 is the WordGeneratorEpoch.
    ///     Previous dates just go backwards for simplicity.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int PuzzleNumber(DateTime? date = null)
    {
        date = date ?? DateTime.Now;
        return Math.Abs(value: (int) Math.Floor(d: date.Value.Subtract(value: WordGeneratorEpoch).TotalDays)) + 1;
    }

    public static int WordIndexForDay(DateTime? date = null, int wordsForLength = Constants.StandardLength)
    {
        // an RNG with a pre-determined seed will generate a repeatable sequence
        var rnd = new Random(Seed: Seed);
        var wordIndex = 0;
        var puzzleNumber = PuzzleNumber(date: date);
        // skip puzzleNumber - 1 entries in the randomizer seed to get to that day's index
        // pick a number between 0 and the maximum
        for (var skips = 0; skips < puzzleNumber - 1; skips++)
            wordIndex = rnd.Next(
                minValue: 0,
                maxValue: wordsForLength - 1);
        return wordIndex;
    }

    public static string WordOfTheDay(DateTime? date = null,
        int length = Constants.StandardLength,
        LiteralDictionary? dictionary = null)
    {
        dictionary = dictionary ?? new LiteralDictionary();
        return dictionary.WordAtIndex(
            length: length,
            wordIndex: WordIndexForDay(
                date: date,
                wordsForLength: dictionary.WordCountForLength(
                    length: length)));
    }
}