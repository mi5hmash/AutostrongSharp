using System.IO.Compression;
using System.Security.Cryptography;

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
    /// Encryption Spell.
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="magic"></param>
    /// <returns></returns>
    public static string Encrypto(this string inputString, string magic)
    {
        var entryData = inputString.FromUtf8String();
        var checksum = MD5.HashData(entryData);
        using MemoryStream ms = new();
        ms.Write(magic.FromAsciiString().SimpleDeEncryption(checksum));
        using MemoryStream ms2 = new();
        ms2.Write(entryData.GzipCompress().SimpleDeEncryption(ms.ToArray()));
        ms2.Write(checksum);
        return ms2.ToArray().Base64Encode();
    }

    /// <summary>
    /// Decryption Spell.
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="magic"></param>
    /// <returns></returns>
    public static string Decrypto(this string inputString, string magic)
    {
        var entryData = inputString.Base64Decode();
        using MemoryStream ms = new();
        ms.Write(magic.FromAsciiString().SimpleDeEncryption(entryData.TakeLast(16).ToArray()));
        using MemoryStream ms2 = new();
        ms2.Write(entryData, 0, entryData.Length - 16);
        return ms2.ToArray().SimpleDeEncryption(ms.ToArray()).GzipDecompress().ToUtf8String();
    }

    /// <summary>
    /// Compresses <see cref="bytes"/> with GZip.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GzipCompress(this byte[] bytes)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.Optimal))
        {
            gz.Write(bytes, 0, bytes.Length);
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Decompresses <see cref="bytes"/> with GZip.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GzipDecompress(this byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var ms2 = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionMode.Decompress))
        {
            gz.CopyTo(ms2);
        }
        return ms2.ToArray();
    }

    /// <summary>
    /// Compresses <see cref="bytes"/> with Brotli.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] BrotliCompress(this byte[] bytes)
    {
        using var ms = new MemoryStream();
        using (var bs = new BrotliStream(ms, CompressionLevel.Optimal))
        {
            bs.Write(bytes, 0, bytes.Length);
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Decompresses <see cref="bytes"/> with Brotli.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] BrotliDecompress(this byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var ms2 = new MemoryStream();
        using (var bs = new BrotliStream(ms, CompressionMode.Decompress))
        {
            bs.CopyTo(ms2);
        }
        return ms2.ToArray();
    }
}