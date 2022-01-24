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
public class ScrabbleDictionaryTest
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

    private static WordDictionaryDictionary GetWordDictionary()
    {
        return new(pathToDictionaryJson: GetTestRoot(fileName: "scrabble-dictionary.json"));
    }

    [TestMethod]
    public void TestScrabbleDictionary()
    {
        var scrabbleDictionary = GetWordDictionary();

        // Test word check
        Assert.IsTrue(condition: scrabbleDictionary.IsWord(word: "hello"));
        Assert.IsTrue(condition: scrabbleDictionary.IsWord(word: "world"));
        Assert.IsTrue(condition: scrabbleDictionary.IsWord(word: "scrabble"));

        // nonsense values
        Assert.IsFalse(condition: scrabbleDictionary.IsWord(word: ""));
        // ReSharper disable once StringLiteralTypo
        Assert.IsFalse(condition: scrabbleDictionary.IsWord(word: "wordle"));
        Assert.IsFalse(condition: scrabbleDictionary.IsWord(word: "z"));
        // ReSharper disable once StringLiteralTypo
        Assert.IsFalse(condition: scrabbleDictionary.IsWord(word: "zzzzzzzzzzzzz"));

        // Test random words
        var minLength = 3;
        var maxLength = 5;
        var testWordCount = 10;
        for (var i = 0; i < testWordCount; i++)
        {
            var word = scrabbleDictionary.GetRandomWord(minLength: minLength,
                maxLength: maxLength);
            Assert.IsTrue(condition: word.Length >= minLength && word.Length <= maxLength);
            Assert.IsTrue(condition: scrabbleDictionary.IsWord(word: word));
        }

        // our scrabble json has 2-15 characters
        Assert.AreEqual(expected: 2,
            actual: scrabbleDictionary.ShortestWordLength);
        Assert.AreEqual(expected: 15,
            actual: scrabbleDictionary.LongestWordLength);
    }

    [TestMethod]
    public void TestRandomWordLengthUpperLimit()
    {
        var scrabbleDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            scrabbleDictionary.GetRandomWord(minLength: scrabbleDictionary.LongestWordLength + 1,
                maxLength: scrabbleDictionary.LongestWordLength + 1));
        Assert.AreEqual(expected: "maxLength must be less than or equal to the longest word length",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestRandomWordLengthLowerLimit()
    {
        var scrabbleDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            scrabbleDictionary.GetRandomWord(minLength: scrabbleDictionary.ShortestWordLength - 1,
                maxLength: scrabbleDictionary.ShortestWordLength - 1));
        Assert.AreEqual(expected: "minLength must be greater than or equal to the shortest word length",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestDailyWordGenerator()
    {
        var scrabbleDictionary = GetWordDictionary();

        var dayOneWord = DailyWordGenerator.WordOfTheDay(
            length: 5,
            date: DailyWordGenerator.WordGeneratorEpoch,
            dictionary: scrabbleDictionary);
        Assert.AreEqual(
            // ReSharper disable once StringLiteralTypo
            expected: "AAHED",
            actual: dayOneWord);

        var oneYearWord = DailyWordGenerator.WordOfTheDay(
            length: 5,
            date: DailyWordGenerator.WordGeneratorEpoch.AddYears(value: 1),
            dictionary: scrabbleDictionary);
        Assert.AreEqual(
            // ReSharper disable once StringLiteralTypo
            expected: "PACAS",
            actual: oneYearWord);
    }
}