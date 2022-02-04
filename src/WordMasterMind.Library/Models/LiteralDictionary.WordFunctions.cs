using System.Text.RegularExpressions;

namespace WordMasterMind.Library.Models;

public partial class LiteralDictionary
{
    /// <summary>
    ///     Returns the number of words in the dictionary for a given word length
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public int WordCountForLength(int length)
    {
        return !this._wordCountByLength
            .ContainsKey(key: length)
            ? 0
            : this._wordCountByLength[key: length];
    }

    public IEnumerable<string> WordsForLength(int length, int maxWords = -1)
    {
        if (!this._wordsByLength.ContainsKey(key: length))
            return Array.Empty<string>();

        var words = this._wordsByLength[key: length];

        return maxWords == -1 ? words : words.Take(count: maxWords);
    }

    public IEnumerable<string> AllWords(int skip = 0, int take = -1)
    {
        return this._wordsByLength.Values
            .SelectMany(selector: x => x)
            .Skip(count: skip)
            .Take(count: take);
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

    public int RandomLength(int minLength = -1, int maxLength = -1)
    {
        if (minLength == -1) minLength = this.ShortestWordLength;

        if (maxLength == -1) maxLength = this.LongestWordLength;

        if (minLength > maxLength || maxLength < minLength)
            throw new ArgumentException(message: "minLength must be less than or equal to maxLength");

        if (minLength < this.ShortestWordLength)
            throw new ArgumentException(message: "minLength must be greater than or equal to the shortest word length");

        if (maxLength > this.LongestWordLength)
            throw new ArgumentException(message: "maxLength must be less than or equal to the longest word length");

        var random = new Random();
        var nonRandomLength = minLength == maxLength;
        return nonRandomLength
            ? maxLength
            : random.Next(minValue: minLength,
                maxValue: maxLength);
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
            var length = this.RandomLength(
                minLength: minLength,
                maxLength: maxLength);
            // set up a variable to contain a reference the dictionary entry for the length
            IEnumerable<string> wordsForLength;
            // if the dictionary does not contain any words of length, or the length had an empty array, loop again
            if (!this._wordsByLength.ContainsKey(key: length) ||
                !(wordsForLength = this._wordsByLength[key: length]).Any())
            {
                // there is no point to continue looping if min and max are locked to a value
                if (minLength == maxLength)
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
    public string FindWord(string regex, int puzzleLength, IEnumerable<string>? skipWords = null,
        IEnumerable<char>? mustIncludeLetters = null)
    {
        var skipWordsArray = skipWords is null ? Array.Empty<string>() : skipWords.ToArray();
        var mustIncludeLettersArray = mustIncludeLetters is null ? new List<char>() : mustIncludeLetters.ToList();

        var length = this.RandomLength(
            minLength: puzzleLength,
            maxLength: puzzleLength);
        var words = this._wordsByLength[key: length]
            .Where(
                predicate: w => Regex.IsMatch(
                    input: w,
                    pattern: regex))
            // check the skip words list
            .Where(
                predicate: w => skipWordsArray is null || !skipWordsArray.Contains(value: w))
            // check the must include list
            .Where(
                predicate: w => mustIncludeLettersArray.All(predicate: mustIncludeLetter
                    => w.Contains(value: mustIncludeLetter))).ToArray();

        switch (words.Length)
        {
            case 0:
                throw new Exception(message: "No words found");
            case 1:
                return words.First();
            default:
            {
                var rnd = new Random();
                return words[rnd.Next(maxValue: words.Length)];
            }
        }
    }
}