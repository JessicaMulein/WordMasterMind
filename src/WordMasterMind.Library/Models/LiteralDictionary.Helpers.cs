using System.Net.Http.Json;
using System.Text.Json;
using WordMasterMind.Library.Enumerations;

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

    /// <summary>
    ///     Attempts to use HTTP to get the json file and then use FillDictionary to format it
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task<Dictionary<int, IEnumerable<string>>> LoadDictionaryFromWebJsonWordArray(
        LiteralDictionarySourceType sourceType)
    {
        var source = LiteralDictionarySource.FromSourceType(sourceType: sourceType);
        var jsonText = await new HttpClient().GetStringAsync(requestUri: source.FileName);
        var dictionaryWords = JsonToWords(jsonText: jsonText).ToArray();
        if (dictionaryWords is null || !dictionaryWords.Any())
            throw new Exception(message: "Dictionary could not be retrieved");

        return FillDictionary(words: dictionaryWords);
    }
}