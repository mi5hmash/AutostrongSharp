namespace AutostrongSharpCore.Models;

public class ProfileFile
{
    public string FullPath { get; }
    public string Name { get; }

    public ProfileFile(string fullPath)
    {
        FullPath = fullPath;
        Name = Path.GetFileNameWithoutExtension(fullPath);
    }
}