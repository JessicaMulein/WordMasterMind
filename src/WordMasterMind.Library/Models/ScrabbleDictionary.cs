using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace WordMasterMind.Library.Models;

public class ScrabbleDictionary
{
    /// <summary>
    ///     Unalterable collection of dictionary words, organized by length
    /// </summary>
    private readonly ImmutableDictionary<int, IEnumerable<string>> _wordsByLength;

    /// <summary>
    ///     Longest word length with at least one word
    /// </summary>
    public readonly int LongestWordLength;

    /// <summary>
    ///     Shortest word length with at least one word
    /// </summary>
    public readonly int ShortestWordLength;

    /// <summary>
    ///     Word lengths known to have at least one word
    /// </summary>
    public readonly IEnumerable<int> ValidWordLengths;

    /// <summary>
    ///     Standard constructor takes a dictionary of strings, organized by length
    /// </summary>
    /// <param name="dictionary"></param>
    /// <exception cref="Exception"></exception>
    public ScrabbleDictionary(Dictionary<int, IEnumerable<string>>? dictionary = null)
    {
        if (dictionary is not null)
        {
            // set this as the instance dictionary, made immutable
            _wordsByLength = dictionary.ToImmutableDictionary();
        }
        else
        {
            // this is a Blazor WebAssembly app, essentially self-hosted.
            // LoadDirectlyFromJsonFile() is a helper method that grabs it using HTTP to localhost
            var result = Task.Run(function: async () => await LoadDictionaryFromWebJson());
            // wait for the task to complete
            result.Wait();
            // set up the instance dictionary with the result, made immutable
            _wordsByLength = result.Result.ToImmutableDictionary();
            if (_wordsByLength.Count == 0) throw new Exception(message: "Dictionary could not be loaded");
        }

        // temporary array while we find lengths with more than one word
        var validWordLengths = new List<int>();
        int? shortest = null;
        int? longest = null;
        // ensure we traverse the dictionary in length order rather than addition order
        var dictionaryLengths = _wordsByLength.Keys.ToArray();
        Array.Sort(array: dictionaryLengths);
        foreach (var key in dictionaryLengths)
        {
            // if there are no words of this length, skip it
            if (!_wordsByLength[key: key].Any()) continue;

            // add to the list of valid word lengths with at least one word
            validWordLengths.Add(item: key);

            shortest ??= key;

            // let this replace every time, the final value will be the longest
            longest = key;
        }

        Debug.Assert(condition: shortest != null,
            message: nameof(shortest) + " != null");
        ShortestWordLength = shortest.Value;

        Debug.Assert(condition: longest != null,
            message: nameof(longest) + " != null");
        LongestWordLength = longest.Value;

        // now that the array is final, set it as the instance variable
        ValidWordLengths = validWordLengths.ToImmutableArray();
    }

    /// <summary>
    ///     This constructor builds a dictionary organized by lengths from a simple array of words
    ///     and passes it to the standard constructor
    /// </summary>
    /// <param name="words"></param>
    public ScrabbleDictionary(IEnumerable<string> words) : this(dictionary: FillDictionary(words: words))
    {
    }

    /// <summary>
    ///     This constructor creates a list of words from a JSON file with an array of strings containing the words
    ///     It will get passed through FillDictionary and then the standard constructor
    /// </summary>
    /// <param name="pathToDictionaryJson"></param>
    public ScrabbleDictionary(string pathToDictionaryJson) : this(
        words: JsonSerializer.Deserialize<string[]>(json: string.Join(separator: "\n",
            value: File.ReadAllLines(path: pathToDictionaryJson))) ?? throw new InvalidOperationException())
    {
    }

    /// <summary>
    ///     Helper method to make a dictionary organized by lengths from a simple array of words
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    private static Dictionary<int, IEnumerable<string>> FillDictionary(in IEnumerable<string> words)
    {
        var dictionary = new Dictionary<int, IEnumerable<string>>();
        foreach (var word in words)
        {
            var wordLength = word.Length;
            if (dictionary.ContainsKey(key: wordLength))
                dictionary[key: wordLength] = dictionary[key: wordLength].Append(element: word.ToUpperInvariant());
            else
                dictionary.Add(key: wordLength,
                    value: new[] { word.ToUpperInvariant() });
        }

        return dictionary;
    }

    /// <summary>
    ///     Attempts to use HTTP to get the json file and then use FillDictionary to format it
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task<Dictionary<int, IEnumerable<string>>> LoadDictionaryFromWebJson()
    {
        var dictionaryWords =
            await new HttpClient().GetFromJsonAsync<string[]>(requestUri: "/scrabble-dictionary.json");
        if (dictionaryWords is null || !dictionaryWords.Any())
            throw new Exception(message: "Dictionary could not be retrieved");

        return FillDictionary(words: dictionaryWords);
    }

