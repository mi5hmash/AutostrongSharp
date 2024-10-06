// v2024-10-06 00:19:03

using System.IO.Compression;
using static AutostrongSharpCore.Helpers.IoHelpers;

namespace AutostrongSharpCore.Helpers;

public class SimpleBackup
{
    private readonly int _maxBackups;
    private readonly string _backupRootDirectory;
    private readonly string _backupFolderNamePrefix;
    private readonly bool _zipBackups;
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
        _backupFolderNamePrefix = backupFolderNamePrefix;
        _zipBackups = zipBackups;
        CurrentBackupDirectory = NewBackupPath();
    }

    /// <summary>
    /// Combines a path to a new backup folder. 
    /// </summary>
    /// <returns></returns>
    private string NewBackupPath()
        => Path.Combine(_backupRootDirectory, $"{_backupFolderNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}");

    /// <summary>
    /// Deletes the current backup.
    /// </summary>
    private void DeleteCurrentBackup()
        => SafelyDeleteDirectory(CurrentBackupDirectory);

    /// <summary>
    /// Creates a folder for a new backup.
    /// </summary>
    public void NewBackup()
    {
        CurrentBackupDirectory = NewBackupPath();
        RecreateDirectory(CurrentBackupDirectory);

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
    /// Finalizes backup and packs it into a zip file if <see cref="_zipBackups"/> flag is true.
    /// </summary>
    public void FinalizeBackup()
    {
        // check if directory exist or if backup is finalized
        if (!Directory.Exists(CurrentBackupDirectory) || _isFinalized) return;

        var files = Directory.GetFiles(CurrentBackupDirectory);
        if (files.Length > 0)
        {
            if (_zipBackups)
            {
                // save a new backup
                ZipFile.CreateFromDirectory(CurrentBackupDirectory, $"{CurrentBackupDirectory}.zip");
                DeleteCurrentBackup();
                // delete the oldest backup(s) if the backup limit is reached
                var backups = Directory.GetFiles(_backupRootDirectory, "*.zip", SearchOption.TopDirectoryOnly)
                    .Where(filePath => Path.GetFileName(filePath).StartsWith(_backupFolderNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();
                var limitOverflow = backups.Count - _maxBackups;
                if (limitOverflow > 0) SafelyDeleteFiles(backups.TakeLast(limitOverflow).ToArray());
            }
            else
            {
                // delete the oldest backup(s) if the backup limit is reached
                var backupDirs = Directory.GetDirectories(_backupRootDirectory)
                    .Where(folderPath => Path.GetFileName(folderPath).StartsWith(_backupFolderNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();
                var limitOverflow = backupDirs.Count - _maxBackups;
                if (limitOverflow > 0) SafelyDeleteDirectories(backupDirs.TakeLast(limitOverflow).ToArray());
            }
            _isFinalized = true;
            return;
        }
        DeleteCurrentBackup();
        _isFinalized = true;
    }
}