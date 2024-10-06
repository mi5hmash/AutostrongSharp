using System.Runtime.InteropServices;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x4)]
public class AutoStrongFooter
{
    /// <summary>
    /// A file signature.
    /// </summary>
    public uint Signature { get; set; } = 0x7856_3412;

    /// <summary>
    /// Create a parameter-less <see cref="AutoStrongFooter"/>.
    /// </summary>
    public AutoStrongFooter() { }

    /// <summary>
    /// Create a <see cref="AutoStrongFooter"/> with given parameters.
    /// </summary>
    /// <param name="signature"></param>
    public AutoStrongFooter(uint signature)
    {
        Signature = signature;
    }
}