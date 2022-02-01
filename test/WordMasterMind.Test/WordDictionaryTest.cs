using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Test;

[TestClass]
public class LiteralDictionaryTest
{
    private static string GetTestRoot(string? fileName = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var startupPath = Path.GetDirectoryName(path: assembly.Location);
        Debug.Assert(condition: startupPath != null,
            message: nameof(startupPath) + " != null");
        var pathItems = startupPath.Split(separator: Path.DirectorySeparatorChar);
        var pos = pathItems.Reverse().ToList().FindIndex(match: x => string.Equals(a: "bin",
            b: x));
        var basePath = string.Join(separator: Path.DirectorySeparatorChar.ToString(),
            values: pathItems.Take(count: pathItems.Length - pos - 1));
        return fileName is null
            ? basePath
            : Path.Combine(path1: basePath,
                path2: fileName);
    }

    private static LiteralDictionary GetWordDictionary()
    {
        return LiteralDictionary.Deserialize(
            inputFilename: GetTestRoot(fileName: "scrabble-dictionary.bin"));
    }

    [TestMethod]
    public void TestLiteralDictionary()
    {
        var literalDictionary = GetWordDictionary();

        // Test word check
        Assert.IsTrue(condition: literalDictionary.IsWord(word: "hello"));
        Assert.IsTrue(condition: literalDictionary.IsWord(word: "world"));
        Assert.IsTrue(condition: literalDictionary.IsWord(word: "scrabble"));

        // nonsense values
        Assert.IsFalse(condition: literalDictionary.IsWord(word: ""));
        // ReSharper disable once StringLiteralTypo
        Assert.IsFalse(condition: literalDictionary.IsWord(word: "wordle"));
        Assert.IsFalse(condition: literalDictionary.IsWord(word: "z"));
        // ReSharper disable once StringLiteralTypo
        Assert.IsFalse(condition: literalDictionary.IsWord(word: "zzzzzzzzzzzzz"));

        // Test random words
        var minLength = 3;
        var maxLength = 5;
        var testWordCount = 10;
        for (var i = 0; i < testWordCount; i++)
        {
            var word = literalDictionary.GetRandomWord(minLength: minLength,
                maxLength: maxLength);
            Assert.IsTrue(condition: word.Length >= minLength && word.Length <= maxLength);
            Assert.IsTrue(condition: literalDictionary.IsWord(word: word));
        }

        // our scrabble json has 2-15 characters
        Assert.AreEqual(expected: 2,
            actual: literalDictionary.ShortestWordLength);
        Assert.AreEqual(expected: 15,
            actual: literalDictionary.LongestWordLength);
    }

    [TestMethod]
    public void TestRandomWordLengthUpperLimit()
    {
        var literalDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            literalDictionary.GetRandomWord(minLength: literalDictionary.LongestWordLength + 1,
                maxLength: literalDictionary.LongestWordLength + 1));
        Assert.AreEqual(expected: "maxLength must be less than or equal to the longest word length",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestRandomWordLengthLowerLimit()
    {
        var literalDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            literalDictionary.GetRandomWord(minLength: literalDictionary.ShortestWordLength - 1,
                maxLength: literalDictionary.ShortestWordLength - 1));
        Assert.AreEqual(expected: "minLength must be greater than or equal to the shortest word length",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestDailyWordGenerator()
    {
        var literalDictionary = GetWordDictionary();

        var dayOneWord = DailyWordGenerator.WordOfTheDay(
            length: 5,
            date: DailyWordGenerator.WordGeneratorEpoch,
            dictionary: literalDictionary);
        Assert.AreEqual(
            // ReSharper disable once StringLiteralTypo
            expected: "AAHED",
            actual: dayOneWord);

        var oneYearWord = DailyWordGenerator.WordOfTheDay(
            length: 5,
            date: DailyWordGenerator.WordGeneratorEpoch.AddYears(value: 1),
            dictionary: literalDictionary);
        Assert.AreEqual(
            // ReSharper disable once StringLiteralTypo
            expected: "PACAS",
            actual: oneYearWord);
    }

    [TestMethod]
    public void TestBinarySerializationAndLoading()
    {
        var dictionary = GetWordDictionary();
        var binaryOutputFile = Path.GetTempFileName();
        File.Delete(path: binaryOutputFile);
        Assert.IsFalse(condition: File.Exists(path: binaryOutputFile));

        var wordsAdded = dictionary.Serialize(outputFilename: binaryOutputFile);
        Assert.AreEqual(
            expected: dictionary.WordCount,
            actual: wordsAdded);
        Assert.IsTrue(condition: File.Exists(path: binaryOutputFile));

        var dictionary2 = LiteralDictionary.Deserialize(
            inputFilename: binaryOutputFile);
        Assert.AreEqual(
            expected: dictionary2.WordCount,
            actual: dictionary.WordCount);
        File.Delete(path: binaryOutputFile);
        Assert.IsFalse(condition: File.Exists(path: binaryOutputFile));
    }

    [TestMethod]
    public void TestDailyPuzzleNumberEpoch()
    {
        Assert.AreEqual(
            expected: 1,
            actual: DailyWordGenerator.PuzzleNumber(date: DailyWordGenerator.WordGeneratorEpoch));
    }
}