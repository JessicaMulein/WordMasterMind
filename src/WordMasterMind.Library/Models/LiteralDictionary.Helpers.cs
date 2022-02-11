using System.Text.Json;

namespace WordMasterMind.Library.Models;

public partial class LiteralDictionary
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
    };

    /// <summary>
    ///     Helper method to make a dictionary organized by lengths from a simple array of words
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    private static Dictionary<int, IEnumerable<string>> FillDictionary(in IEnumerable<string> words)
    {
        var dictionary = new Dictionary<int, IEnumerable<string>>();
        foreach (var word in words
                     .Select(selector: w => w
                         .Trim()
                         .ToUpperInvariant())
                     .Where(predicate: w => w.Length > 0))
        {
            var wordLength = word.Length;
            if (dictionary.ContainsKey(key: wordLength))
                dictionary[key: wordLength] = dictionary[key: wordLength].Append(element: word);
            else
                dictionary.Add(key: wordLength,
                    value: new[] {word});
        }

        return AlphabetizeDictionary(dictionary: dictionary);
    }

    private static Dictionary<int, IEnumerable<string>> AlphabetizeDictionary(
        in Dictionary<int, IEnumerable<string>> dictionary)
    {
        foreach (var (length, wordsForLength) in dictionary)
        {
            var arrayToSort = wordsForLength.ToArray();
            // skip empty arrays
            if (!arrayToSort.Any())
                continue;
            Array.Sort(array: arrayToSort);
            dictionary[key: length] = arrayToSort;
        }

        return dictionary;
    }
}