namespace WordMasterMind.Library.Helpers;

public static class Crc32
{
    private static uint[] Crc32Table()
    {
        const uint poly = 0xedb88320;
        var table = new uint[256];
        for (uint i = 0; i < table.Length; ++i)
        {
            var temp = i;
            for (var j = 8; j > 0; --j)
                if ((temp & 1) == 1)
                    temp = (temp >> 1) ^ poly;
                else
                    temp >>= 1;
            table[i] = temp;
        }

        return table;
    }

    public static uint ComputeChecksum(IEnumerable<byte> bytes)
    {
        var crc = 0xffffffff;
        var table = Crc32Table();
        foreach (var t in bytes)
        {
            var index = (byte) ((crc & 0xff) ^ t);
            crc = (crc >> 8) ^ table[index];
        }

        return ~crc;
    }

    public static byte[] ComputeChecksumBytes(IEnumerable<byte> bytes)
    {
        return BitConverter.GetBytes(value: ComputeChecksum(bytes: bytes));
    }
}