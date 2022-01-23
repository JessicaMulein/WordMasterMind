using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Models;

namespace WordMasterMind;

[TestClass]
public class WordMasterMindTest
{
    private static ScrabbleDictionary GetScrabbleDictionary()
    {
        return new ScrabbleDictionary(pathToDictionaryJson: GetTestRoot(fileName: "scrabble-dictionary.json"));
    }

    private static string GetTestRoot(string? fileName = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var startupPath = Path.GetDirectoryName(path: assembly.Location);
        Debug.Assert(condition: startupPath != null, message: nameof(startupPath) + " != null");
        var pathItems = startupPath.Split(separator: Path.DirectorySeparatorChar);
        var pos = pathItems.Reverse().ToList().FindIndex(match: x => string.Equals(a: "bin", b: x));
        var basePath = string.Join(separator: Path.DirectorySeparatorChar.ToString(),
            values: pathItems.Take(count: pathItems.Length - pos - 1));
        return fileName is null ? basePath : Path.Combine(path1: basePath, path2: fileName);
    }


    [TestMethod]
    public void TestWordMasterMindWordTooShort()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            new Models.WordMasterMind(
                minLength: 5,
                maxLength: 5,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                // secretWord is valid, but not long enough
                secretWord: "wow"));
        Assert.AreEqual(expected: "Secret word must be between minLength and maxLength",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordTooLong()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            new Models.WordMasterMind(
                minLength: 5,
                maxLength: 5,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                // secretWord is valid, but too long
                secretWord: "invalid"));
        Assert.AreEqual(expected: "Secret word must be between minLength and maxLength",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordNotInDictionary()
    {
        // secretWord is made up word not in dictionary
        const string expectedSecretWord = "fizzbuzz";
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            new Models.WordMasterMind(
                minLength: 8,
                maxLength: 8,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                secretWord: expectedSecretWord));
        Assert.AreEqual(expected: "Secret word must be a valid word in the Scrabble dictionary",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptLengthMismatch()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: 5,
            maxLength: 5,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: "valid");
        var thrownException = Assert.ThrowsException<ArgumentException>(action: () =>
            mastermind.Attempt(wordAttempt: "invalid"));
        Assert.AreEqual(expected: "Word length does not match secret word length",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptCorrect()
    {
        var secretWord = "valid";
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: 5,
            maxLength: 5,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: secretWord);
        var attempt = mastermind.Attempt(wordAttempt: secretWord);
        Assert.IsTrue(condition: mastermind.Solved);
        TestAttempt(knownSecretWord: secretWord, attemptDetails: attempt);
    }

    [TestMethod]
    public void TestWordMasterMindTooManyAttempts()
    {
        var secretWord = "valid";
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: 5,
            maxLength: 5,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: secretWord);
        for (var i = 0; i < Models.WordMasterMind.GetMaxAttemptsForLength(length: secretWord.Length); i++)
        {
            var attempt = mastermind.Attempt(wordAttempt: "wrong");
            TestAttempt(knownSecretWord: secretWord, attemptDetails: attempt);
        }

        Assert.IsFalse(condition: mastermind.Solved);
        var thrownException = Assert.ThrowsException<Exception>(action: () => mastermind.Attempt(wordAttempt: "wrong"));
        Assert.AreEqual(
            expected: "You have reached the maximum number of attempts",
            actual: thrownException.Message);
    }

    private static void TestAttempt(string knownSecretWord, IEnumerable<AttemptDetail> attemptDetails)
    {
        attemptDetails = attemptDetails.ToArray();
        var positionIndex = 0;
        foreach (var position in attemptDetails)
        {
            var correspondingSecretLetter = knownSecretWord[index: positionIndex++];
            var letterMatch = knownSecretWord.Contains(value: position.Letter);
            Assert.AreEqual(
                expected: letterMatch,
                actual: position.LetterCorrect);
            var positionMatch = position.Letter.Equals(obj: correspondingSecretLetter);
            Assert.AreEqual(
                expected: positionMatch,
                actual: position.PositionCorrect);
        }
    }


    [TestMethod]
    public void TestWordMasterMindWithProvidedRandomWord()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: 5,
            maxLength: 5,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
        var secretWord = mastermind.SecretWord.ToUpperInvariant();
        // ReSharper disable once StringLiteralTypo
        var attempt = mastermind.Attempt(wordAttempt: "aeiou").ToArray();
        Assert.AreEqual(
            expected: secretWord.Length,
            actual: attempt.Length);
        TestAttempt(knownSecretWord: secretWord, attemptDetails: attempt);
    }

    [TestMethod]
    public void TestWordMasterMindHardMode()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new Models.WordMasterMind(
            minLength: 5,
            maxLength: 5,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
    }
}