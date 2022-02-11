using System.Text;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Library.Models;

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

    public static string? BasePath { get; set; } = null;

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

    public static int SourceTypeToSeed(string description)
    {
        var hash = Crc32.ComputeChecksumBytes(
            bytes: Encoding.ASCII.GetBytes(
                s: description.ToLowerInvariant()));
        var seed = Seed;
        for (var offset = 0; offset < hash.Length; offset += 4)
            seed ^= BitConverter.ToInt32(value: hash,
                startIndex: offset);
        return seed;
    }

    /// <summary>
    ///     TODO: AAHED is the 3rd word in the scrabble dictionary, and the first 5 letter. Given the seed system, this is
    ///     unlikely to be correct
    ///     TODO: investigate and improve puzzle sequence generator
    /// </summary>
    /// <param name="dictionaryDescription"></param>
    /// <param name="wordLength"></param>
    /// <param name="advance"></param>
    /// <returns></returns>
    public static Random RandomSequenceGenerator(string dictionaryDescription,
        int wordLength,
        int advance = 0)
    {
        var rnd = new Random(Seed: SourceTypeToSeed(
            description: dictionaryDescription));
        // skip wordLength entries
        for (var i = 0; i < wordLength; i++) rnd.Next();
        // then make a new random sequence starting from the current seed
        var rnd2 = new Random(Seed: rnd.Next());
        // advance the random sequence by advance entries
        for (var skips = 0; skips < advance; skips++)
            rnd2.Next();
        return rnd2;
    }

    public static int WordIndexForDay(string dictionaryDescription,
        int wordLength, int wordsForLength,
        DateTime? date = null)
    {
        // an RNG with a pre-determined seed will generate a repeatable sequence
        var rnd = RandomSequenceGenerator(
            dictionaryDescription: dictionaryDescription,
            wordLength: wordLength);

        var wordIndex = 0;
        var puzzleNumber = PuzzleNumber(date: date);
        // skip puzzleNumber - 1 entries in the randomizer seed to get to that day's index
        // pick a number between 0 and the maximum
        for (var skips = 0; skips < puzzleNumber - 1; skips++)
            wordIndex = rnd.Next(
                minValue: 0,
                maxValue: wordsForLength);
        return wordIndex;
    }

    public static string WordOfTheDay(
        DateTime? date = null,
        int length = Constants.StandardLength,
        LiteralDictionary? dictionary = null, string? basePath = null)
    {
        dictionary = dictionary ??
                     LiteralDictionary.NewFromSourceType(
                         sourceType: LiteralDictionarySourceType.Scrabble,
                         basePath: basePath ?? throw new InvalidOperationException());
        return dictionary.WordAtIndex(
            length: length,
            wordIndex: WordIndexForDay(
                dictionaryDescription: dictionary.Description,
                wordLength: length,
                wordsForLength: dictionary.WordCountForLength(
                    length: length),
                date: date));
    }

    public static int PuzzleNumberForWordOfTheDay(string word, LiteralDictionary dictionary)
    {
        var rnd = RandomSequenceGenerator(
            dictionaryDescription: dictionary.Description,
            wordLength: word.Length,
            advance: 0);
        var wordIndex = dictionary.IndexForWord(word: word);
        // return number of skips when we match the word
        var wordCount = dictionary.WordCountForLength(length: word.Length);
        for (var puzzleNumber = 0; puzzleNumber < wordCount; puzzleNumber++)
            if (wordIndex == rnd.Next(
                    minValue: 0,
                    maxValue: wordCount))
                return puzzleNumber + 1;
        return -1;
    }
}