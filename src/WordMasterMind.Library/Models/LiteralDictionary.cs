using System.Collections.Immutable;
using System.Diagnostics;

namespace WordMasterMind.Library.Models;

/// <summary>
///     This is a Word Dictionary (word list/book) - dictionary (data structure) of words grouped by length.
///     It is designed for small scale usage. It is not (yet) optimized for a multi-user host.
/// </summary>
public partial class LiteralDictionary
{
    /// <summary>
    ///     Quick look up table with counts of words for each length.
    /// </summary>
    private readonly Dictionary<int, int> _wordCountByLength;

    /// <summary>
    ///     Unalterable collection of dictionary words, organized by length.
    /// </summary>
    private readonly ImmutableDictionary<int, IEnumerable<string>> _wordsByLength;

    /// <summary>
    ///     Notes/Description of the dictionary.
    /// </summary>
    public readonly string Description;

    /// <summary>
    ///     Longest word length with at least one word.
    /// </summary>
    public readonly int LongestWordLength;

    /// <summary>
    ///     Shortest word length with at least one word
    /// </summary>
    public readonly int ShortestWordLength;

    /// <summary>
    ///     Word lengths known to have at least one word
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly IEnumerable<int> ValidWordLengths;

    /// <summary>
    ///     Total count of words in the dictionary
    /// </summary>
    public readonly int WordCount;

    /// <summary>
    ///     Standard constructor takes a dictionary of strings, organized by length
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="description"></param>
    /// <exception cref="Exception"></exception>
    public LiteralDictionary(Dictionary<int, IEnumerable<string>>? dictionary = null, string? description = null)
    {
        ImmutableDictionary<int, IEnumerable<string>> wordsByLength;
        if (dictionary is not null)
        {
            // set this as the instance dictionary, made immutable
            wordsByLength = AlphabetizeDictionary(dictionary: dictionary).ToImmutableDictionary();
        }
        else
        {
            // this is a Blazor WebAssembly app, essentially self-hosted.
            // LoadDirectlyFromJsonFile() is a helper method that grabs it using HTTP to localhost
            var result = Task.Run(function: async () => await LoadDictionaryFromWebJsonWordArray());
            // wait for the task to complete
            result.Wait();
            // set up the instance dictionary with the result, made immutable
            wordsByLength = AlphabetizeDictionary(dictionary: result.Result).ToImmutableDictionary();
        }

        if (wordsByLength.Count == 0) throw new Exception(message: "Dictionary could not be loaded");
        this._wordsByLength = wordsByLength;

        // now that the dictionary is loaded, tally up the word counts and analyze the dictionary

        var wordCountByLength = new Dictionary<int, int>();
        var validWordLengths = new List<int>();
        int? shortest = null;
        int? longest = null;

        var dictionaryLengths = this._wordsByLength.Keys.ToArray();
        // ensure we traverse the dictionary in length order rather than addition order
        Array.Sort(array: dictionaryLengths);

        var totalWords = 0;
        foreach (var length in dictionaryLengths)
        {
            // if there are no words of this length, skip it
            if (!this._wordsByLength[key: length].Any()) continue;

            // add to the list of valid word lengths with at least one word
            validWordLengths.Add(item: length);

            var wordsForLength = this._wordsByLength[key: length].Count();
            wordCountByLength.Add(
                key: length,
                value: wordsForLength);

            totalWords += wordsForLength;

            shortest ??= length;

            // let this replace every time, the final value will be the longest
            longest = length;
        }

        this.Description = description ?? string.Empty;
        this.WordCount = totalWords;
        this._wordCountByLength = wordCountByLength;

        Debug.Assert(condition: shortest != null,
            message: nameof(shortest) + " != null");
        this.ShortestWordLength = shortest.Value;

        Debug.Assert(condition: longest != null,
            message: nameof(longest) + " != null");
        this.LongestWordLength = longest.Value;

        // now that the array is final, set it as the instance variable
        this.ValidWordLengths = validWordLengths.ToImmutableArray();
    }

    /// <summary>
    ///     This constructor builds a dictionary organized by lengths from a simple array of words
    ///     and passes it to the standard constructor
    /// </summary>
    /// <param name="words"></param>
    /// <param name="description"></param>
    // ReSharper disable once MemberCanBePrivate.Global
    public LiteralDictionary(IEnumerable<string> words, string? description = null)
        : this(
            dictionary: FillDictionary(words: words),
            description: description)
    {
    }
}