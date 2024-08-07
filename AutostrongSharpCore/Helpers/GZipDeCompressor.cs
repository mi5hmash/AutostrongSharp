// v2024-07-13 21:16:48

using System.IO.Compression;

namespace AutostrongSharpCore.Helpers;

public static class GZipDeCompressor
{
    /// <summary>
    /// Compresses <paramref name="bytes"/> with GZip.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GzipCompress(this byte[] bytes)
    {
        using var mso = new MemoryStream();
        using (var gz = new GZipStream(mso, CompressionLevel.Optimal))
        {
            gz.Write(bytes, 0, bytes.Length);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Asynchronously compress <paramref name="bytes"/> with GZip.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static async Task<byte[]> GZipCompressAsync(this byte[] bytes)
    {
        using var mso = new MemoryStream();
        await using (GZipStream gz = new(mso, CompressionLevel.Optimal))
        {
            gz.Write(bytes, 0, bytes.Length);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Decompresses <paramref name="bytes"/> with GZip.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] GzipDecompress(this byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gz = new GZipStream(msi, CompressionMode.Decompress))
        {
            gz.CopyTo(mso);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Asynchronously decompress <paramref name="bytes"/> compressed with GZip.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static async Task<byte[]> GZipDecompressAsync(this byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        await using (var gz = new GZipStream(msi, CompressionMode.Decompress))
        {
            await gz.CopyToAsync(mso);
        }
        return mso.ToArray();
    }
}