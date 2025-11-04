using AutostrongSharpCore.Helpers;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

public class AutoStrongDataHeader
{
    public const int Size = 0x20;
    private const uint Dsss = 0x5353_5344;

    /// <summary>
    /// Readonly buffer.
    /// </summary>
    private readonly uint[] _data = [Dsss, Dsss, 0x0000_0001, 0, 0x0511_4CB5, 0x0000_0035, 0xF627_F2FD, 0xE15F_B3A5];

    /// <summary>
    /// This should be the ANSI for "DSSS", or 0x5353_5344.
    /// </summary>
    public uint FileFormat1
    {
        get => _data[0];
        set => _data[0] = value;
    }

    /// <summary>
    /// Another one. This also should be the ANSI for "DSSS", or 0x5353_5344.
    /// </summary>
    public uint FileFormat2
    {
        get => _data[1];
        set => _data[1] = value;
    }

    /// <summary>
    /// Holds User ID.
    /// </summary>
    public uint UserId
    {
        get => _data[2];
        set => _data[2] = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown1
    {
        get => _data[3];
        set => _data[3] = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown2
    {
        get => _data[4];
        set => _data[4] = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown3
    {
        get => _data[5];
        set => _data[5] = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown4
    {
        get => _data[6];
        set => _data[6] = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown5
    {
        get => _data[7];
        set => _data[7] = value;
    }
    
    /// <summary>
    /// Copies the specified sequence of 32-bit unsigned integers into the internal data buffer.
    /// </summary>
    /// <param name="data">A read-only span of 32-bit unsigned integers containing the data to copy. The number of elements copied is limited to the buffer size.</param>
    public void SetData(ReadOnlySpan<uint> data) 
        => data[..(Size / sizeof(uint))].CopyTo(_data);

    /// <summary>
    /// Returns the array of unsigned integer data currently held by the instance.
    /// </summary>
    /// <returns>An array of <see cref="uint"/> values representing the current data.</returns>
    public uint[] GetData()
        => _data;

    /// <summary>
    /// Returns a read-only span over the underlying sequence of unsigned integer data.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{uint}"/> representing the current data. The span reflects the contents at the time of the call and is valid only while the underlying data remains unchanged.</returns>
    public ReadOnlySpan<uint> GetDataAsSpan()
        => _data.AsSpan();

    /// <summary>
    /// Decrypts <see cref="AutoStrongDataHeader"/>.
    /// </summary>
    /// <param name="deencryptor">The deencryptor instance used to perform the decryption.</param>
    public void Decrypt(AutoStrongDeencryptor deencryptor)
        => deencryptor.DecryptHeader(_data);

    /// <summary>
    /// Encrypts <see cref="AutoStrongDataHeader"/>.
    /// </summary>
    /// <param name="deencryptor">The deencryptor instance used to perform the encryption.</param>
    public void Encrypt(AutoStrongDeencryptor deencryptor)
        => deencryptor.EncryptHeader(_data);

    /// <summary>
    /// Returns true if file is Encrypted.
    /// </summary>
    /// <returns></returns>
    public bool IsEncrypted() => FileFormat1 != Dsss || FileFormat2 != Dsss;

    /// <summary>
    /// Retrieves the user identifier, optionally decrypting it using the provided deencryptor.
    /// </summary>
    /// <param name="deencryptor">The deencryptor instance used to perform the deencryption.</param>
    /// <returns>The user identifier as an unsigned integer. If the data is not encrypted, returns the stored user identifier; otherwise, returns the decrypted value.</returns>
    public uint GetUserId(AutoStrongDeencryptor deencryptor)
    {
        if (!IsEncrypted()) return UserId;
        var dataSpan = _data.AsSpan(2, 2);
        deencryptor.DecryptHeader(dataSpan);
        return dataSpan[0];
    }

    /// <summary>
    /// Sets the user identifier, updating the encrypted or unencrypted value as appropriate.
    /// </summary>
    /// <param name="newUserId">The new user identifier to assign. This value will be stored in encrypted form if encryption is enabled.</param>
    /// <param name="deencryptor">The deencryptor instance used to perform the deencryption.</param>
    public void SetUserId(uint newUserId, AutoStrongDeencryptor deencryptor)
    {
        if (!IsEncrypted())
        {
            UserId = newUserId;
            return;
        }
        var dataSpan = _data.AsSpan(2,2);
        deencryptor.DecryptHeader(dataSpan);
        dataSpan[0] = newUserId;
        deencryptor.EncryptHeader(dataSpan);
    }

    /// <summary>
    /// Verifies the integrity of the encrypted file using the provided deencryptor.</summary>
    /// /// <param name="deencryptor">The deencryptor instance used to perform the decryption.</param>
    /// <exception cref="InvalidDataException">Thrown when the file is not compatible with the selected game profile after decryption.</exception>
    public void CheckIntegrity(AutoStrongDeencryptor deencryptor)
    {
        if (!IsEncrypted()) return;
        Span<uint> dataSpan = [FileFormat1, FileFormat2];
        deencryptor.DecryptHeader(dataSpan);
        if (dataSpan[0] != Dsss || dataSpan[1] != Dsss)
            throw new InvalidDataException("File is not compatible with the selected game profile.");
    }
}