    /// <summary>
    ///     Returns whether the word is in the dictionary
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public bool IsWord(string word)
    {
        var length = word.Length;
        return _wordsByLength.ContainsKey(key: length) &&
               _wordsByLength[key: length].Contains(value: word.ToUpperInvariant());
    }

    /// <summary>
    ///     Gets a random word from the dictionary from a random length between minLength and maxLength
    /// </summary>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public string GetRandomWord(int minLength, int maxLength)
    {
        if (minLength > maxLength || maxLength < minLength)
            throw new ArgumentException(message: "minLength must be less than or equal to maxLength");

        if (minLength < ShortestWordLength)
            throw new ArgumentException(message: "minLength must be greater than or equal to the shortest word length");

        if (maxLength > LongestWordLength)
            throw new ArgumentException(message: "maxLength must be less than or equal to the longest word length");

        var random = new Random();
        /* maxTries is the number of tries to find a word of length between minLength and maxLength
         * that is of a length included in the dictionary. Although the dictionary is known to have
         * words of length between minLength and maxLength, it is possible that the dictionary does
         * not have words of a specific length between the minLength and maxLength, though I don't
         * think that is possible in the supplied dictionary. (untested assertion)
         * A non standard dictionary may have been provided, so we must ensure we test that the length
         * has any words
         * It is very unlikely that the standard dictionary will have any trouble of finding words
         * of length between minLength and maxLength, but we will allow for some number of retries
         * of different random lengths to find a word of a length included in the dictionary.
         */
        var maxTries = 10000;
        while (maxTries-- > 0)
        {
            // get a random length between minLength and maxLength
            var nonRandomLength = minLength == maxLength;
            var length = nonRandomLength
                ? maxLength
                : random.Next(minValue: minLength,
                    maxValue: maxLength);
            // set up a variable to contain a reference the dictionary entry for the length
            IEnumerable<string> wordsForLength;
            // if the dictionary does not contain any words of length, or the length had an empty array, loop again
            if (!_wordsByLength.ContainsKey(key: length) ||
                !(wordsForLength = _wordsByLength[key: length]).Any())
            {
                // there is no point to continue looping if min and max are locked to a value
                if (nonRandomLength)
                    break;

                // continue looping, but skip the following code
                continue;
            }

            ;

            /* at this point we have a length that is included in the dictionary
             * and we have at least one word of that length
             * this is guaranteed to return a word of the desired length
             * enumerate to an array to prevent multiple enumerations during the return call
            */
            var wordsForLengthArray = wordsForLength.ToArray();

            return wordsForLengthArray
                .ElementAt(index: random.Next(minValue: 0,
                    maxValue: wordsForLengthArray.Length));
        } // end while maxTries

        // we've failed out of the loop
        throw new Exception(message: "Dictionary doesn't seem to have any words of the requested parameters");
    }

    public string FindWord(in char[] knownCharacters, int maxIterations = 1000, IEnumerable<string>? skipWords = null,
        IEnumerable<char>? mustIncludeLetters = null)
    {
        if (knownCharacters.Length < ShortestWordLength)
            throw new ArgumentException(
                message: "knownCharacters array length must be greater than or equal to the shortest word length");

        if (knownCharacters.Length > LongestWordLength)
            throw new ArgumentException(
                message: "knownCharacters array length must be less than or equal to the longest word length");

        var skipWordsArray = skipWords is null ? Array.Empty<string>() : skipWords.ToArray();
        var mustIncludeLettersArray = mustIncludeLetters is null ? new List<char>() : mustIncludeLetters.ToList();

        while (maxIterations-- > 0)
        {
            var word = GetRandomWord(
                minLength: knownCharacters.Length,
                maxLength: knownCharacters.Length);

            // check the skip words list
            if (skipWordsArray is not null && skipWordsArray.Contains(value: word)) continue;

            // check the must include list
            var allLetters = true;
            foreach (var mustIncludeLetter in mustIncludeLettersArray)
                if (!word.Contains(value: mustIncludeLetter))
                {
                    allLetters = false;
                    break;
                }

            if (!allLetters) continue;

            var allMatch = true;
            for (var i = 0; i < knownCharacters.Length; i++)
            {
                if (knownCharacters[i] == '\0' || knownCharacters[i] == ' ') continue;

                if (word[index: i] != knownCharacters[i])
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch)
                return word;
        }

        throw new Exception(message: "Number of iterations exceeded without finding a matching word");
    }
}