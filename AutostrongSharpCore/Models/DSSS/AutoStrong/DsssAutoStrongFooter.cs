using System.Runtime.InteropServices;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x4)]
public class DsssAutoStrongFooter
{
    /// <summary>
    /// A file signature.
    /// </summary>
    public uint Signature { get; set; } = 0x7856_3412;

    /// <summary>
    /// Create a parameter-less <see cref="DsssAutoStrongFooter"/>.
    /// </summary>
    public DsssAutoStrongFooter() { }

    /// <summary>
    /// Create a <see cref="DsssAutoStrongFooter"/> with given parameters.
    /// </summary>
    /// <param name="signature"></param>
    public DsssAutoStrongFooter(uint signature)
    {
        Signature = signature;
    }
}