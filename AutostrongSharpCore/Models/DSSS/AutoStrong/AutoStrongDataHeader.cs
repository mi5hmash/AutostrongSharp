﻿using AutostrongSharpCore.Helpers;
using System.Runtime.InteropServices;

namespace AutostrongSharpCore.Models.DSSS.AutoStrong;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
public class AutoStrongDataHeader
{
    private uint _fileFormat1 = 0x5353_5344;
    private uint _fileFormat2 = 0x5353_5344;
    private uint _steam32Id = 0x0000_0001;
    private uint _unknown1;
    private uint _unknown2 = 0x0511_4CB5;
    private uint _unknown3 = 0x0000_0035;
    private uint _unknown4 = 0xF627_F2FD;
    private uint _unknown5 = 0xE15F_B3A5;

    /// <summary>
    /// This should be the ANSI for "DSSS", or 0x5353_5344.
    /// </summary>
    public uint FileFormat1
    {
        get => _fileFormat1;
        set => _fileFormat1 = value;
    }

    /// <summary>
    /// Another one. This also should be the ANSI for "DSSS", or 0x5353_5344.
    /// </summary>
    public uint FileFormat2
    {
        get => _fileFormat2;
        set => _fileFormat2 = value;
    }

    /// <summary>
    /// Holds Steam User ID in the Steam32_ID format.
    /// </summary>
    public uint Steam32Id
    {
        get => _steam32Id;
        set => _steam32Id = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown1
    {
        get => _unknown1;
        set => _unknown1 = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown2
    {
        get => _unknown2;
        set => _unknown2 = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown3
    {
        get => _unknown3;
        set => _unknown3 = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown4
    {
        get => _unknown4;
        set => _unknown4 = value;
    }

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown5
    {
        get => _unknown5;
        set => _unknown5 = value;
    }

    /// <summary>
    /// Create a parameter-less <see cref="AutoStrongHeader"/>.
    /// </summary>
    public AutoStrongDataHeader() { }

    /// <summary>
    /// Create a <see cref="AutoStrongHeader"/> with given parameters.
    /// </summary>
    /// <param name="steam32Id"></param>
    /// <param name="unknown1"></param>
    /// <param name="unknown2"></param>
    /// <param name="unknown3"></param>
    /// <param name="unknown4"></param>
    /// <param name="unknown5"></param>
    public AutoStrongDataHeader(uint steam32Id, uint unknown1, uint unknown2, uint unknown3, uint unknown4, uint unknown5)
    {
        Steam32Id = steam32Id;
        Unknown1 = unknown1;
        Unknown2 = unknown2;
        Unknown3 = unknown3;
        Unknown4 = unknown4;
        Unknown5 = unknown5;
    }

    /// <summary>
    /// Decrypts <see cref="AutoStrongDataHeader"/>.
    /// </summary>
    public void DecryptData(AutoStrongDeencryptor dsssDeencryptor)
    {
        dsssDeencryptor.Decrypt(ref _fileFormat1, ref _fileFormat2);
        dsssDeencryptor.Decrypt(ref _steam32Id, ref _unknown1);
        dsssDeencryptor.Decrypt(ref _unknown2, ref _unknown3);
        dsssDeencryptor.Decrypt(ref _unknown4, ref _unknown5);
    }

    /// <summary>
    /// Encrypts <see cref="AutoStrongDataHeader"/>.
    /// </summary>
    public void EncryptData(AutoStrongDeencryptor dsssDeencryptor)
    {
        dsssDeencryptor.Encrypt(ref _fileFormat1, ref _fileFormat2);
        dsssDeencryptor.Encrypt(ref _steam32Id, ref _unknown1);
        dsssDeencryptor.Encrypt(ref _unknown2, ref _unknown3);
        dsssDeencryptor.Encrypt(ref _unknown4, ref _unknown5);
    }

    /// <summary>
    /// Returns true if file is Encrypted.
    /// </summary>
    /// <returns></returns>
    public bool IsEncrypted() => FileFormat1 != 0x5353_5344 || FileFormat2 != 0x5353_5344;

    /// <summary>
    /// Gets decrypted <see cref="Steam32Id"/>.
    /// </summary>
    /// <param name="dsssDeencryptor"></param>
    /// <returns></returns>
    public uint GetSteamId(AutoStrongDeencryptor dsssDeencryptor)
    {
        var sid = _steam32Id;
        var unk = _unknown1;
        if (IsEncrypted()) dsssDeencryptor.Decrypt(ref sid, ref unk);
        return sid;
    }

    /// <summary>
    /// Returns false if file is not supported.
    /// </summary>
    /// <param name="dsssDeencryptor"></param>
    /// <returns></returns>
    public BoolResult CheckIntegrity(AutoStrongDeencryptor dsssDeencryptor)
    {
        var ff1 = _fileFormat1;
        var ff2 = _fileFormat2;
        if (IsEncrypted()) dsssDeencryptor.Decrypt(ref ff1, ref ff2);
        if (ff1 != 0x5353_5344 || ff2 != 0x5353_5344) return new BoolResult(false, "File is not compatible with the selected game profile.");
        return new BoolResult(true);
    }
}