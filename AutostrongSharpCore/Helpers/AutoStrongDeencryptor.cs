namespace AutostrongSharpCore.Helpers;

public class AutoStrongDeencryptor
{
    /// <summary>
    /// An array of 32-bit unsigned integers representing the encryption key used for the decryption and encryption processes.
    /// </summary>
    public uint[] EncryptionKey { get; set; } = null!;

    /// <summary>
    /// An array of 32-bit unsigned integers representing the S-box lookup table. Must contain at least 1024 elements.
    /// </summary>
    public uint[] SBox { get; set; } = null!;

    /// <summary>
    /// Configures the deencryptor with the specified encryption key and substitution box (S-box) values.
    /// </summary>
    /// <param name="encryptionKey">An array of 32-bit unsigned integers representing the encryption key to use. If null, an empty key is applied.</param>
    /// <param name="sBox">An array of 32-bit unsigned integers representing the substitution box (S-box) values. If null, an empty S-box is applied.</param>
    public void Configure(uint[]? encryptionKey = null, uint[]? sBox = null)
    {
        EncryptionKey = encryptionKey ?? [];
        SBox = sBox ?? [];
    }

    public AutoStrongDeencryptor(uint[]? encryptionKey = null, uint[]? sBox = null)
        => Configure(encryptionKey, sBox);

    /// <summary>
    /// Transforms the specified 32-bit value using the provided S-box span according to a cryptographic substitution operation.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer to be substituted using the S-box.</param>
    /// <returns>A 32-bit unsigned integer result produced by applying the S-box transformation to the input value.</returns>
    private uint Sboxize(uint value) 
        => ((SBox[(int)(((value >> 16) & 0xFF) + 256)] + SBox[(int)(value >> 24)]) ^ SBox[(int)(((value >> 8) & 0xFF) + 512)]) + SBox[(int)((value & 0xFF) + 768)];

    /// <summary>
    /// Decrypts a block of data in-place using the specified encryption key.
    /// </summary>
    /// <param name="data">The span of 32-bit unsigned integers representing the data to be decrypted. The decryption is performed in-place, modifying the contents of this span.</param>
    /// <param name="encryptionKeySpan">A read-only span of 32-bit unsigned integers that provides the encryption key used for decryption. The length of this span determines the number of decryption rounds.</param>
    private void Decrypt(Span<uint> data, ReadOnlySpan<uint> encryptionKeySpan)
    {
        data[0] ^= encryptionKeySpan[^1];
        for (var i = 1; i < encryptionKeySpan.Length - 1; i++)
        {
            var t0 = Sboxize(data[0]);
            t0 ^= encryptionKeySpan[^(i + 1)] ^ data[1];
            data[1] = data[0];
            data[0] = t0;
        }
        data[1] ^= encryptionKeySpan[0];
        (data[0], data[1]) = (data[1], data[0]);
    }
    
    /// <summary>
    /// Encrypts a block of data in-place using the specified encryption key.
    /// </summary>
    /// <param name="data">The span of 32-bit unsigned integers representing the data to be encrypted. The encryption is performed in-place, modifying the contents of this span.</param>
    /// <param name="encryptionKeySpan">A read-only span of 32-bit unsigned integers that provides the encryption key used for encryption. The length of this span determines the number of decryption rounds.</param>
    private void Encrypt(Span<uint> data, ReadOnlySpan<uint> encryptionKeySpan)
    {
        (data[0], data[1]) = (data[1], data[0]);
        data[1] ^= encryptionKeySpan[0];
        for (var i = 1; i < encryptionKeySpan.Length - 1; i++)
        {
            var t0 = Sboxize(data[1]);
            t0 ^= encryptionKeySpan[i] ^ data[0];
            data[0] = data[1];
            data[1] = t0;
        }
        data[0] ^= encryptionKeySpan[^1];
    }

    /// <summary>
    /// Decrypts the specified data in place using the provided encryption key and substitution box (S-box) spans.
    /// </summary>
    /// <param name="data">A span of 32-bit unsigned integers representing the encrypted data to be decrypted.</param>
    public void DecryptHeader(Span<uint> data)
    {
        ReadOnlySpan<uint> encryptionKeySpan = EncryptionKey.AsSpan();
        
        for (var i = 0; i < data.Length - 1; i += 2)
            Decrypt(data.Slice(i, 2), encryptionKeySpan);
    }
    
    /// <summary>
    /// Decrypts the specified data in place using the provided encryption key and substitution box (S-box) spans.
    /// </summary>
    /// <param name="data">A span of 32-bit unsigned integers representing the encrypted data to be decrypted.</param>
    public void DecryptData(Span<uint> data)
    {
        ReadOnlySpan<uint> encryptionKeySpan = EncryptionKey.AsSpan();

        Queue<uint> queue = new();
        queue.Enqueue(0);
        queue.Enqueue(0);

        for (var i = 4; i < data.Length - 1; i += 2)
        {
            queue.Enqueue(data[i]);
            queue.Enqueue(data[i + 1]);

            Decrypt(data.Slice(i, 2), encryptionKeySpan);

            data[i] ^= queue.Dequeue();
            data[i + 1] ^= queue.Dequeue();
        }
    }
    
    /// <summary>
    /// Encrypts the specified data in place using the provided encryption key and substitution box (S-box) spans.
    /// </summary>
    /// <param name="data">A span of 32-bit unsigned integers representing the data to be encrypted.</param>
    public void EncryptHeader(Span<uint> data)
    {
        ReadOnlySpan<uint> encryptionKeySpan = EncryptionKey.AsSpan();

        for (var i = 0; i < data.Length - 1; i += 2)
            Encrypt(data.Slice(i, 2), encryptionKeySpan);
    }

    /// <summary>
    /// Encrypts the specified data in place using the provided encryption key and substitution box (S-box) spans.
    /// </summary>
    /// <param name="data">A span of 32-bit unsigned integers representing the data to be encrypted.</param>
    public void EncryptData(Span<uint> data)
    {
        ReadOnlySpan<uint> encryptionKeySpan = EncryptionKey.AsSpan();

        Queue<uint> queue = new();
        queue.Enqueue(0);
        queue.Enqueue(0);

        for (var i = 4; i < data.Length - 1; i += 2)
        {
            data[i] ^= queue.Dequeue();
            data[i + 1] ^= queue.Dequeue();

            Encrypt(data.Slice(i, 2), encryptionKeySpan);

            queue.Enqueue(data[i]);
            queue.Enqueue(data[i + 1]);
        }
    }
}