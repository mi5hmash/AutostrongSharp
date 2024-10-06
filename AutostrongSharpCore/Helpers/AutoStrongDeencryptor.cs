namespace AutostrongSharpCore.Helpers;

/// <summary>
/// Default Constructor that loads configuration.
/// </summary>
public class AutoStrongDeencryptor
{
    #region PROPERTIES

    private uint[] EncryptionKey { get; set; } = [];
    private uint[] EncryptionTable { get; set; } = [];

    #endregion

    /// <summary>
    /// Sets up all parameters.
    /// </summary>
    /// <param name="base64EncryptionKey"></param>
    /// <param name="base64EncryptionTable"></param>
    public void Setup(string base64EncryptionKey, string base64EncryptionTable)
    {
        EncryptionKey = base64EncryptionKey.Base64DecodeUtf8().ToUintArray();
        EncryptionTable = base64EncryptionTable.Base64DecodeUtf8().ToUintArray();
    }
    
    /// <summary>
    /// Decrypts data.
    /// </summary>
    /// <param name="inputUint1"></param>
    /// <param name="inputUint2"></param>
    public void Decrypt(ref uint inputUint1, ref uint inputUint2)
    {
        Span<uint> encryptionKeySpan = EncryptionKey;
        Span<uint> encryptionTableSpan = EncryptionTable;
        
        Queue<uint> queue = new();
        queue.Enqueue(inputUint1);
        queue.Enqueue(inputUint2);

        var v0 = queue.Dequeue() ^ encryptionKeySpan[^1];

        for (var i = 1; i < encryptionKeySpan.Length - 1; i++)
        {
            var v1 = (((encryptionTableSpan[(int)(((v0 >> 16) & 0xFF) + 256)] + encryptionTableSpan[(int)(v0 >> 24)]) ^ encryptionTableSpan[(int)(((v0 >> 8) & 0xFF) + 512)]) + encryptionTableSpan[(int)((v0 & 0xFF) + 768)]) ^ encryptionKeySpan[^(i+1)] ^ queue.Dequeue();
            queue.Enqueue(v0);
            v0 = v1;
        }

        inputUint1 = encryptionKeySpan[0] ^ queue.Dequeue();
        inputUint2 = v0;
    }

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="inputUint1"></param>
    /// <param name="inputUint2"></param>
    public void Encrypt(ref uint inputUint1, ref uint inputUint2)
    {
        Span<uint> encryptionKeySpan = EncryptionKey;
        Span<uint> encryptionTableSpan = EncryptionTable;

        Queue<uint> queue = new();
        queue.Enqueue(inputUint1);
        queue.Enqueue(inputUint2);

        var v0 = queue.Dequeue() ^ encryptionKeySpan[0];

        for (var i = 1; i < encryptionKeySpan.Length - 1; i++)
        {
            var v1 = (((encryptionTableSpan[(int)(((v0 >> 16) & 0xFF) + 256)] + encryptionTableSpan[(int)(v0 >> 24)]) ^ encryptionTableSpan[(int)(((v0 >> 8) & 0xFF) + 512)]) + encryptionTableSpan[(int)((v0 & 0xFF) + 768)]) ^ encryptionKeySpan[i] ^ queue.Dequeue();
            queue.Enqueue(v0);
            v0 = v1;
        }

        inputUint1 = encryptionKeySpan[^1] ^ queue.Dequeue();
        inputUint2 = v0;
    }
}