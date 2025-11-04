using System.Runtime.InteropServices;
using AutostrongSharpCore.Helpers;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

/// <summary>
/// Represents a file that uses the AutoStrong encryption format, providing access to its headers, data, signature, and encryption operations.
/// </summary>
/// <param name="deencryptor">The AutoStrong decryption service used to decrypt and encrypt file data.</param>
public class AutoStrongFile(AutoStrongDeencryptor deencryptor)
{
    /// <summary>
    /// File extension of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public const string FileExtension = ".bin";

    /// <summary>
    /// Gets the AutoStrong deencryption service used to deencrypt data.
    /// </summary>
    public AutoStrongDeencryptor Deencryptor { get; } = deencryptor;

    /// <summary>
    /// Header of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public AutoStrongHeader Header { get; set; } = new();

    /// <summary>
    /// DataHeader of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public AutoStrongDataHeader DataHeader { get; set; } = new();

    /// <summary>
    /// Data of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public uint[] Data { get; set; } = [];

    /// <summary>
    /// A signature of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public uint Signature { get; set; } = 0x4835_494D;

    /// <summary>
    /// Stores the encryption state of the current file.
    /// </summary>
    public bool IsEncrypted { get; private set; }

    /// <summary>
    /// Parses and sets the file data from the specified byte span, updating the header, data header, main data, and signature fields.
    /// </summary>
    /// <param name="data">A read-only span of bytes containing the file data to be parsed and set.</param>
    /// <param name="encryptedFilesOnly">If <see langword="true"/>, only encrypted files will be processed; otherwise, both encrypted and unencrypted files are accepted.</param>
    /// <exception cref="InvalidDataException">Thrown when the provided data is invalid or cannot be parsed into the expected file structure.</exception>
    public void SetFileData(ReadOnlySpan<byte> data, bool encryptedFilesOnly = false)
    {
        IsEncrypted = false;
        var dataAsInts = MemoryMarshal.Cast<byte, uint>(data);
        // HEADER
        // try to load header data into the Header
        try { Header.SetData(dataAsInts); }
        catch (Exception e) { throw new InvalidDataException(e.Message); }
        // DATA HEADER
        // try to load data header data into the DataHeader
        var position = AutoStrongHeader.Size / sizeof(uint);
        try { DataHeader.SetData(dataAsInts.Slice(position, AutoStrongDataHeader.Size / sizeof(uint))); }
        catch (Exception e) { throw new InvalidDataException(e.Message); }
        // check if file is encrypted
        IsEncrypted = DataHeader.IsEncrypted();
        if (encryptedFilesOnly && !IsEncrypted) return;
        // DATA
        position += AutoStrongDataHeader.Size / sizeof(uint);
        Data = dataAsInts.Slice(position, dataAsInts.Length - (position + Marshal.SizeOf(Signature) / sizeof(uint))).ToArray();
        // FOOTER
        Signature = dataAsInts[^1];
    }

    /// <summary>
    /// Generates and returns the complete file data, including headers, content, and signature, in binary format.
    /// </summary>
    /// <returns>A byte array containing the assembled file data.</returns>
    public byte[] GetFileData()
    {
        // prepare memory stream and binary writer
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        // write DSSS HEADER content
        var data = MemoryMarshal.Cast<uint, byte>(Header.GetDataAsSpan());
        bw.Write(data);
        // write DSSS DATA HEADER content
        data = MemoryMarshal.Cast<uint, byte>(DataHeader.GetDataAsSpan());
        bw.Write(data);
        // write DSSS DATA content
        data = MemoryMarshal.Cast<uint, byte>(Data.AsSpan());
        bw.Write(data);
        // write DSSS FOOTER content
        bw.Write(Signature);

        var dataAsBytes = ms.ToArray();
        var dataSpan = dataAsBytes.AsSpan();

        // sign file
        SignFile(ref dataSpan);

        // return data
        return dataAsBytes;
    }
    
    /// <summary>
    /// Decrypts the file data and updates the encryption state to reflect the current status.
    /// </summary>
    public void DecryptFile()
    {
        // Decrypt DataHeader
        DataHeader.Decrypt(Deencryptor);
        // Decrypt Data
        Deencryptor.DecryptData(Data);
        // update encryption state
        IsEncrypted = DataHeader.IsEncrypted();
    }

    /// <summary>
    /// Encrypts the file data and updates the encryption state.
    /// </summary>
    public void EncryptFile()
    {
        // Encrypt DataHeader
        DataHeader.Encrypt(Deencryptor);
        // Encrypt Data
        Deencryptor.EncryptData(Data);
        // update encryption state
        IsEncrypted = DataHeader.IsEncrypted();
    }

    /// <summary>
    /// Computes a 32-bit Murmur3 hash for the specified sequence of unsigned integers.
    /// </summary>
    /// <param name="data">The input data to hash, represented as a read-only span of 32-bit unsigned integers.</param>
    /// <param name="seed">An optional seed value to initialize the hash computation. Using different seeds produces different hash results for the same input data.</param>
    /// <returns>A 32-bit unsigned integer containing the computed Murmur3 hash of the input data.</returns>
    private static uint Murmur3_32(ReadOnlySpan<uint> data, uint seed = 0)
    {
        const uint hash0 = 0x1B873593;
        const uint hash1 = 0xCC9E2D51;
        const uint hash2 = 0x052250EC;
        const uint hash3 = 0xC2B2AE35;
        const uint hash4 = 0x85EBCA6B;

        const byte rotation1 = 0xD;
        const byte rotation2 = 0xF;
        const byte shift1 = 0x10;

        var lengthInBytes = data.Length * sizeof(uint);

        foreach (var e in data)
            seed = 5 * (uint.RotateLeft((hash0 * uint.RotateLeft(hash1 * e, rotation2)) ^ seed, rotation1) - hash2);

        uint mod0 = 0;
        switch (lengthInBytes & 3)
        {
            case 3:
                mod0 = data[2] << shift1;
                goto case 2;
            case 2:
                mod0 ^= data[1] << 8;
                goto case 1;
            case 1:
                seed ^= hash0 * uint.RotateLeft(hash1 * (mod0 ^ data[0]), rotation2);
                break;
        }

        var basis = (uint)(lengthInBytes ^ seed);
        var hiWordOfBasis = (basis >> shift1) & 0xFFFF;

        return (hash3 * ((hash4 * (basis ^ hiWordOfBasis)) ^ ((hash4 * (basis ^ hiWordOfBasis)) >> rotation1))) ^ ((hash3 * ((hash4 * (basis ^ hiWordOfBasis)) ^ ((hash4 * (basis ^ hiWordOfBasis)) >> rotation1))) >> shift1);
    }

    /// <summary>
    /// Calculates and writes a Murmur3 hash signature to the end of the specified file data buffer.
    /// </summary>
    /// <remarks>Thanks to windwakr (https://github.com/windwakr) for identifying this hashing method as MurmurHash3_32.</remarks>
    /// <typeparam name="T">The value type of each element in the file data buffer.</typeparam>
    /// <param name="fileData">A span representing the file data to be signed. The signature will be written to the last element of this span.</param>
    private static void SignFile<T>(ref Span<T> fileData) where T : struct
    {
        var span = MemoryMarshal.Cast<T, uint>(fileData);
        span[^1] = Murmur3_32(span[..^1], 0xFFFFFFFF);
    }
}