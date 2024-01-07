using System.Text.Json;
using AutostrongSharpCore.Models;
using AutostrongSharpCore.Models.GameProfile;
using static AutostrongSharpCore.Helpers.IoHelpers;

namespace AutostrongSharpCore.Helpers;

public class DsssGameProfileService
{
    /// <summary>
    /// Enumeration of all available Platforms.
    /// </summary>
    public enum PlatformsEnum
    {
        Pc,
        Ps4,
        XboxOne
    }

    /// <summary>
    /// Type of the <see cref="GameProfile"/>.
    /// </summary>
    private static string GpType => "AutostrongSharpDSSS";

    /// <summary>
    /// <see cref="GameProfile"/> version.
    /// </summary>
    private static uint GpVersion => 1;

    /// <summary>
    /// Magic string used for spell casting.
    /// </summary>
    private static string Magic => @"{`liVW.(%(e6iP\Hy/Le\ivA(xx.>sx\Ml2pDe}^Of9rmMf&=>01UXh3k?wWv^|Y`/M_B(l5";
    
    /// <summary>
    /// A path to directory where the <see cref="GameProfileFileList"/> are stored.
    /// </summary>
    private string GameProfilesPath { get; }

    /// <summary>
    /// A list of available <see cref="GameProfile"/>\s.
    /// </summary>
    public List<DsssGameProfileFileInfo> GameProfileFileList { get; private set; } = [];

    /// <summary>
    /// An index of currently selected <see cref="GameProfile"/>.
    /// </summary>
    public int GameProfileIndex { get; private set; }

    /// <summary>
    /// Currently loaded <see cref="GameProfile"/>.
    /// </summary>
    public DsssGameProfile GameProfile { get; private set; } = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="gameProfilesPath"></param>
    public DsssGameProfileService(string gameProfilesPath)
    {
        GameProfilesPath = gameProfilesPath;
        RefreshGameProfiles();
    }
    
    private void RefreshGameProfiles()
        => GameProfileFileList = Directory.GetFiles(GameProfilesPath, "*.bin", SearchOption.TopDirectoryOnly).Select(file => new DsssGameProfileFileInfo(file)).ToList();

    /// <summary>
    /// Tries to load the game profile from the input file.
    /// </summary>
    /// <param name="selectedIndex"></param>
    /// <param name="deencryptor"></param>
    /// <returns>Success status flag.</returns>
    public BoolResult LoadGameProfile(int selectedIndex, AutoStrongDeencryptor deencryptor)
    {
        string? failureReason;

        // Make sure the selectedIndex is in bound.
        if (selectedIndex < 0 || selectedIndex > GameProfileFileList.Count) { failureReason = "File index out of bound"; goto ORDER_66; }
        
        // Try to read file.
        DsssGameProfileJson gpJson;
        try
        {
            var jsonData = ReadFile(GameProfileFileList[selectedIndex].FullPath).Decrypto(Magic);
            gpJson = JsonSerializer.Deserialize<DsssGameProfileJson>(jsonData)!;
        }
        catch { failureReason = "Invalid file"; goto ORDER_66; }

        // Check profile type.
        if (gpJson.GpType is null || gpJson.GpType != GpType) { failureReason = "Invalid file type"; goto ORDER_66; }

        // Check file version.
        if (gpJson.GpVersion is null || gpJson.GpVersion > GpVersion) { failureReason = "Invalid file version"; goto ORDER_66; }

        // Check values for null.
        if (gpJson.Platform is null) { failureReason = "Platform is null"; goto ORDER_66; }
        if (gpJson.EncryptionKey is null) { failureReason = "EncryptionKey is null"; goto ORDER_66; }
        if (gpJson.EncryptionTable is null) { failureReason = "EncryptionTable is null"; goto ORDER_66; }
        if (gpJson.GameTitle is null) { failureReason = "GameTitle is null"; goto ORDER_66; }

        DsssGameProfile gp = new();

        // Checks for the PC version.
        if (gpJson.Platform == (uint)PlatformsEnum.Pc)
        {
            if (gpJson.SteamAppId is null) { failureReason = "GameTitle is null"; goto ORDER_66; } 
            gp.SteamAppId = (uint)gpJson.SteamAppId;
        }

        // Load data from the Game Profile.
        gp.GameTitle = gpJson.GameTitle;
        gp.Platform = (uint)gpJson.Platform;
        if (!string.IsNullOrEmpty(gpJson.Base64AppIcon)) gp.Base64AppIcon = gpJson.Base64AppIcon;
        // Setup encryption parameters.
        deencryptor.Setup(gpJson.EncryptionKey, gpJson.EncryptionTable);

        // Update GameProfile and its Index
        GameProfile = gp;
        GameProfileIndex = selectedIndex;
        return new BoolResult(true, "Successfully loaded the Game Profile.");

        ORDER_66:
        return new BoolResult(false, $"Game Profile couldn't be loaded. {failureReason}.");
    }
}