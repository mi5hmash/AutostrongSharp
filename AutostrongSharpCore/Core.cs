using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Models;
using AutostrongSharpCore.Models.DSSS.AutoStrong;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static AutostrongSharpCore.Helpers.ISimpleMediator;

namespace AutostrongSharpCore;

public class Core
{

    #region PATHS

    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    private static string BackupFolder => "_BACKUP";
    public static string BackupPath => Path.Combine(RootPath, BackupFolder);

    private static string ProfilesFolder => "profiles";
    public static string ProfilesPath => Path.Combine(RootPath, ProfilesFolder);

    private static string PathPattern => @$"\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}remote\{Path.DirectorySeparatorChar}win64_save\{Path.DirectorySeparatorChar}?$";

    #endregion

    #region BACKUP

    public bool BackupEnabled { get; private set; } = true;
    
    public void SetBackupEnabled(bool boolean)
    {
        if (IsBusy) return;
        BackupEnabled = boolean;
    }

    public void ToggleBackupEnabled()
    {
        if (IsBusy) return;
        BackupEnabled ^= true;
    }

    #endregion

    #region BUSY_LOCK

    public bool IsBusy { get; private set; }

    #endregion

    #region PROGRESS

    private readonly IProgress<string> _pText;
    private readonly IProgress<int> _pValue;

    /// <summary>
    /// Reports progress.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    private void ReportProgress(string text, int value)
    {
        _pText.Report(text);
        _pValue.Report(value);
    }

    #endregion

    #region GAME_PROFILE

    private DsssGameProfileService _gameProfile = new();
    public List<ProfileFile> GameProfileFiles { get; private set; } = new();
    public int GameProfileIndex { get; private set; }

    private void RefreshGameProfiles()
        => GameProfileFiles = Directory.GetFiles(ProfilesPath, "*.bin", SearchOption.TopDirectoryOnly).Select(file => new ProfileFile(file)).ToList();

    public string GetGameProfileIcon() => _gameProfile.Base64AppIcon;
    public uint GetGameProfileSteamAppId() => _gameProfile.SteamAppId;

    public void SetNewGameProfile(int selectedIndex)
    {
        if (IsBusy) return;
        GameProfileIndex = selectedIndex;
        var profileFilePath = GameProfileFiles[GameProfileIndex].FullPath;
        _gameProfile = new DsssGameProfileService();
        var boolResult = _gameProfile.LoadGameProfile(profileFilePath, _deencryptor);
        ReportProgress(boolResult.Description, 0);
    }

    private string GetGameUrl() => $"https://store.steampowered.com/app/{_gameProfile.SteamAppId}/";

    #endregion

    #region CONSTRUCTOR

    private readonly AutoStrongDeencryptor _deencryptor = new();
    private CancellationTokenSource _cts = new();
    private static ISimpleMediator _mediator = null!;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Core(ISimpleMediator mediator, IProgress<string> pText, IProgress<int> pValue)
    {
        _mediator = mediator;
        _pText = pText;
        _pValue = pValue;
        InputDirectory = RootPath;
        InitializeComponent();
    }

    /// <summary>
    /// Initialize component.
    /// </summary>
    private void InitializeComponent()
    {
        // create directories
        CreateDirectories();
        RefreshGameProfiles();
    }

    #endregion

    #region IO

    /// <summary>
    /// Creates necessary directories.
    /// </summary>
    private static void CreateDirectories()
    {
        Directory.CreateDirectory(ProfilesPath);
        Directory.CreateDirectory(BackupPath);
    }

