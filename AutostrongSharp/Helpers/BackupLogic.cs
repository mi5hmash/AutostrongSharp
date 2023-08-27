namespace AutostrongSharp.Helpers;

public class BackupLogic
{
    private const string DirectoryName = "backup_"; 

    private readonly int _maxBackups;
    private readonly string _backupDirectory;
    private string _currentBackupDirectory;

    public BackupLogic(string backupDirectory, int maxBackups = 3)
    {
        _backupDirectory = backupDirectory;
        _currentBackupDirectory = NewBackupDir();
        _maxBackups = maxBackups;
    }

    private string NewBackupDir() => Path.Combine(_backupDirectory, $"{DirectoryName}{DateTime.Now:yyyyMMddHHmmssfff}");

    public void NewBackup()
    {
        var backupDirs = Directory.GetDirectories(_backupDirectory)
            .Where(folderPath => Path.GetFileName(folderPath).StartsWith(DirectoryName, StringComparison.OrdinalIgnoreCase)).OrderDescending();

        if (backupDirs != null && backupDirs.Count() >= _maxBackups) Directory.Delete(backupDirs.Last(), true);

        _currentBackupDirectory = NewBackupDir();
        Directory.CreateDirectory(_currentBackupDirectory);
    }

    public void Backup(string filePath) => File.Copy(filePath, Path.Combine(_currentBackupDirectory, Path.GetFileName(filePath)));

    public void DeleteCurrentBackup() => Directory.Delete(_currentBackupDirectory, true);
}