using AutostrongSharpCore.Helpers;
using System.Runtime.InteropServices;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

public class DsssAutoStrongFile
{
    /// <summary>
    /// Header of the <see cref="DsssAutoStrongFile"/>.
    /// </summary>
    public DsssAutoStrongHeader AutoStrongHeader { get; set; } = new();

    /// <summary>
    /// DataHeader of the <see cref="DsssAutoStrongFile"/>.
    /// </summary>
    public DsssAutoStrongDataHeader DataHeader { get; set; } = new();

    /// <summary>
    /// Data of the <see cref="DsssAutoStrongFile"/>.
    /// </summary>
    public uint[] Data { get; set; } = [];

    /// <summary>
    /// Footer of the <see cref="DsssAutoStrongFile"/>.
    /// </summary>
    public DsssAutoStrongFooter AutoStrongFooter { get; set; } = new();
    
    /// <summary>
    /// Hashes needed to calculate the file checksum.
    /// </summary>
    private static uint[] Hashes { get; set; } = [];

    /// <summary>
    /// Deencryptor instance.
    /// </summary>
    public AutoStrongDeencryptor Deencryptor { get; set; }

    /// <summary>
    /// Creates an empty <see cref="DsssAutoStrongFile"/> class.
    /// </summary>
    /// <param name="deencryptor"></param>
    public DsssAutoStrongFile(AutoStrongDeencryptor deencryptor)
    {
        Deencryptor = deencryptor;
        Hashes = "OTMzNTg3MUI1MTJEOUVDQ0VDNTAyMjA1MzVBRUIyQzI2QkNBRUI4NQ==".Base64DecodeUtf8().ToUintArray();
    }

