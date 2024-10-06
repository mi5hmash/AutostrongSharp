namespace AutostrongSharpCore.Models.DSSS.AutoStrong.GameProfile;

public class GameProfileFileInfo(string fullPath)
{
    public string FullPath { get; } = fullPath;
    public string Name { get; } = Path.GetFileNameWithoutExtension((string?)fullPath) ?? string.Empty;
}