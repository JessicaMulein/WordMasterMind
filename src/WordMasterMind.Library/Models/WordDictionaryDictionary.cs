using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using WordMasterMind.Library.Exceptions;

namespace WordMasterMind.Library.Models;

/// <summary>
///     This is a Word Dictionary (word list/book) - dictionary (data structure) of words grouped by length.
///     It is designed for small scale usage. It is not (yet) optimized for a multi-user host.
/// </summary>
public class WordDictionaryDictionary
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true
    };

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
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly IEnumerable<int> ValidWordLengths;

    public readonly int WordCount;

    // ReSharper disable once MemberCanBePrivate.Global
    public readonly IEnumerable<int> WordLengths;

    /// <summary>
    ///     Standard constructor takes a dictionary of strings, organized by length
    /// </summary>
    /// <param name="dictionary"></param>
    /// <exception cref="Exception"></exception>
    public WordDictionaryDictionary(Dictionary<int, IEnumerable<string>>? dictionary = null)
    {
        if (dictionary is not null)
        {
            // set this as the instance dictionary, made immutable
            this._wordsByLength = dictionary.ToImmutableDictionary();
        }
        else
        {
            // this is a Blazor WebAssembly app, essentially self-hosted.
            // LoadDirectlyFromJsonFile() is a helper method that grabs it using HTTP to localhost
            var result = Task.Run(function: async () => await LoadDictionaryFromWebJson());
            // wait for the task to complete
            result.Wait();
            // set up the instance dictionary with the result, made immutable
            this._wordsByLength = result.Result.ToImmutableDictionary();
            if (this._wordsByLength.Count == 0) throw new Exception(message: "Dictionary could not be loaded");
        }

        // temporary array while we find lengths with more than one word
        var validWordLengths = new List<int>();
        int? shortest = null;
        int? longest = null;
        // ensure we traverse the dictionary in length order rather than addition order
        var dictionaryLengths = this._wordsByLength.Keys.ToArray();
        Array.Sort(array: dictionaryLengths);
        foreach (var key in dictionaryLengths)
        {
            // if there are no words of this length, skip it
            if (!this._wordsByLength[key: key].Any()) continue;

            // add to the list of valid word lengths with at least one word
            validWordLengths.Add(item: key);

            shortest ??= key;

            // let this replace every time, the final value will be the longest
            longest = key;
        }

        Debug.Assert(condition: shortest != null,
            message: nameof(shortest) + " != null");
        this.ShortestWordLength = shortest.Value;

        Debug.Assert(condition: longest != null,
            message: nameof(longest) + " != null");
        this.LongestWordLength = longest.Value;

        // now that the array is final, set it as the instance variable
        this.ValidWordLengths = validWordLengths.ToImmutableArray();

        // do the expensive computation once
        this.WordLengths = this.LiveWordLengths;
        // must have WordLengths filled in before this
        this.WordCount = this.LiveWordCount;
    }

    /// <summary>
    ///     This constructor builds a dictionary organized by lengths from a simple array of words
    ///     and passes it to the standard constructor
    /// </summary>
    /// <param name="words"></param>
    // ReSharper disable once MemberCanBePrivate.Global
    public WordDictionaryDictionary(IEnumerable<string> words) : this(dictionary: FillDictionary(words: words))
    {
    }

    /// <summary>
    ///     This constructor creates a list of words from a JSON file with an array of strings containing the words
    ///     It will get passed through FillDictionary and then the standard constructor
    /// </summary>
    /// <param name="pathToDictionaryJson"></param>
    public WordDictionaryDictionary(string pathToDictionaryJson) : this(
        words: JsonSerializer.Deserialize<string[]>(
            json: string.Join(separator: "\n",
                value: File.ReadAllLines(path: pathToDictionaryJson)),
            options: JsonSerializerOptions) ?? throw new InvalidOperationException())
    {
    }

    private IEnumerable<int> LiveWordLengths
    {
        get
        {
            var lengths = this._wordsByLength
                .Keys
                .Where(predicate: length => this.WordsForLength(length: length) > 0)
                .ToArray();
            Array.Sort(array: lengths);
            return lengths;
        }
    }

    private int LiveWordCount => this.WordLengths.Sum(selector: this.WordsForLength);

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
                    value: new[] {word.ToUpperInvariant()});
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
    ///     Save the dictionary to a binary encoded file
    /// </summary>
    /// <param name="outputFilename"></param>
    /// <exception cref="FileAlreadyExistsException"></exception>
    public int SerializeLengthDictionary(string outputFilename)
    {
        if (File.Exists(path: outputFilename))
            throw new FileAlreadyExistsException(message: $"File already exists: {outputFilename}");

        var wordCount = 0;
        using var stream = new StreamWriter(path: outputFilename);
        var writer = new BinaryWriter(output: stream.BaseStream);
        writer.Write(value: this._wordsByLength.Count);
        foreach (var (key, value) in this._wordsByLength)
        {
            writer.Write(value: key);
            var words = value.ToArray();
            writer.Write(value: words.Length);
            foreach (var word in words)
            {
                wordCount++;
                writer.Write(value: word);
            }
        }

        writer.Flush();
        stream.Flush();
        stream.Close();
        return wordCount;
    }

    /// <summary>
    ///     Read a binary encoded file and re-create a sorted dictionary from it
    /// </summary>
    /// <param name="inputFilename"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static WordDictionaryDictionary LoadDictionaryFromSerializedLengthDictionary(string inputFilename)
    {
        if (!File.Exists(path: inputFilename))
            throw new FileNotFoundException(message: "File not found",
                fileName: inputFilename);

        var stream = new StreamReader(path: inputFilename);
        var reader = new BinaryReader(input: stream.BaseStream);

        var count = reader.ReadInt32();
        var dictionary = new Dictionary<int, IEnumerable<string>>(capacity: count);
        for (var n = 0; n < count; n++)
        {
            var key = reader.ReadInt32();
            var wordCount = reader.ReadInt32();
            var words = new List<string>(capacity: wordCount);
            for (var i = 0; i < wordCount; i++)
            {
                var value = reader.ReadString();
                words.Add(item: value);
            }

            dictionary.Add(key: key,
                value: words);
        }

        return new WordDictionaryDictionary(dictionary: dictionary);
    }

    /// <summary>
    ///     Returns the number of words in the dictionary for a given word length
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public int WordsForLength(int length)
    {
        return !this._wordsByLength
            .ContainsKey(key: length)
            ? 0
            : this._wordsByLength[key: length].Count();
    }

    /// <summary>
    ///     Returns the word at the given array index for a given word length
    /// </summary>
    /// <param name="length"></param>
    /// <param name="wordIndex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public string WordAtIndex(int length, int wordIndex)
    {
        if (!this._wordsByLength.ContainsKey(key: length))
            throw new ArgumentOutOfRangeException(
                paramName: nameof(length),
                message: "No words of this length");

        var words = this._wordsByLength[key: length].ToArray();
        if (wordIndex < 0 || wordIndex >= words.Length)
            throw new ArgumentOutOfRangeException(paramName: nameof(wordIndex),
                message: "Word index out of range");

        return words[wordIndex];
    }

    /// <summary>
    ///     Returns whether the word is in the dictionary
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public bool IsWord(string word)
    {
        var length = word.Length;
        return this._wordsByLength.ContainsKey(key: length) &&
               this._wordsByLength[key: length].Contains(value: word.ToUpperInvariant());
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

        if (minLength < this.ShortestWordLength)
            throw new ArgumentException(message: "minLength must be greater than or equal to the shortest word length");

        if (maxLength > this.LongestWordLength)
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
            if (!this._wordsByLength.ContainsKey(key: length) ||
                !(wordsForLength = this._wordsByLength[key: length]).Any())
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

    /// <summary>
    ///     Attempts to find a word in the dictionary given some additional constraints
    /// </summary>
    /// <param name="knownCharacters"></param>
    /// <param name="maxIterations"></param>
    /// <param name="skipWords"></param>
    /// <param name="mustIncludeLetters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public string FindWord(in char[] knownCharacters, int maxIterations = 1000, IEnumerable<string>? skipWords = null,
        IEnumerable<char>? mustIncludeLetters = null)
    {
        if (knownCharacters.Length < this.ShortestWordLength)
            throw new ArgumentException(
                message: "knownCharacters array length must be greater than or equal to the shortest word length");

        if (knownCharacters.Length > this.LongestWordLength)
            throw new ArgumentException(
                message: "knownCharacters array length must be less than or equal to the longest word length");

        var skipWordsArray = skipWords is null ? Array.Empty<string>() : skipWords.ToArray();
        var mustIncludeLettersArray = mustIncludeLetters is null ? new List<char>() : mustIncludeLetters.ToList();

        while (maxIterations-- > 0)
        {
            var word = this.GetRandomWord(
                minLength: knownCharacters.Length,
                maxLength: knownCharacters.Length);

            // check the skip words list
            if (skipWordsArray is not null && skipWordsArray.Contains(value: word)) continue;

            // check the must include list
            var allLetters =
                mustIncludeLettersArray.All(predicate: mustIncludeLetter => word.Contains(value: mustIncludeLetter));

            if (!allLetters) continue;

            var allMatch = !knownCharacters.Where(predicate: (c, i) => c != '\0' && c != ' ' && word[index: i] != c)
                .Any();

            if (allMatch)
                return word;
        }

        throw new Exception(message: "Number of iterations exceeded without finding a matching word");
    }
}