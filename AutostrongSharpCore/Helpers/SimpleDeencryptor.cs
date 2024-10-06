// v2024-10-05 21:16:48

using System.Runtime.InteropServices;

namespace AutostrongSharpCore.Helpers;

public static class SimpleDeencryptor
{
    /// <summary>
    /// DeEncrypts input byte array.
    /// </summary>
    /// <param name="bytesToDeEncrypt"></param>
    /// <param name="magicBytes"></param>
    /// <returns></returns>
    private static byte[] SimpleDeEncryption(this byte[] bytesToDeEncrypt, IReadOnlyList<byte> magicBytes)
    {
        var spellLength = magicBytes.Count;
        for (var i = 0; i < bytesToDeEncrypt.Length; i++)
        {
            bytesToDeEncrypt[i] ^= magicBytes[i % spellLength];
        }
        return bytesToDeEncrypt;
    }

    /// <summary>
    /// Encryption spell.
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="magic"></param>
    /// <param name="murMurSeed"></param>
    /// <returns></returns>
    public static string Encrypto(this string inputString, string magic, uint murMurSeed = 0)
    {
        var entryData = inputString.FromUtf8String();
        var checksum = BitConverter.GetBytes(Murmur3_32(MemoryMarshal.Cast<byte, uint>(entryData), murMurSeed));
        using MemoryStream msi = new();
        msi.Write(magic.FromAsciiString().SimpleDeEncryption(checksum));
        using MemoryStream mso = new();
        mso.Write(entryData.GzipCompress().SimpleDeEncryption(msi.ToArray()));
        mso.Write(checksum);
        return mso.ToArray().Base64Encode();
    }

    /// <summary>
    /// Decryption spell.
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="magic"></param>
    /// <returns></returns>
    public static string Decrypto(this string inputString, string magic)
    {
        var entryData = inputString.Base64Decode();
        using MemoryStream ms = new();
        ms.Write(magic.FromAsciiString().SimpleDeEncryption(entryData.TakeLast(sizeof(uint)).ToArray()));
        using MemoryStream ms2 = new();
        ms2.Write(entryData, 0, entryData.Length - sizeof(uint));
        return ms2.ToArray().SimpleDeEncryption(ms.ToArray()).GzipDecompress().ToUtf8String();
    }

    /// <summary>
    /// Calculates MurmurHash3.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static uint Murmur3_32(ReadOnlySpan<uint> data, uint seed = 0)
    {
        const uint hash0 = 0x1B873593;
        const uint hash1 = 0xCC9E2D51;
        const uint hash2 = 0x052250EC;
        const uint hash3 = 0xC2B2AE35;
        const uint hash4 = 0x85EBCA6B;

        var lengthInBytes = data.Length * 4;

        foreach (var e in data)
            seed = 5 * (uint.RotateLeft((hash0 * uint.RotateLeft(hash1 * e, 15)) ^ seed, 13) - hash2);

        uint mod0 = 0;
        switch (lengthInBytes & 3)
        {
            case 3:
                mod0 = data[2] << 16;
                goto case 2;
            case 2:
                mod0 ^= data[1] << 8;
                goto case 1;
            case 1:
                seed ^= hash0 * uint.RotateLeft(hash1 * (mod0 ^ data[0]), 15);
                break;
        }

        var basis = (uint)(lengthInBytes ^ seed);
        var hiWordOfBasis = (basis >> 16) & 0xFFFF;
        var lv1 = hash4 * (basis ^ hiWordOfBasis);
        var lv2 = hash3 * (lv1 ^ (lv1 >> 13));
        lv2 ^= lv2 >> 16;

        return lv2;
    }
}