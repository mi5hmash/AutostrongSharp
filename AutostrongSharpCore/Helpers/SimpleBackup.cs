using System.IO.Compression;
using static AutostrongSharpCore.Helpers.IoHelpers;

namespace AutostrongSharpCore.Helpers;

public class SimpleBackup
{
    private readonly int _maxBackups;
    private readonly string _backupRootDirectory;
    private readonly string _backupFolderNamePrefix;
    private readonly bool _zipBackups;
    private uint _backupFilesCount;
    private bool _isFinalized = true;

    /// <summary>
    /// A directory of a current backup.
    /// </summary>
    public string CurrentBackupDirectory { get; private set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="backupRootDirectory"></param>
    /// <param name="maxBackups"></param>
    /// <param name="backupFolderNamePrefix"></param>
    /// <param name="zipBackups"></param>
    public SimpleBackup(string backupRootDirectory, int maxBackups = 3, string backupFolderNamePrefix = "backup", bool zipBackups = true)
    {
        _backupRootDirectory = backupRootDirectory;
        _maxBackups = maxBackups;
        _backupFolderNamePrefix = $"{backupFolderNamePrefix}_";
        _zipBackups = zipBackups;
        CurrentBackupDirectory = NewBackupDir();
    }

    private string NewBackupDir() 
        => Path.Combine(_backupRootDirectory, $"{_backupFolderNamePrefix}{DateTime.Now:yyyyMMddHHmmssfff}");
    
    /// <summary>
    /// Creates a folder for a new backup.
    /// </summary>
    public void NewBackup()
    {
        if (_zipBackups)
        {
            var backups = Directory.GetFiles(_backupRootDirectory, "*.zip", SearchOption.TopDirectoryOnly)
                .Where(folderPath => Path.GetFileName(folderPath).StartsWith(_backupFolderNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();
            // delete the oldest backup if the backup limit is reached
            if (backups.Count >= _maxBackups) SafelyDeleteFile(backups.Last()); 
        }
        else
        {
            var backupDirs = Directory.GetDirectories(_backupRootDirectory)
                .Where(folderPath => Path.GetFileName(folderPath).StartsWith(_backupFolderNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();
            // delete the oldest backup if the backup limit is reached
            if (backupDirs.Count >= _maxBackups) SafelyDeleteDirectory(backupDirs.Last());
        }

        CurrentBackupDirectory = NewBackupDir();
        Directory.CreateDirectory(CurrentBackupDirectory);

        _isFinalized = false;
    }

    /// <summary>
    /// Adds file to a directory.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool Backup(string filePath)
    {
        if (!File.Exists(filePath) || _isFinalized) return false;
        File.Copy(filePath, Path.Combine(CurrentBackupDirectory, Path.GetFileName(filePath)));
        _backupFilesCount++;
        return true;
    }
    /// <summary>
    /// Adds many files to a directory.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    public bool[] Backup(string[] filePaths)
    {
        var results = new bool[filePaths.Length];
        for (var i = 0; i < filePaths.Length; i++) 
            results[i] = Backup(filePaths[i]);

        return results;
    }

    /// <summary>
    /// Deletes the current backup.
    /// </summary>
    private void DeleteCurrentBackup() => Directory.Delete(CurrentBackupDirectory, true);


    /// <summary>
    /// Finalizes backup and packs it into a zip file if <see cref="_zipBackups"/> flag is true.
    /// </summary>
    public void FinalizeBackup()
    {
        if (!Directory.Exists(CurrentBackupDirectory) || _isFinalized) return;
        if (_backupFilesCount > 0)
        {
            if (_zipBackups)
            {
                ZipFile.CreateFromDirectory(CurrentBackupDirectory, $"{CurrentBackupDirectory}.zip");
                DeleteCurrentBackup();
            }
            _isFinalized = true;
            return;
        }
        DeleteCurrentBackup();
        _isFinalized = true;
    }
}