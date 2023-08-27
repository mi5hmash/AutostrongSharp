namespace AutostrongSharp.Helpers;

/// <summary>
/// Container for Global Read-Only Variables.
/// </summary>
public static class AppInfo
{
    #region APP INFO
    
    public static string Name => "Autostrong Sharp";

    public const string ToolVersion = "1.0.0.0";

    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    public static bool BackupEnabled = true;

    #endregion
    
    #region OTHER INFO
    
    public static string BackupFolder => "_BACKUP";

    public static string ProfilesFolder => "profiles";
    
    public static string BackupPath => Path.Combine(RootPath, BackupFolder);

    public static string ProfilesPath => Path.Combine(RootPath, ProfilesFolder);

    #endregion
}