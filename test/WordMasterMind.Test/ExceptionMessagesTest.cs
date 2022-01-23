using System;
using Bogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Exceptions;

namespace WordMasterMind;

[TestClass]
public class ExceptionMessagesTest
{
    [TestMethod]
    public void DebugModeExceptionTest()
    {
        var paramName = new Faker().Random.String();
        var thrownException =
            Assert.ThrowsException<DebugModeException>(action: ()
                => throw new DebugModeException(paramName: paramName));
        Assert.AreEqual(
            expected: "Only available in debug and testing mode.",
            actual: thrownException.Message);
        Assert.AreEqual(
            expected: paramName,
            actual: thrownException.ParamName);
    }

    [TestMethod]
    public void GameOverExceptionTest()
    {
        var thrownException =
            Assert.ThrowsException<GameOverException>(action: () => throw new GameOverException(solved: true));
        Assert.AreEqual(
            expected: "You have already solved this word!",
            actual: thrownException.Message);

        thrownException =
            Assert.ThrowsException<GameOverException>(action: () => throw new GameOverException(solved: false));
        Assert.AreEqual(
            expected: "Game Over: You have reached the maximum number of attempts.",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void TestInvalidAttemptLengthException()
    {
        var thrownException =
            Assert.ThrowsException<InvalidAttemptLengthException>(action: ()
                => throw new InvalidAttemptLengthException());
        Assert.AreEqual(
            expected: "Word length does not match secret word length.",
            actual: thrownException.Message);
    }

    [TestMethod]
    public void InvalidLengthExceptionTest()
    {
        var rnd = new Random();
        var minLength = rnd.Next(minValue: 3,
            maxValue: 5);
        var maxLength = rnd.Next(minValue: minLength,
            maxValue: 15);
        var thrownException = Assert.ThrowsException<InvalidLengthException>(action: ()
            => throw new InvalidLengthException(minLength: minLength,
                maxLength: maxLength));
        Assert.AreEqual(
            expected: "word must be between minLength and maxLength.",
            actual: thrownException.Message);
        Assert.AreEqual(
            expected: minLength,
            actual: thrownException.MinLength);
        Assert.AreEqual(
            expected: maxLength,
            actual: thrownException.MaxLength);
    }

    [TestMethod]
    public void TestNotInDictionaryException()
    {
        var thrownException =
            Assert.ThrowsException<NotInDictionaryException>(action: () => throw new NotInDictionaryException());
        Assert.AreEqual(
            expected: "not a valid word in the Scrabble dictionary.",
            actual: thrownException.Message);
    }
}