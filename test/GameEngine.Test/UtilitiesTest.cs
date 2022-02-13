using System;
using System.Collections.Generic;
using GameEngine.Library.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEngine.Test;

[TestClass]
public class UtilitiesTest
{
    /// <summary>
    ///     HumanizeIndex just turns 0 offset based numbering to 1 based numbering
    /// </summary>
    [TestMethod]
    public void TestHumanizeIndex()
    {
        var randomInt = new Random().Next(minValue: 1,
            maxValue: 1000);
        var result = Utilities.HumanizeIndex(index: randomInt);
        Assert.AreEqual(
            expected: randomInt + 1,
            actual: result);
    }

    [TestMethod]
    public void TestNumberToOrdinal()
    {
        var expectedResultDict = new Dictionary<int, string>
        {
            {0, "0th"},
            {1, "1st"},
            {2, "2nd"},
            {3, "3rd"},
            {4, "4th"},
            {5, "5th"},
            {6, "6th"},
            {7, "7th"},
            {8, "8th"},
            {9, "9th"},
            {10, "10th"},
            {11, "11th"},
            {12, "12th"},
            {13, "13th"},
            {14, "14th"},
            {15, "15th"},
            {16, "16th"},
            {17, "17th"},
            {18, "18th"},
            {19, "19th"},
            {20, "20th"},
            {21, "21st"},
            {22, "22nd"},
            {23, "23rd"},
            {24, "24th"},
            {25, "25th"},
            {26, "26th"},
            {27, "27th"},
            {28, "28th"},
            {29, "29th"},
            {30, "30th"},
            {31, "31st"},
            {32, "32nd"},
            {33, "33rd"},
            {34, "34th"},
            {35, "35th"},
            {100, "100th"},
            {150, "150th"},
            {1000, "1,000th"},
            {1000000, "1,000,000th"},
            {1000001, "1,000,001st"},
        };
        foreach (var (key, value) in expectedResultDict)
        {
            var result = Utilities.NumberToOrdinal(number: key);
            Assert.AreEqual(
                expected: value,
                actual: result);
        }
    }
}