using System.Text.Json.Nodes;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Exceptions;

namespace WordMasterMind.Library.Models;

public partial class LiteralDictionary
{
    public static Stream OpenFileForRead(string fileName)
    {
        if (!File.Exists(path: fileName))
            throw new FileNotFoundException(message: "File not found",
                fileName: fileName);

        return File.Open(
            path: fileName,
            mode: FileMode.Open,
            access: FileAccess.Read);
    }

    /// <summary>
    ///     Read a binary encoded file and re-create a sorted dictionary from it
    ///     TODO: re-serialize dictionaries with Description included, add to deserialize
    /// </summary>
    /// <param name="sourceType"></param>
    /// <param name="inputStream"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static LiteralDictionary Deserialize(
        LiteralDictionarySourceType sourceType,
        Stream inputStream,
        string? description = null)
    {
        using var reader = new BinaryReader(input: inputStream);

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

        reader.Close();
        inputStream.Close();

        return new LiteralDictionary(
            sourceType: sourceType,
            dictionary: dictionary,
            description: description);
    }

    /// <summary>
    ///     Save the dictionary to a binary encoded file
    /// </summary>
    /// <param name="outputFilename"></param>
    /// <exception cref="FileAlreadyExistsException"></exception>
    public int Serialize(string outputFilename)
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

        return wordCount;
    }

    public JsonObject ToJson()
    {
        throw new NotImplementedException();
    }
}