    /// <summary>
    /// Checks whether the directory at the given path exists.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    private static bool DoesDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath)) return true;
        _mediator.Inform($"""Directory: "{directoryPath}" does not exists.""", "Error", DialogType.Error);
        return false;
    }

    /// <summary>
    /// Tries to write an array of bytes to a file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileData"></param>
    /// <returns></returns>
    private static DialogAnswer WriteBytesToFile(string filePath, Span<byte> fileData)
    {
        do
        {
            if (TryWriteAllBytes(filePath, fileData)) return DialogAnswer.Continue;
            // ask the user if they want to try again
            var dialogResult = _mediator.Ask($"""Failed to save the file: "{filePath}".{Environment.NewLine}It may be currently in use by another program.{Environment.NewLine}Would you like to try again?""", "Failed to save the file", QuestionOptions.AbortRetryIgnore, DialogType.Exclamation);
            if (dialogResult == DialogAnswer.Retry) continue;
            return dialogResult;
        } while (true);

        static bool TryWriteAllBytes(string fPath, Span<byte> bytes)
        {
            try
            {
                File.WriteAllBytes(fPath, bytes.ToArray());
            }
            catch { return false; }
            return true;
        }
    }

    /// <summary>
    /// Tries to make a backup of a file.
    /// </summary>
    /// <param name="simpleBackup"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static DialogAnswer MakeBackup(string filePath, SimpleBackup simpleBackup)
    {
        do
        {
            if (TryToMakeBackup(filePath, simpleBackup)) return DialogAnswer.Continue;
            // ask the user if they want to try again
            var dialogResult = _mediator.Ask($"""Failed to backup the file: "{filePath}".{Environment.NewLine}The file cannot be found, or a file with the same name exists in the target location and is used by another program.{Environment.NewLine}Would you like to try again?""", "Failed to backup the file", QuestionOptions.AbortRetryIgnore, DialogType.Exclamation);
            if (dialogResult == DialogAnswer.Retry) continue;
            return dialogResult;
        } while (true);

        static bool TryToMakeBackup(string filePath, SimpleBackup simpleBackup)
        {
            try
            {
                simpleBackup.Backup(filePath);
            }
            catch { return false; }
            return true;
        }
    }

    /// <summary>
    /// Opens the Backup directory
    /// </summary>
    public static void OpenBackupDirectory()
        => IoHelpers.OpenDirectory(BackupPath);

    /// <summary>
    /// Opens a game website in a default web browser.
    /// </summary>
    public void VisitGameWebsite()
        => Process.Start(new ProcessStartInfo { FileName = GetGameUrl(), UseShellExecute = true });

    #endregion

    #region STEAM_ID

    public uint SteamId { get; private set; }

    /// <summary>
    /// Validates a <paramref name="steamId"/> string and sets the <see cref="SteamId"/> property.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="verbose"></param>
    public void SetSteamId(string steamId, bool verbose = false)
    { 
        if (IsBusy) return;
        var result = ulong.TryParse(steamId, out var parsed);
        if (result) SteamId = (uint)parsed;
        if (!verbose) ReportProgress(result ? "The entered SteamID is correct." : "The entered SteamID was invalid.", 0);
    }

    /// <summary>
    /// Extracts SteamID from a directory path.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public void ExtractSteamIdFromPathIfValid(string directoryPath)
    {
        var match = Regex.Match(directoryPath, PathPattern);
        if (match.Success) SetSteamId(match.Groups[1].Value, true);
    }

    #endregion

    #region INPUT_DIRECTORY

    public string InputDirectory { get; private set; }

    /// <summary>
    /// Validates an <paramref name="inputPath"/> string and sets the <see cref="InputDirectory"/> property.
    /// </summary>
    /// <param name="inputPath"></param>
    public void SetInputDirectory(string inputPath)
    {
        if (IsBusy) return;
        var result = Directory.Exists(inputPath);
        if (result)
        {
            InputDirectory = inputPath;
            ExtractSteamIdFromPathIfValid(inputPath);
        }
        ReportProgress(result ? "The entered Input Folder Path is correct." : "The entered Input Folder Path was invalid.", 0);
    }

    #endregion

    #region OPERATIONS

    /// <summary>
    /// Aborts currently running operation and lifts the Busy Lock.
    /// </summary>
    public void AbortOperation()
    {
        if (!IsBusy) return;
        _cts.Cancel();
        _cts.Dispose();
        IsBusy = false;
    }

    private delegate void OperationDelegate(DsssAutoStrongFile dsssFile);

    private async Task ProcessAsyncOperation(OperationType operationType, OperationDelegate operationDelegate)
    {
        if (IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        try
        {
            await AsyncOperation(operationType, operationDelegate);
        }
        catch (OperationCanceledException)
        {
            _pText.Report("The operation was aborted by the user.");
        }
        AbortOperation();
    }

    private Task AsyncOperation(OperationType operationType, OperationDelegate operationDelegate)
    {
        return Task.Run(() =>
        {
            if (!DoesDirectoryExists(InputDirectory)) return;

            var processedFiles = 0;

            var files = Directory.GetFiles(InputDirectory);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            // initialize backup
            SimpleBackup simpleBackup = new(BackupPath);
            if (files.Length > 0) simpleBackup.NewBackup();

            var progress = 0;
            ReportProgress($"[{progress}/{files.Length}] Processing files...", progress);
            Parallel.For((long)0, files.Length, po, ctr =>
            {
                // load file
                var dsssFile = new DsssAutoStrongFile(_deencryptor);
                var result = dsssFile.SetFileData(files[ctr]);
                Debug.Print(result.Result.ToString());
                if (!result.Result) goto ORDER_66;

                // check whether the file is encrypted or decrypted
                switch (operationType)
                {
                    case OperationType.Encryption:
                        if (dsssFile.IsEncrypted()) goto ORDER_66;
                        break;
                    case OperationType.Decryption:
                        if (!dsssFile.IsEncrypted()) goto ORDER_66;
                        break;
                }
                
                // backup file
                if (BackupEnabled)
                {
                    var backupResult = MakeBackup(files[ctr], simpleBackup);
                    switch (backupResult)
                    {
                        case DialogAnswer.Continue:
                            break;
                        case DialogAnswer.Abort:
                            _cts.Cancel();
                            goto ORDER_66;
                        default:
                            goto ORDER_66;
                    }
                }

                // run operation
                operationDelegate(dsssFile);

                // save file
                var fileData = dsssFile.GetFileData();
                var writeResult = WriteBytesToFile(files[ctr], fileData);
                Debug.Print(writeResult.ToString());
                switch (writeResult)
                {
                    case DialogAnswer.Continue:
                        processedFiles++;
                        break;
                    case DialogAnswer.Abort:
                        _cts.Cancel();
                        goto ORDER_66;
                    default:
                        goto ORDER_66;
                }

            ORDER_66:
                Interlocked.Increment(ref progress);
                ReportProgress($"[{progress}/{files.Length}] Processing files...", (int)((double)progress / files.Length * 100));
            });
            
            simpleBackup.FinalizeBackup();

            ReportProgress($"{operationType} done. Number of processed files: {processedFiles}.", 100);
        });
    }
    
    private void DecryptAll(DsssAutoStrongFile dsssFile)
    {
        dsssFile.DecryptDataHeader();
        dsssFile.DecryptData();
    }

    private void EncryptAll(DsssAutoStrongFile dsssFile)
    {
        dsssFile.EncryptDataHeader();
        dsssFile.EncryptData();
    }

    private void ResignAll(DsssAutoStrongFile dsssFile)
    {
        if (dsssFile.IsEncrypted())
        {
            dsssFile.DecryptDataHeader();
            dsssFile.DataHeader.Steam32Id = SteamId;
            dsssFile.EncryptDataHeader();
        }
        else
        {
            dsssFile.DataHeader.Steam32Id = SteamId;
            dsssFile.EncryptDataHeader();
            dsssFile.EncryptData();
        }
    }

    private enum OperationType
    {
        Encryption,
        Decryption,
        Resigning
    }

    public async Task DecryptAllAsync()
        => await ProcessAsyncOperation(OperationType.Decryption, DecryptAll);

    public async Task EncryptAllAsync()
        => await ProcessAsyncOperation(OperationType.Encryption, EncryptAll);

    public async Task ResignAllAsync()
        => await ProcessAsyncOperation(OperationType.Resigning, ResignAll);

    #endregion
}