using System.IO;
using GameEngine.Library.Enumerations;
using GameEngine.Library.Exceptions;
using GameEngine.Library.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEngine.Test;

[TestClass]
public class LiteralDictionaryTest
{
    [TestMethod]
    public void TestLiteralDictionary()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        Assert.AreEqual(
            expected: 279496, // known size of collins scrabble dictionary
            actual: literalDictionary.WordCount);

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
            var word = literalDictionary.GetRandomWord(
                minLength: minLength,
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
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            literalDictionary.GetRandomWord(minLength: literalDictionary.LongestWordLength + 1,
                maxLength: literalDictionary.LongestWordLength + 1));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestRandomWordLengthLowerLimit()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            literalDictionary.GetRandomWord(
                minLength: literalDictionary.ShortestWordLength - 1,
                maxLength: literalDictionary.ShortestWordLength - 1));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestDailyWordGenerator()
    {
        var literalDictionary =
            TestHelpers.GetWordDictionaryFromTestRoot(sourceType: LiteralDictionarySourceType.CollinsScrabble);

        var dayOneWord = DailyWordGenerator.WordOfTheDay(
            date: DailyWordGenerator.WordGeneratorEpoch,
            length: 5,
            dictionary: literalDictionary);
        Assert.AreEqual(
            // ReSharper disable once StringLiteralTypo
            expected:
            "AAHED", // this is the 3rd word in the scrabble dictionary, given the seed system, this is unlikely to be correct
            actual: dayOneWord);

        var oneYearWord = DailyWordGenerator.WordOfTheDay(
            date: DailyWordGenerator.WordGeneratorEpoch.AddYears(value: 1),
            length: 5,
            dictionary: literalDictionary);
        Assert.AreEqual(
            // ReSharper disable once StringLiteralTypo
            expected: "JOKED",
            actual: oneYearWord);
    }

    [TestMethod]
    public void TestBinarySerializationAndLoading()
    {
        var dictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var binaryOutputFile = Path.GetTempFileName();
        File.Delete(path: binaryOutputFile);
        Assert.IsFalse(condition: File.Exists(path: binaryOutputFile));

        var wordsAdded = dictionary.Serialize(outputFilename: binaryOutputFile);
        Assert.AreEqual(
            expected: dictionary.WordCount,
            actual: wordsAdded);
        Assert.IsTrue(condition: File.Exists(path: binaryOutputFile));

        var dictionary2 = LiteralDictionary.Deserialize(
            sourceType: dictionary.SourceType,
            inputStream: LiteralDictionary.OpenFileForRead(fileName: binaryOutputFile));
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

    [DataTestMethod]
    [DataRow(data1: LiteralDictionarySourceType.Crossword,
        2,
        22,
        62799)]
    [DataRow(data1: LiteralDictionarySourceType.CollinsScrabble,
        2,
        15,
        279496)]
    [DataRow(data1: LiteralDictionarySourceType.English,
        1,
        31,
        370103)]
    public void TestDictionarySources(LiteralDictionarySourceType sourceType, int expectedShortestWord,
        int expectedLongestWord, int expectedTotalWords)
    {
        var dictionary = TestHelpers.GetWordDictionaryFromTestRoot(sourceType: sourceType);
        Assert.AreEqual(
            expected: expectedShortestWord,
            actual: dictionary.ShortestWordLength);
        Assert.AreEqual(
            expected: expectedLongestWord,
            actual: dictionary.LongestWordLength);
        Assert.AreEqual(
            expected: expectedTotalWords,
            actual: dictionary.WordCount);
    }
}