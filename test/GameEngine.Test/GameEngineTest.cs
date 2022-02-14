using System;
using System.Linq;
using GameEngine.Library.Enumerations;
using GameEngine.Library.Exceptions;
using GameEngine.Library.Helpers;
using GameEngine.Library.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameEngine.Test;

[TestClass]
public class GameEngineTest
{
    [TestInitialize]
    public void TestSetup()
    {
        UnitTestDetector.ForceTestMode(value: true);
    }

    /// <summary>
    ///     Inspects an attempt result and makes sure it is valid.
    /// </summary>
    /// <param name="knownSecretWord"></param>
    /// <param name="attemptDetails"></param>
    private static void VerifyTestAttempt(string knownSecretWord, AttemptDetails attemptDetails)
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
    public void TestGameEngineWordTooShort()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            new GameEngineInstance(
                literalDictionary: literalDictionary,
                minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength,
                hardMode: false,
                dailyWordWhenComputer: true,
                // secretWord is valid, but not long enough
                secretWord: literalDictionary.GetRandomWord(
                    minLength: 3,
                    maxLength: Constants.StandardLength - 1)));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestGameEngineWordTooLong()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: () =>
            new GameEngineInstance(
                literalDictionary: literalDictionary,
                minLength: Constants.StandardLength,
                maxLength: Constants.StandardLength,
                hardMode: false,
                dailyWordWhenComputer: true,
                // secretWord is valid, but too long
                secretWord: literalDictionary.GetRandomWord(minLength: Constants.StandardLength + 1,
                    maxLength: Constants.StandardLength + 1)));
        Assert.AreEqual(expected: InvalidLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestGameEngineWordNotInDictionary()
    {
        // secretWord is made up word not in dictionary
        const string expectedSecretWord = "fizzbuzz";
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var thrownException = Assert.ThrowsException<NotInDictionaryException>(action: () =>
            new GameEngineInstance(
                literalDictionary: literalDictionary,
                minLength: 8,
                maxLength: 8,
                hardMode: false,
                dailyWordWhenComputer: true,
                secretWord: expectedSecretWord));
        Assert.AreEqual(expected: NotInDictionaryException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestGameEngineAttemptLengthMismatch()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: null);
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        var thrownException = Assert.ThrowsException<InvalidAttemptLengthException>(action: () =>
            mastermind.MakeAttempt(
                wordAttempt: literalDictionary.GetRandomWord(
                    minLength: Constants.StandardLength + 1,
                    maxLength: Constants.StandardLength + 1)));
        Assert.AreEqual(expected: InvalidAttemptLengthException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestGameEngineAttemptCorrect()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var secretWord = literalDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: length,
            maxLength: length,
            hardMode: false,
            dailyWordWhenComputer: true,
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
        VerifyTestAttempt(knownSecretWord: secretWord,
            attemptDetails: attempt);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
    }

    [TestMethod]
    public void TestGameEngineTooManyAttempts()
    {
        var rnd = new Random();
        var length = rnd.Next(minValue: 3,
            maxValue: 5);
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var secretWord = literalDictionary.GetRandomWord(minLength: length,
            maxLength: length);
        var incorrectWord = secretWord;
        while (incorrectWord.Equals(value: secretWord))
            incorrectWord = literalDictionary.GetRandomWord(minLength: length,
                maxLength: length);
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: length,
            maxLength: length,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: secretWord);
        Assert.AreEqual(
            expected: length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        for (var i = 0;
             i < GameEngineInstance.GetMaxAttemptsForLength(
                 length: length);
             i++)
        {
            GameEnginePlayer.AttemptComputerSolve(
                mastermind: mastermind,
                turns: 1,
                maximumDictionaryLookupAttemptsPerTry: GameEnginePlayer.DefaultTries,
                noStrategy: false,
                avoidSecretWord: true);
            var attempt = mastermind.Attempts.Last();
            VerifyTestAttempt(knownSecretWord: secretWord,
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
            => GameEnginePlayer.AttemptComputerSolve(
                mastermind: mastermind,
                turns: 1));
        Assert.IsFalse(condition: thrownGuessException.Solved);
        Assert.AreEqual(
            expected: GameOverException.GameOverText,
            actual: thrownGuessException.Message);
    }

    [TestMethod]
    public void TestGameEngineWithProvidedRandomWordAndInvalidAttempt()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            dailyWordWhenComputer: true);
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
        // a word of all z's is extremely unlikely to be a valid word in any dictionary
        var invalidSecretWord = string.Empty.PadLeft(
            totalWidth: Constants.StandardLength,
            paddingChar: 'z');
        var thrownAssertion = Assert.ThrowsException<NotInDictionaryException>(action: () =>
            mastermind.MakeAttempt(wordAttempt: invalidSecretWord));
        Assert.AreEqual(
            expected: NotInDictionaryException.MessageText,
            actual: thrownAssertion.Message);
    }

    [TestMethod]
    public void TestGameEngineHardModeCorrect()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        const string expectedWord = "while";
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: true,
            dailyWordWhenComputer: true,
            secretWord: expectedWord);
        Assert.AreEqual(
            expected: expectedWord.Length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: true,
            actual: mastermind.HardMode);
        // a first attempt of where should lock in three letters, 'w', 'h', and the final 'e'
        var attempt = mastermind.MakeAttempt(wordAttempt: "where");
        VerifyTestAttempt(knownSecretWord: mastermind.SecretWord,
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
    public void TestGameEngineHardModePresent()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        const string expectedWord = "while";
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: true,
            dailyWordWhenComputer: true,
            secretWord: expectedWord);
        Assert.AreEqual(
            expected: expectedWord.Length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: true,
            actual: mastermind.HardMode);
        // a first attempt of 'liars' should mark the 'i' and 'l' as present
        var attempt = mastermind.MakeAttempt(wordAttempt: "liars");
        VerifyTestAttempt(knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
        // this should throw an exception because we've not guessed the 'i' and 'l' this attempt
        // will throw on the I as I comes before L in the alphabet and the internal list is sorted
        var thrownException =
            Assert.ThrowsException<HardModeException>(action: () => mastermind.MakeAttempt(wordAttempt: "doors"));
        Assert.AreEqual(
            expected:
            "Guess must contain I",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestAttemptsToString()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: Constants.StandardLength,
            maxLength: Constants.StandardLength,
            hardMode: false,
            dailyWordWhenComputer: true);
        Assert.AreEqual(
            expected: Constants.StandardLength,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        GameEnginePlayer.AttemptComputerSolve(mastermind: mastermind,
            turns: mastermind.MaxAttempts - 1);
        Assert.AreEqual(
            expected: mastermind.CurrentAttempt,
            actual: mastermind.Attempts.Count());
    }

    [TestMethod]
    public void TestSolvedLetters()
    {
        const string expectedWord = "HELLO";
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: expectedWord);
        Assert.AreEqual(
            expected: expectedWord.Length,
            actual: mastermind.WordLength);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);
        // a first guess of "weigh" should register "e" as a solved letter, regardless of hardmode, and "h" as a solved letter
        var attempt = mastermind.MakeAttempt(wordAttempt: "WEIGH");
        VerifyTestAttempt(
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
        VerifyTestAttempt(
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
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        foreach (var hardMode in new[] {false, true})
            for (var length = literalDictionary.ShortestWordLength;
                 length <= literalDictionary.LongestWordLength;
                 length++)
            {
                var attemptsForLength = GameEngineInstance.GetMaxAttemptsForLength(
                    length: length);
                Assert.AreEqual(
                    expected: length + 1,
                    actual: attemptsForLength);
            }
    }

    [TestMethod]
    public void TestSecretWordWithoutIsDebug()
    {
        UnitTestDetector.ForceTestMode(value: false);
        const string expectedWord = "HELLO";
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: expectedWord.Length,
            maxLength: expectedWord.Length,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: expectedWord);
        Assert.AreEqual(
            expected: string.Empty,
            actual: mastermind.SecretWord);
        mastermind.MakeAttempt(wordAttempt: expectedWord);
        Assert.AreEqual(
            expected: expectedWord,
            actual: mastermind.SecretWord);
    }

    [TestMethod]
    public void TestDailywordFalse()
    {
        var collinsSource = LiteralDictionarySource.ScrabbleDictionarySource;
        var dictionaryMock = new Mock<LiteralDictionary>(
            TestHelpers.GetWordDictionaryDictFromTestRoot(
                source: collinsSource),
            collinsSource.SourceType,
            collinsSource.Description
        )
        {
            CallBase = true,
        };
        var randomLength = dictionaryMock.Object.RandomLength();

        dictionaryMock
            .Setup(expression: d
                => d.GetRandomWord(
                    It.Is<int>(l => l.Equals(randomLength)),
                    It.Is<int>(l => l.Equals(randomLength)))).CallBase();

        var mastermind = new GameEngineInstance(
            literalDictionary: dictionaryMock.Object,
            minLength: randomLength,
            maxLength: randomLength,
            hardMode: false,
            dailyWordWhenComputer: false,
            secretWord: null);
        dictionaryMock.VerifyAll();
        dictionaryMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void TestDailyWordTrue()
    {
        var collinsSource = LiteralDictionarySource.ScrabbleDictionarySource;
        var dictionaryMock = new Mock<LiteralDictionary>(
            TestHelpers.GetWordDictionaryDictFromTestRoot(
                source: collinsSource),
            collinsSource.SourceType,
            collinsSource.Description
        )
        {
            CallBase = true,
        };
        var randomLength = dictionaryMock.Object.RandomLength();
        var expectedWordIndex = DailyWordGenerator.WordIndexForDay(
            dictionaryDescription: collinsSource.Description,
            wordLength: randomLength,
            wordsForLength: dictionaryMock.Object.WordsForLength(length: randomLength).Count(),
            date: null);
        /*
         *         return dictionary.WordAtIndex(
            length: length,
            wordIndex: WordIndexForDay(
                dictionaryDescription: dictionary.Description,
                wordLength: length,
                wordsForLength: dictionary.WordCountForLength(
                    length: length),
                date: date));
         */
        dictionaryMock
            .Setup(expression: d
                => d.WordAtIndex(
                    It.Is<int>(length => length.Equals(randomLength)),
                    It.Is<int>(index => index.Equals(expectedWordIndex)))).CallBase();

        var mastermind = new GameEngineInstance(
            literalDictionary: dictionaryMock.Object,
            minLength: randomLength,
            maxLength: randomLength,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: null);
        dictionaryMock.VerifyAll();
        dictionaryMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void TestGameEngineHardModeChangeException()
    {
        var literalDictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: literalDictionary,
            minLength: -1,
            maxLength: -1,
            hardMode: false);
        Assert.AreEqual(
            expected: false,
            actual: mastermind.HardMode);

        // set hard mode before first attempt
        mastermind.HardMode = true;

        // make the first attempt
        var attempt = mastermind.MakeAttempt(wordAttempt: "where");
        VerifyTestAttempt(knownSecretWord: mastermind.SecretWord,
            attemptDetails: attempt);

        // this should throw an exception because we tried to change hard mode after making an attempt
        var thrownException =
            Assert.ThrowsException<HardModeLockedException>(action: () => mastermind.HardMode = false);
        Assert.AreEqual(
            expected: HardModeLockedException.MessageText,
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestMaximumFlaggedLetters()
    {
        var expectedWord = "elegy";
        var dictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: dictionary,
            minLength: -1,
            maxLength: -1,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: expectedWord);
        var attempt = mastermind.MakeAttempt(wordAttempt: "eerie");
        // we should only end up with one solved and one present letter
        Assert.AreEqual(
            expected: LetterEvaluation.Correct,
            actual: attempt.Details.ElementAt(index: 0).Evaluation);
        Assert.AreEqual(
            expected: LetterEvaluation.Present,
            actual: attempt.Details.ElementAt(index: 1).Evaluation);
        for (var i = 2; i < mastermind.WordLength; i++)
            Assert.AreEqual(
                expected: LetterEvaluation.Absent,
                actual: attempt.Details.ElementAt(index: i).Evaluation);
    }

    [TestMethod]
    public void TestMaximumFlaggedLettersV2()
    {
        var expectedWord = "abbey";
        var dictionary = TestHelpers.GetWordDictionaryFromTestRoot();
        var mastermind = new GameEngineInstance(
            literalDictionary: dictionary,
            minLength: -1,
            maxLength: -1,
            hardMode: false,
            dailyWordWhenComputer: true,
            secretWord: expectedWord);

        var attempt = mastermind.MakeAttempt(wordAttempt: "algae");
        Assert.AreEqual(
            expected: LetterEvaluation.Correct,
            actual: attempt.Details.ElementAt(index: 0).Evaluation);
        Assert.AreEqual(
            expected: LetterEvaluation.Present,
            actual: attempt.Details.ElementAt(index: 4).Evaluation);
        for (var i = 2; i < 4; i++)
            Assert.AreEqual(
                expected: LetterEvaluation.Absent,
                actual: attempt.Details.ElementAt(index: i).Evaluation);

        var attempt2 = mastermind.MakeAttempt(wordAttempt: "keeps");
        Assert.AreEqual(
            expected: LetterEvaluation.Present,
            actual: attempt2.Details.ElementAt(index: 1).Evaluation);
        for (var i = 2; i < mastermind.WordLength; i++)
            Assert.AreEqual(
                expected: LetterEvaluation.Absent,
                actual: attempt2.Details.ElementAt(index: i).Evaluation);
    }
}