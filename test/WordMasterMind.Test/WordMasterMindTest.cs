using System;
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
    private static LiteralDictionary GetWordDictionary()
    {
        return LiteralDictionary.Deserialize(
            inputFilename: GetTestRoot(fileName: "collins-scrabble.bin"));
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
    private static void TestAttempt(string knownSecretWord, AttemptDetails attemptDetails)
    {
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
        var literalDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            new WordMasterMindGame(
                minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength,
                hardMode: false,
                literalDictionary: literalDictionary,
                // secretWord is valid, but not long enough
                secretWord: literalDictionary.GetRandomWord(minLength: 3,
                    maxLength: Constants.StandardLength - 1)));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordTooLong()
    {
        var literalDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            new WordMasterMindGame(
                minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength,
                hardMode: false,
                literalDictionary: literalDictionary,
                // secretWord is valid, but too long
                secretWord: literalDictionary.GetRandomWord(minLength: Constants.StandardLength + 1,
                    maxLength: Constants.StandardLength + 1)));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWordNotInDictionary()
    {
        // secretWord is made up word not in dictionary
        const string expectedSecretWord = "fizzbuzz";
        var literalDictionary = GetWordDictionary();
        var thrownException = Assert.ThrowsException<NotInDictionaryException>(action: () =>
            new WordMasterMindGame(
                minLength: 8,
                maxLength: 8,
                hardMode: false,
                literalDictionary: literalDictionary,
                secretWord: expectedSecretWord));
        Assert.AreEqual(expected: NotInDictionaryException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindAttemptLengthMismatch()
    {
        var literalDictionary = GetWordDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            literalDictionary: literalDictionary,
            secretWord: literalDictionary.GetRandomWord(minLength: Constants.StandardLength,
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
        var literalDictionary = GetWordDictionary();
        var secretWord = literalDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var mastermind = new WordMasterMindGame(
            minLength: length,
            maxLength: length,
            hardMode: false,
            literalDictionary: literalDictionary,
            secretWord: secretWord);
        Assert.AreEqual(
            expected: length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        var attempt = mastermind.Attempt(wordAttempt: secretWord);
        Assert.IsTrue(condition: mastermind.GameOver);
        Assert.IsTrue(condition: mastermind.Solved);
        TestAttempt(knownSecretWord: secretWord,
            attemptDetails: attempt);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
    }

    [TestMethod]
    public void TestWordMasterMindTooManyAttempts()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var literalDictionary = GetWordDictionary();
        var secretWord = literalDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var incorrectWord = secretWord;
        while (incorrectWord.Equals(value: secretWord))
            incorrectWord = literalDictionary.GetRandomWord(minLength: length,
                maxLength: length);
        var mastermind = new WordMasterMindGame(
            minLength: length,
            maxLength: length,
            hardMode: false,
            literalDictionary: literalDictionary,
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
            WordMasterMindPlayer.AttemptComputerSolve(
                mastermind: mastermind,
                turns: 1,
                maximumDictionaryLookupAttemptsPerTry: WordMasterMindPlayer.DefaultTries,
                noStrategy: false,
                avoidSecretWord: true);
            var attempt = mastermind.Attempts.Last();
            TestAttempt(knownSecretWord: secretWord,
                attemptDetails: attempt);
            Assert.AreEqual(
                expected: mastermind.CurrentAttempt,
                actual: mastermind.Attempts.Count());
            Assert.AreEqual(
                expected: i + 1,
                actual: attempt.AttemptNumber);
        }

        Assert.IsTrue(condition: mastermind.GameOver);
        Assert.IsFalse(condition: mastermind.Solved);
        var thrownException =
            Assert.ThrowsException<GameOverException>(action: () => mastermind.Attempt(wordAttempt: "wrong"));
        Assert.IsFalse(condition: thrownException.Solved);
        Assert.AreEqual(
            expected: GameOverException.GameOverText,
            actual: thrownException.Message);

        // our Computer player needs to cover this same case where the game is over for test coverage
        var thrownGuessException = Assert.ThrowsException<GameOverException>(action: ()
            => WordMasterMindPlayer.AttemptComputerSolve(
                mastermind: mastermind,
                turns: 1));
        Assert.IsFalse(condition: thrownGuessException.Solved);
        Assert.AreEqual(
            expected: GameOverException.GameOverText,
            actual: thrownGuessException.Message);
    }

    [TestMethod]
    public void TestWordMasterMindWithProvidedRandomWordAndInvalidAttempt()
    {
        var literalDictionary = GetWordDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            literalDictionary: literalDictionary);
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
        var literalDictionary = GetWordDictionary();
        const string expectedWord = "while";
        var mastermind = new WordMasterMindGame(
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: true,
            literalDictionary: literalDictionary,
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
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
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
        var literalDictionary = GetWordDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            literalDictionary: literalDictionary);
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        WordMasterMindPlayer.AttemptComputerSolve(mastermind: mastermind,
            turns: 1);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
    }

    [TestMethod]
    public void TestSolvedLetters()
    {
        var expectedWord = "hello";
        var literalDictionary = GetWordDictionary();
        var mastermind = new WordMasterMindGame(
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: false,
            literalDictionary: literalDictionary,
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
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
        Assert.IsTrue(
            condition: mastermind.SolvedLetters.SequenceEqual(second: new[] {false, true, false, false, false}));
        // now let's solve it
        attempt = mastermind.Attempt(wordAttempt: expectedWord);
        TestAttempt(
            knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
        Assert.IsTrue(
            condition: mastermind.SolvedLetters.SequenceEqual(second: new[] {true, true, true, true, true}));
        Assert.IsTrue(condition: mastermind.GameOver);
        Assert.IsTrue(condition: mastermind.Solved);
    }

    [TestMethod]
    public void TestAttemptsFunction()
    {
        var literalDictionary = GetWordDictionary();
        foreach (var hardMode in new[] {false, true})
            for (var length = literalDictionary.ShortestWordLength;
                 length <= literalDictionary.LongestWordLength;
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