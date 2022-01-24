using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Library.Exceptions;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Test;

[TestClass]
public class WordMasterMindTest
{
    private static ScrabbleDictionary GetScrabbleDictionary()
    {
        return new ScrabbleDictionary(pathToDictionaryJson: GetTestRoot(fileName: "scrabble-dictionary.json"));
    }

    /// <summary>
    ///     Gets the path to the test project's root directory.
    /// </summary>
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

    /// <summary>
    ///     Inspects an attempt result and makes sure it is valid.
    /// </summary>
    /// <param name="knownSecretWord"></param>
    /// <param name="attemptDetails"></param>
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
    public void TestWordMasterMindWordTooShort()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            new WordMasterMindGame(
                minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                // secretWord is valid, but not long enough
                secretWord: scrabbleDictionary.GetRandomWord(minLength: 3,
                    maxLength: Constants.StandardLength - 1)));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordTooLong()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            new WordMasterMindGame(
                minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                // secretWord is valid, but too long
                secretWord: scrabbleDictionary.GetRandomWord(minLength: Constants.StandardLength + 1,
                    maxLength: Constants.StandardLength + 1)));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordNotInDictionary()
    {
        // secretWord is made up word not in dictionary
        const string expectedSecretWord = "fizzbuzz";
        var scrabbleDictionary = GetScrabbleDictionary();
        var thrownException = Assert.ThrowsException<NotInDictionaryException>(action: () =>
            new WordMasterMindGame(
                minLength: 8,
                maxLength: 8,
                hardMode: false,
                scrabbleDictionary: scrabbleDictionary,
                secretWord: expectedSecretWord));
        Assert.AreEqual(expected: NotInDictionaryException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptLengthMismatch()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: scrabbleDictionary.GetRandomWord(minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength));
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        var thrownException = Assert.ThrowsException<InvalidAttemptLengthException>(action: () =>
            mastermind.Attempt(wordAttempt: "invalid"));
        Assert.AreEqual(expected: InvalidAttemptLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptCorrect()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var scrabbleDictionary = GetScrabbleDictionary();
        var secretWord = scrabbleDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var mastermind = new WordMasterMindGame(
            minLength: length,
            maxLength: length,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: secretWord);
        Assert.AreEqual(
            expected: length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        var attempt = mastermind.Attempt(wordAttempt: secretWord);
        Assert.IsTrue(condition: mastermind.Solved);
        TestAttempt(knownSecretWord: secretWord,
            attemptDetails: attempt);
    }

    [TestMethod]
    public void TestWordMasterMindTooManyAttempts()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var scrabbleDictionary = GetScrabbleDictionary();
        var secretWord = scrabbleDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var incorrectWord = secretWord;
        while (incorrectWord.Equals(value: secretWord))
            incorrectWord = scrabbleDictionary.GetRandomWord(minLength: length,
                maxLength: length);
        var mastermind = new WordMasterMindGame(
            minLength: length,
            maxLength: length,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: secretWord);
        Assert.AreEqual(
            expected: length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        for (var i = 0;
             i < WordMasterMindGame.GetMaxAttemptsForLength(
                 length: length,
                 hardMode: mastermind.HardMode);
             i++)
        {
            var attempt = mastermind.Attempt(wordAttempt: incorrectWord);
            TestAttempt(knownSecretWord: secretWord,
                attemptDetails: attempt);
        }

        Assert.IsFalse(condition: mastermind.Solved);
        var thrownException =
            Assert.ThrowsException<GameOverException>(action: () => mastermind.Attempt(wordAttempt: "wrong"));
        Assert.IsFalse(condition: thrownException.Solved);
        Assert.AreEqual(
            expected: GameOverException.GameOverText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWithProvidedRandomWordAndInvalidAttempt()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        var secretWord = mastermind.SecretWord.ToUpperInvariant();
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: secretWord.Length);
        var invalidSecretWord = "".PadLeft(totalWidth: Constants.StandardLength,
            paddingChar: 'z');
        var thrownAssertion = Assert.ThrowsException<NotInDictionaryException>(action: () =>
            mastermind.Attempt(wordAttempt: invalidSecretWord));
        Assert.AreEqual(
            expected: NotInDictionaryException.MessageText,
            actual: thrownAssertion.Message);
    }

    [TestMethod]
    public void TestWordMasterMindHardMode()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        const string expectedWord = "while";
        var mastermind = new WordMasterMindGame(
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: true,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: expectedWord);
        Assert.AreEqual(
            expected: expectedWord.Length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: true,
            actual: mastermind.HardMode);
        // a first attempt of where should lock in three letters, 'w', 'h', and the final 'e'
        var attempt = mastermind.Attempt(wordAttempt: "where");
        TestAttempt(knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);

        // this should throw an exception because we've changed the 'w' to 't' in a locked position
        var thrownException =
            Assert.ThrowsException<HardModeException>(action: () => mastermind.Attempt(wordAttempt: "there"));
        Assert.AreEqual(
            expected: HardModeException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestAttemptsToString()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary);
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        WordMasterMindPlayer.ComputerGuess(mastermind: mastermind,
            turns: 1);
    }

    [TestMethod]
    public void TestSolvedLetters()
    {
        var expectedWord = "hello";
        var scrabbleDictionary = GetScrabbleDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: false,
            scrabbleDictionary: scrabbleDictionary,
            secretWord: expectedWord);
        Assert.AreEqual(
            expected: expectedWord.Length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        // a guess of "weight" should register "e" as a solved letter, regardless of hardmode
        var attempt = mastermind.Attempt(wordAttempt: "weigh");
        TestAttempt(
            knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);
        Assert.IsTrue(
            condition: mastermind.SolvedLetters.SequenceEqual(second: new[] {false, true, false, false, false}));
        // now let's solve it
        attempt = mastermind.Attempt(wordAttempt: expectedWord);
        TestAttempt(
            knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);
        Assert.IsTrue(
            condition: mastermind.SolvedLetters.SequenceEqual(second: new[] {true, true, true, true, true}));
        Assert.IsTrue(condition: mastermind.Solved);
    }

    [TestMethod]
    public void TestAttemptsFunction()
    {
        var scrabbleDictionary = GetScrabbleDictionary();
        foreach (var hardMode in new[] {false, true})
            for (var length = scrabbleDictionary.ShortestWordLength;
                 length <= scrabbleDictionary.LongestWordLength;
                 length++)
            {
                var attemptsForLength = WordMasterMindGame.GetMaxAttemptsForLength(
                    length: length,
                    hardMode: hardMode);
                Assert.AreEqual(
                    expected: length + (hardMode ? 2 : 1),
                    actual: attemptsForLength);
            }
    }

    /* TODO: I'd like to have a test that verifies the SecretWord is only accessible when the game is complete,
     * but the Test framework by default makes it impossible to test all of the paths without introducing more */
}