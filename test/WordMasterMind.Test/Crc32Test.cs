using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Test;

[TestClass]
public class Crc32Test
{
    // ReSharper disable once StringLiteralTypo
    [DataTestMethod]
    [DataRow(data1: "54686520717569636B2062726F776E20666F78206A756D7073206F76657220746865206C617A7920646F67",
        0x414FA339U)]
    [DataRow(data1: "0000000000000000000000000000000000000000000000000000000000000000",
        0x190A55ADU)]
    [DataRow(data1: "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
        0xFF6CAB0BU)]
    [DataRow(data1: "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F",
        0x91267E8AU)]
    public void TestCrc32KnownHashes(string hexString, uint expected)
    {
        var data = new byte[hexString.Length / 2];
        for (int offset = 0, dataOffset = 0; offset < hexString.Length; offset += 2)
            data[dataOffset++] = Convert.ToByte(
                value: hexString.Substring(
                    startIndex: offset,
                    length: 2),
                fromBase: 16);

        var actual = Crc32.ComputeChecksum(bytes: data);
        Assert.AreEqual(
            expected: expected,
            actual: actual);
    }

    [TestMethod]
    public void TestNullThrows()
    {
        Assert.ThrowsException<ArgumentNullException>(
            action: () => Crc32.ComputeChecksum(bytes: null));
    }
}