namespace AutostrongSharpCore.Models;

public class DsssGameProfileJson
{
    /// <summary>
    /// The title of a game.
    /// </summary>
    public string? GameTitle { get; set; }

    /// <summary>
    /// Game Profile version.
    /// </summary>
    public uint? GpVersion { get; set; }

    /// <summary>
    /// Steam AppID.
    /// </summary>
    public uint? SteamAppId { get; set; }

    /// <summary>
    /// AppIcon encoded with Base64.
    /// </summary>
    public string? Base64AppIcon { get; set; }

    /// <summary>
    /// EncryptionKey.
    /// </summary>
    public string? EncryptionKey { get; set; }

    /// <summary>
    /// EncryptionKey.
    /// </summary>
    public string? EncryptionTable { get; set; }

    /// <summary>
    /// Platform (0:PC; 1:PS4; 2:XboxOne).
    /// </summary>
    public uint? Platform { get; set; }

    /// <summary>
    /// Default Constructor that loads configuration.
    /// </summary>
    public DsssGameProfileJson()
    {
        // must be present, otherwise there will be problems on deserialization
    }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    /// <param name="gameTitle"></param>
    /// <param name="gpVersion"></param>
    public DsssGameProfileJson(string gameTitle, uint gpVersion)
    {
        GameTitle = gameTitle;
        GpVersion = gpVersion;
    }
}