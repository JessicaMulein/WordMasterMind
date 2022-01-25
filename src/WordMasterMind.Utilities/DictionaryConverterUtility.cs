using System.Text.RegularExpressions;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Library.Helpers;

public static class DictionaryConverterUtility
{
    private const int DefaultMaximumWordLength = 100;

    public static bool ConvertFile(string inputTextFilename, string? outputTextFilename = null,
        int wordLengthAbort = DefaultMaximumWordLength, bool permitNonAlpha = false, bool forceCreate = false)
    {
        outputTextFilename ??= inputTextFilename + ".json";
        if (!forceCreate && File.Exists(path: outputTextFilename))
        {
            Console.WriteLine(value: $"File {outputTextFilename} already exists. Aborting.");
            return false;
        }

        using var streamReader = new StreamReader(path: inputTextFilename);
        using var streamWriter = new StreamWriter(path: outputTextFilename,
            append: false);
        streamWriter.Write(value: "[\n");
        while (!streamReader.EndOfStream)
        {
            var newWord = streamReader.ReadLine();
            if (newWord is null)
                continue;
            if (newWord.Length >= wordLengthAbort)
            {
                Console.WriteLine(value: $"Word {newWord} is too long. Aborting.");
                return false;
            }

            newWord = newWord.ToUpperInvariant();

            if (!permitNonAlpha && Regex.IsMatch(input: newWord,
                    pattern: @"[^A-Z]"))
            {
                Console.WriteLine(value: $"Word {newWord} contains non-alpha characters. Aborting.");
                return false;
            }

            streamWriter.Write(value: $"\t\"{newWord}\",\n");
        }

        streamReader.Close();
        streamWriter.Write(value: "]\n");
        streamWriter.Flush();
        streamWriter.Close();
        return true;
    }

    public static int Main(string[] arguments)
    {
        const string usage = "Usage: DictionaryConverterUtility.exe (json|binary) <inputFilename> [<outputFilename>]";
        if (arguments.Length is > 3 or < 2)
        {
            Console.WriteLine(value: usage);
            return 1;
        }

        var jsonMode = arguments[0].ToLowerInvariant().Equals(value: "json");
        var binaryMode = arguments[0].ToLowerInvariant().Equals(value: "binary");
        if (!jsonMode && !binaryMode)
        {
            Console.WriteLine(value: "Invalid mode. Must be json or binary.");
            Console.WriteLine(value: usage);
            return 1;
        }

        var outputFile = arguments.Length > 2 ? arguments[2] : null;

        var jsonOutputFile = binaryMode ? Path.GetTempFileName() : outputFile ?? arguments[1] + ".json";
        var binaryOutputFile = outputFile ?? arguments[1] + ".bin";

        var result = ConvertFile(
            inputTextFilename: arguments[1],
            outputTextFilename: jsonOutputFile,
            wordLengthAbort: DefaultMaximumWordLength,
            permitNonAlpha: true,
            forceCreate: true);

        if (!result)
            return 1;

        if (!binaryMode)
            return result ? 0 : 1;

        var dictionary = new WordDictionaryDictionary(pathToDictionaryJson: jsonOutputFile);
        dictionary.SerializeLengthDictionary(outputFilename: binaryOutputFile);

        return result ? 0 : 1;
    }
}