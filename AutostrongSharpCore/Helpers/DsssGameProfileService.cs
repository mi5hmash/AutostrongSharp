using System.Text.Json;
using AutostrongSharpCore.Models;
using static AutostrongSharpCore.Helpers.IoHelpers;

namespace AutostrongSharpCore.Helpers;

public class DsssGameProfileService
{
    /// <summary>
    /// Name of the profile.
    /// </summary>
    public string ProfileName { get; } = "DSSS";

    /// <summary>
    /// Magic string used for spell casting.
    /// </summary>
    private string Magic { get; } = @"{`liVW.(%(e6iP\Hy/Le\ivA(xx.>sx\Ml2pDe}^Of9rmMf&=>01UXh3k?wWv^|Y`/M_B(l5";

    /// <summary>
    /// Game Profile version.
    /// </summary>
    private uint GpVersion { get; } = 1;
    
    /// <summary>
    /// Enumeration of all available Platforms.
    /// </summary>
    public enum PlatformsEnum
    {
        Pc = 0,
        Ps4 = 1,
        XboxOne = 2
    }

    /// <summary>
    /// The title of a game.
    /// </summary>
    public string GameTitle { get; set; } = "Unknown";

    /// <summary>
    /// AppIcon encoded with Base64.
    /// </summary>
    public string Base64AppIcon { get; set; } = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAclBMVEUAAAD6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRP6tRO9kW18AAAAJXRSTlMAvkD40IEJ/GnklCsfEe2qn45b3mPziXduUToMyLaxpp1UTjMltzRUGQAAAOtJREFUOMvNk7kCgjAQREeEEC4V8EDwVv7/F9UsiRMsaCx8DcxmSPYI+Blq1nvMFHyifkQEj3w+NsxzMMv+iyWIYAjW5VpHh0EEZAhlVy2qrY0M4dDyyQ3AvVVAvjFa2/ViYfQJUK+tkgewNYFFASGVDSpg936WwF4iqaxnscg1kAwduEokzozB9nBTmrewA862n1yijRXmBCp15a0fFVDFTq7GhqQDGqfEEIxGVLEOKElbScgJUZnCBdlH2DKRchI895RbLdTVThxeq6E5qdIJTeMmgxzI4+ZSk2YbO/PUlZu8tFPXfvrH+R+eloxHObKCkJYAAAAASUVORK5CYII=";

    /// <summary>
    /// Steam AppID.
    /// </summary>
    public uint SteamAppId { get; set; }
    
    /// <summary>
    /// Platform.
    /// </summary>
    public uint Platform { get; set; } = (uint)PlatformsEnum.Pc;

    /// <summary>
    /// Tries to load the game profile from the input file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Success status flag.</returns>
    public BoolResult LoadGameProfile(string filePath, AutoStrongDeencryptor deencryptor)
    {
        // Try to read file.
        DsssGameProfileJson gpJson;
        try
        {
            var jsonData = ReadFile(filePath).Decrypto(Magic);
            gpJson = JsonSerializer.Deserialize<DsssGameProfileJson>(jsonData)!;
        }
        catch { return new BoolResult(false, "Game profile couldn't be loaded. Invalid file."); }
        
        // Check file version.
        if (gpJson.GpVersion is null || gpJson.GpVersion > GpVersion) { return new BoolResult(false, "Game profile couldn't be loaded. Invalid file version."); }

        // Check values for null.
        if (gpJson.Platform is null || gpJson.Base64AppIcon is null || gpJson.EncryptionKey is null || gpJson.EncryptionTable is null || gpJson.GameTitle is null)
        { return new BoolResult(false, "Game profile couldn't be loaded. Invalid values."); }

        // Checks for the PC version.
        if (gpJson.Platform == (uint)PlatformsEnum.Pc)
        {
            if (gpJson.SteamAppId is null) return new BoolResult(false, "Game profile couldn't be loaded. Missing SteamAppId.");
            SteamAppId = (uint)gpJson.SteamAppId;
        }

        // Load data from the Game Profile.
        GameTitle = gpJson.GameTitle;
        Platform = (uint)gpJson.Platform;
        if (!string.IsNullOrEmpty(gpJson.Base64AppIcon)) Base64AppIcon = gpJson.Base64AppIcon;
        deencryptor.Setup(gpJson.EncryptionKey, gpJson.EncryptionTable);
        
        return new BoolResult(true, "Successfully loaded the Game Profile.");
    }
}