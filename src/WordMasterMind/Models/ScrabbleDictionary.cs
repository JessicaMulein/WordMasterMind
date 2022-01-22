using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;

namespace WordMasterMind.Models;

public class ScrabbleDictionary
{
    private readonly ImmutableDictionary<int, IEnumerable<string>> _wordsByLength;

    public ScrabbleDictionary(string pathToDictionaryJson) : this(
        words: JsonSerializer.Deserialize<string[]>(json: string.Join(separator: "\n",
            value: File.ReadAllLines(path: pathToDictionaryJson))) ?? throw new InvalidOperationException())
    {
    }

    public ScrabbleDictionary(Dictionary<int, IEnumerable<string>>? dictionary = null)
    {
        if (dictionary is not null)
        {
            _wordsByLength = dictionary.ToImmutableDictionary();
        }
        else
        {
            var result = Task.Run(function: async () => await LoadDictionaryFromWebJson());
            result.Wait();
            _wordsByLength = result.Result.ToImmutableDictionary();
            if (_wordsByLength.Count == 0) throw new Exception(message: "Dictionary could not be loaded");
        }
    }

    public ScrabbleDictionary(IEnumerable<string> words) : this(dictionary: FillDictionary(words: words))
    {
    }

    private static Dictionary<int, IEnumerable<string>> FillDictionary(in IEnumerable<string> words)
    {
        var dictionary = new Dictionary<int, IEnumerable<string>>();
        foreach (var word in words)
        {
            var wordLength = word.Length;
            if (dictionary.ContainsKey(key: wordLength))
                dictionary[key: wordLength] = dictionary[key: wordLength].Append(element: word.ToUpperInvariant());
            else
                dictionary.Add(key: wordLength, value: new[] {word.ToUpperInvariant()});
        }

        return dictionary;
    }

    private static async Task<Dictionary<int, IEnumerable<string>>> LoadDictionaryFromWebJson()
    {
        var dictionaryWords =
            await new HttpClient().GetFromJsonAsync<string[]>(requestUri: "/scrabble-dictionary.json");
        if (dictionaryWords is null || !dictionaryWords.Any())
            throw new Exception(message: "Dictionary could not be retrieved");

        return FillDictionary(words: dictionaryWords);
    }


    public bool IsWord(string word)
    {
        var length = word.Length;
        return _wordsByLength.ContainsKey(key: length) &&
               _wordsByLength[key: length].Contains(value: word.ToUpperInvariant());
    }

    public string GetRandomWord(int minLength, int maxLength)
    {
        var random = new Random();
        var maxTries = 1000;
        while (maxTries-- > 0)
        {
            var length = random.Next(minValue: minLength, maxValue: maxLength);
            var countForLength = _wordsByLength[key: length].Count();
            if (countForLength == 0) continue;
            return _wordsByLength[key: length].ElementAt(index: random.Next(minValue: 0, maxValue: countForLength));
        }

        throw new Exception(message: "Dictionary doesn't seem to have any words of the requested parameters");
    }
}