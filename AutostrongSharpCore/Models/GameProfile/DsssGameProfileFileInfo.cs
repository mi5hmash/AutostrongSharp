namespace AutostrongSharpCore.Models.GameProfile;

public class DsssGameProfileFileInfo(string fullPath)
{
    public string FullPath { get; } = fullPath;
    public string Name { get; } = Path.GetFileNameWithoutExtension((string?)fullPath) ?? string.Empty;
}