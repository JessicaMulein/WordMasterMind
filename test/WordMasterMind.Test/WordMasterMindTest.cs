using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Exceptions;
using WordMasterMind.Library.Helpers;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Test;

[TestClass]
public class WordMasterMindTest
{
    private static LiteralDictionary GetWordDictionary()
    {
        using var stream = LiteralDictionary.OpenFileForRead(
            fileName: Utilities.GetTestRoot(
                fileName: "collins-scrabble.bin"));
        return LiteralDictionary.Deserialize(
            sourceType: LiteralDictionarySourceType.Scrabble,
            inputStream: stream);
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
            var positionMatch = position.Letter.Equals(obj: correspondingSecretLetter);
            var expected = positionMatch
                ? LetterEvaluation.Correct
                : letterMatch
                    ? LetterEvaluation.Present
                    : LetterEvaluation.Absent;
            Assert.AreEqual(
                expected: expected,
                actual: position.Evaluation);
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
                secretWord: literalDictionary.GetRandomWord(
                    minLength: 3,
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
            mastermind.MakeAttempt(wordAttempt: "invalid"));
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
        var attempt = mastermind.MakeAttempt(wordAttempt: secretWord);
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
                 length: length);
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
            Assert.ThrowsException<GameOverException>(action: () => mastermind.MakeAttempt(wordAttempt: "wrong"));
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
            mastermind.MakeAttempt(wordAttempt: invalidSecretWord));
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
        var attempt = mastermind.MakeAttempt(wordAttempt: "where");
        TestAttempt(knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
        // this should throw an exception because we've changed the 'w' to 't' in a locked position, and 
        var thrownException =
            Assert.ThrowsException<HardModeException>(action: () => mastermind.MakeAttempt(wordAttempt: "there"));
        Assert.AreEqual(
            expected:
            $"{Utilities.NumberToOrdinal(number: Utilities.HumanizeIndex(index: thrownException.LetterPosition))} letter must be {thrownException.Letter}",
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
            turns: mastermind.MaxAttempts - 1);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
    }

    [TestMethod]
    public void TestSolvedLetters()
    {
        var expectedWord = "HELLO";
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
        // a first guess of "weigh" should register "e" as a solved letter, regardless of hardmode, and "h" as a solved letter
        var attempt = mastermind.MakeAttempt(wordAttempt: "WEIGH");
        TestAttempt(
            knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
        Assert.IsTrue(
            condition: mastermind.SolvedLetters.SequenceEqual(second: new[] {false, true, false, false, false}));
        Assert.IsTrue(
            condition: mastermind.FoundLetters.SequenceEqual(second: new[] {'E', 'H'}));
        // now let's solve it
        attempt = mastermind.MakeAttempt(wordAttempt: expectedWord);
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
                    length: length);
                Assert.AreEqual(
                    expected: length + 1,
                    actual: attemptsForLength);
            }
    }

    /* TODO: I'd like to have a test that verifies the SecretWord is only accessible when the game is complete,
     * but the Test framework by default makes it impossible to test all of the paths without introducing more */
}