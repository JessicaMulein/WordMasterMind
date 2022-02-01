using System.Text.Json;
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
        var linesProcessed = 0;
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

            linesProcessed++;

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

        Console.WriteLine(format: "Processed {0} lines.",
            arg0: linesProcessed);

        return true;
    }

    public static bool FileIsAlphabeticOnly(string filename)
    {
        return File.ReadAllLines(filename).Any(predicate: line => Regex.IsMatch(input: line,
            pattern: @"^[a-zA-Z]+[\s]*$"));
    }

    public static bool FileIsJson(string filename, out object? decodedJson)
    {
        var fileText = File.ReadAllText(path: filename);

        try
        {
            decodedJson = JsonSerializer.Deserialize<object>(
                json: fileText,
                options: new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                }) ?? throw new InvalidOperationException();

            return true;
        }
        catch
        {
            decodedJson = null;
            return false;
        }
    }

    public static int Main(string[] arguments)
    {
        const string usage = "Usage: DictionaryConverterUtility.exe (json|binary) <inputFilename> [<outputFilename>]";
        if (arguments.Length is > 3 or < 2)
        {
            Console.WriteLine(value: usage);
            return 1;
        }

        var result = false;
        var makeJsonOutput = arguments[0].ToLowerInvariant().Equals(value: "json");
        var makeBinaryOutput = arguments[0].ToLowerInvariant().Equals(value: "binary");
        if (!makeJsonOutput && !makeBinaryOutput)
        {
            Console.WriteLine(value: "Invalid mode. Must be json or binary.");
            Console.WriteLine(value: usage);
            return 1;
        }

        var outputFile = arguments.Length > 2 ? arguments[2] : null;

        var jsonOutputFile = makeBinaryOutput
            ? Path.GetTempFileName()
            : outputFile ?? Path.ChangeExtension(path: arguments[1],
                extension: ".json");
        var binaryOutputFile = outputFile ?? Path.ChangeExtension(path: arguments[1],
            extension: ".bin");

        var fileIsJson = FileIsJson(filename: arguments[1],
            decodedJson: out _);

        if (fileIsJson)
            Console.WriteLine(value: "JSON input file detected.");

        if (fileIsJson && makeJsonOutput)
            return 1;

        var fileIsAlphabeticOnly = FileIsAlphabeticOnly(filename: arguments[1]);
        if (fileIsAlphabeticOnly)
            Console.WriteLine(value: "Alphabetic input file detected.");

        if (makeJsonOutput && !fileIsAlphabeticOnly)
        {
            Console.WriteLine(value: "text input file is not alphabetic only. Aborting.");
            return 1;
        }

        if (makeJsonOutput && !fileIsJson && fileIsAlphabeticOnly)
            result = ConvertFile(
                inputTextFilename: arguments[1],
                outputTextFilename: jsonOutputFile,
                wordLengthAbort: DefaultMaximumWordLength,
                permitNonAlpha: true,
                forceCreate: true);

        if (makeJsonOutput)
            return result ? 0 : 1;

        var dictionary = LiteralDictionary.NewFromJson(
            pathToDictionaryJson: fileIsJson ? arguments[1] : jsonOutputFile);
        var wordsAdded = dictionary.Serialize(
            outputFilename: binaryOutputFile);

        Console.WriteLine(format: "Serialized {0} words to file",
            arg0: wordsAdded);
        Console.WriteLine(format: "Dictionary contains {0} words.",
            arg0: dictionary.WordCount);
        Console.WriteLine(value: "Verifying dictionary file...");

        var dictionary2 = LiteralDictionary
            .Deserialize(
                inputFilename: binaryOutputFile);
        Console.WriteLine(format: "Dictionary contains {0} words.",
            arg0: dictionary2.WordCount);

        if (dictionary2.WordCount == dictionary.WordCount)
            return result ? 0 : 1;

        Console.WriteLine(value: "Dictionary verification failed.");
        return 1;
    }
}