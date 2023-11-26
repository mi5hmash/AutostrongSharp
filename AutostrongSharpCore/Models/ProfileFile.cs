namespace AutostrongSharpCore.Models;

public class ProfileFile(string fullPath)
{
    public string FullPath { get; } = fullPath;
    public string Name { get; } = Path.GetFileNameWithoutExtension((string?)fullPath) ?? string.Empty;
}