    /// <summary>
    /// Loads a '*.bin' archive of <see cref="DsssAutoStrongFile"/> type into the existing object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public BoolResult SetFileData(string filePath)
    {
        FileStream fs;
        try
        {
            fs = File.OpenRead(filePath);
        }
        catch { return new BoolResult(false, "Couldn't load the file. Error on trying to open the file."); }

        using BinReader br = new(fs);
        try
        {
            // try to load header data into the AutoStrongHeader
            AutoStrongHeader = br.ReadStruct<DsssAutoStrongHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file header structure."); }

        var test = AutoStrongHeader.CheckIntegrity();
        if (!test.Result) return new BoolResult(test.Result, $"Couldn't load the file. {test.Description}");

        try
        {
            // try to load data header data into the DataHeader
            DataHeader = br.ReadStruct<DsssAutoStrongDataHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file data header structure."); }

        test = AutoStrongHeader.CheckIntegrity();
        if (!test.Result) return new BoolResult(test.Result, $"Couldn't load the file. {test.Description}");

        var dataLength = fs.Length - (Marshal.SizeOf<DsssAutoStrongHeader>() + Marshal.SizeOf<DsssAutoStrongDataHeader>() + Marshal.SizeOf<DsssAutoStrongFooter>());
        var dataSize = dataLength / Marshal.SizeOf<uint>();

        // load data
        Data = new uint[dataSize];
        for (var i = 0; i < dataSize; i++)
        {
            Data[i] = br.ReadUInt32();
        }

        try
        {
            // try to load footer data into the AutoStrongFooter
            AutoStrongFooter = br.ReadStruct<DsssAutoStrongFooter>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file footer structure."); }

        return new BoolResult(true);
    }

    /// <summary>
    /// Get an existing object of a <see cref="DsssAutoStrongFile"/> type as byte array.
    /// </summary>
    /// <returns></returns>
    public Span<byte> GetFileData()
    {
        using MemoryStream ms = new();
        using BinWriter bw = new(ms);
        // write DSSS HEADER content
        bw.WriteStruct(AutoStrongHeader);
        // write DSSS DATA HEADER content
        bw.WriteStruct(DataHeader);
        // write DSSS DATA content
        foreach (var data in Data) bw.Write(data);
        // write DSSS FOOTER content
        bw.WriteStruct(AutoStrongFooter);

        var dataAsBytes = ms.ToArray().AsSpan();
        var dataAsInts = MemoryMarshal.Cast<byte, uint>(dataAsBytes[..(dataAsBytes.Length / sizeof(uint) * sizeof(uint))]);

        // sign file
        SignFile(ref dataAsInts);

        // return data
        return dataAsBytes;
    }

    /// <summary>
    /// Decrypts the <see cref="DataHeader"/>.
    /// </summary>
    public void DecryptDataHeader() => DataHeader.DecryptData(Deencryptor);

    /// <summary>
    /// Encrypts the <see cref="DataHeader"/>.
    /// </summary>
    public void EncryptDataHeader() => DataHeader.EncryptData(Deencryptor);

    /// <summary>
    /// Returns true if the <see cref="DataHeader"/> is Encrypted.
    /// </summary>
    /// <returns></returns>
    public bool IsEncrypted() => DataHeader.IsEncrypted();

    /// <summary>
    /// Decrypts <see cref="Data"/>.
    /// </summary>
    public void DecryptData()
    {
        Queue<uint> queue = new();
        queue.Enqueue(0);
        queue.Enqueue(0);

        for (long i = 4; i < Data.LongLength-1; i+=2)
        {
            queue.Enqueue(Data[i]);
            queue.Enqueue(Data[i + 1]);
            Deencryptor.Decrypt(ref Data[i], ref Data[i + 1]);
            Data[i] ^= queue.Dequeue();
            Data[i+1] ^= queue.Dequeue();
        }
    }

    /// <summary>
    /// Encrypts <see cref="Data"/>.
    /// </summary>
    public void EncryptData()
    {
        Queue<uint> queue = new();
        queue.Enqueue(0);
        queue.Enqueue(0);

        for (long i = 4; i < Data.LongLength - 1; i += 2)
        {
            Data[i] ^= queue.Dequeue();
            Data[i + 1] ^= queue.Dequeue();
            Deencryptor.Encrypt(ref Data[i], ref Data[i + 1]);
            queue.Enqueue(Data[i]);
            queue.Enqueue(Data[i + 1]);
        }
    }

    /// <summary>
    /// This method signs a DSSS file.
    /// </summary>
    /// <param name="fileData"></param>
    private static void SignFile(ref Span<uint> fileData)
    {
        var hash0 = 0xFFFFFFFF;
        var length = fileData.Length - 1;
        var lengthInBytes = length * 4;

        for (var i = 0; i < length; i++)
            hash0 = 5 * (uint.RotateLeft((Hashes[0] * uint.RotateLeft(Hashes[1] * fileData[i], 15)) ^ hash0, 13) - Hashes[2]);

        uint mod0 = 0;
        switch (lengthInBytes & 3)
        {
            case 1:
                hash0 ^= Hashes[0] * uint.RotateLeft(Hashes[1] * (mod0 ^ fileData[0]), 15);
                break;
            case 2:
                mod0 ^= fileData[1] << 8;
                hash0 ^= Hashes[0] * uint.RotateLeft(Hashes[1] * (mod0 ^ fileData[0]), 15);
                break;
            case 3:
                mod0 = fileData[2] << 16;
                mod0 ^= fileData[1] << 8;
                hash0 ^= Hashes[0] * uint.RotateLeft(Hashes[1] * (mod0 ^ fileData[0]), 15);
                break;
        }
        var basis = (uint)(lengthInBytes ^ hash0);
        var hiWordOfBasis = (basis >> 16) & 0xFFFF;

        fileData[^1] = (Hashes[3] * ((Hashes[4] * (basis ^ hiWordOfBasis)) ^ ((Hashes[4] * (basis ^ hiWordOfBasis)) >> 13))) ^ ((Hashes[3] * ((Hashes[4] * (basis ^ hiWordOfBasis)) ^ ((Hashes[4] * (basis ^ hiWordOfBasis)) >> 13))) >> 16);
    }
}