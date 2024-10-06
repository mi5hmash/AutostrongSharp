using AutostrongSharpCore.Helpers;
using System.Runtime.InteropServices;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

public class AutoStrongFile(AutoStrongDeencryptor deencryptor)
{
    /// <summary>
    /// Header of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public AutoStrongHeader AutoStrongHeader { get; set; } = new();

    /// <summary>
    /// DataHeader of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public AutoStrongDataHeader DataHeader { get; set; } = new();

    /// <summary>
    /// Data of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public uint[] Data { get; set; } = [];

    /// <summary>
    /// Footer of the <see cref="AutoStrongFile"/>.
    /// </summary>
    public AutoStrongFooter AutoStrongFooter { get; set; } = new();

    /// <summary>
    /// Deencryptor instance.
    /// </summary>
    public AutoStrongDeencryptor Deencryptor { get; } = deencryptor;
    
    /// <summary>
    /// Loads a '*.bin' archive of <see cref="AutoStrongFile"/> type into the existing object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public BoolResult SetFileData(string filePath)
    {
        FileStream fs;
        try { fs = File.OpenRead(filePath); }
        catch { return new BoolResult(false, "Couldn't load the file. Error on trying to open the file."); }

        using BinReader br = new(fs);
        try
        {
            // try to load header data into the AutoStrongHeader
            AutoStrongHeader = br.ReadStruct<AutoStrongHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file header structure."); }

        var test = AutoStrongHeader.CheckIntegrity();
        if (!test.Result) return new BoolResult(test.Result, $"Couldn't load the file. {test.Description}");

        try
        {
            // try to load data header data into the DataHeader
            DataHeader = br.ReadStruct<AutoStrongDataHeader>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file data header structure."); }

        test = AutoStrongHeader.CheckIntegrity();
        if (!test.Result) return new BoolResult(test.Result, $"Couldn't load the file. {test.Description}");

        var dataLength = fs.Length - (Marshal.SizeOf<AutoStrongHeader>() + Marshal.SizeOf<AutoStrongDataHeader>() + Marshal.SizeOf<AutoStrongFooter>());
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
            AutoStrongFooter = br.ReadStruct<AutoStrongFooter>() ?? throw new NullReferenceException();
        }
        catch { return new BoolResult(false, "Couldn't load the file. Invalid file footer structure."); }

        return new BoolResult(true);
    }

    /// <summary>
    /// Get an existing object of a <see cref="AutoStrongFile"/> type as byte array.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> GetFileData()
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
        var dataAsInts = MemoryMarshal.Cast<byte, uint>(dataAsBytes);

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
    /// Thanks to windwakr (https://github.com/windwakr) for identifying this hashing method as MurmurHash3_32.
    /// </summary>
    /// <param name="fileData"></param>
    private static void SignFile(ref Span<uint> fileData)
        => fileData[^1] = SimpleDeencryptor.Murmur3_32(fileData[..^1], 0xFFFFFFFF);
}