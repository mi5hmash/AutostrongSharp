using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Models.DSSS.AutoStrong;
using AutostrongSharpCore.Models.GameProfile;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static AutostrongSharpCore.Helpers.IoHelpers;
using static AutostrongSharpCore.Helpers.ISimpleMediator;
using static AutostrongSharpCore.Helpers.SimpleLogger;

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

    /// <summary>
    /// Determines if backup is enabled.
    /// </summary>
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
        => OpenDirectory(BackupPath);

    #endregion

    #region BUSY_LOCK

    public bool IsBusy { get; private set; }

    #endregion

    #region LOGGER

    private readonly SimpleLogger _logger;

    public void ActivateLogger() => _logger.NewLogFile();

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

    private DsssGameProfileService _gpService = new(ProfilesPath);
    
    public void SetNewGameProfile(int selectedIndex)
    {
        if (IsBusy) return;
        var boolResult = _gpService.LoadGameProfile(selectedIndex, _deencryptor);
        ReportProgress(boolResult.Description, 0);
    }

    public List<DsssGameProfileFileInfo> GetGameProfileFileList() => _gpService.GameProfileFileList;
    public int GetSelectedGameProfileFileIndex() => _gpService.GameProfileIndex;
    public string GetGameProfileIcon() => _gpService.GameProfile.Base64AppIcon;
    public uint GetGameProfileSteamAppId() => _gpService.GameProfile.SteamAppId;
    private string GetGameUrl() => $"https://store.steampowered.com/app/{GetGameProfileSteamAppId()}/";

    #endregion

    #region CONSTRUCTOR
    
    private CancellationTokenSource _cts = new();
    private static ISimpleMediator _mediator = null!;
    private readonly AutoStrongDeencryptor _deencryptor = new();
    
    /// <summary>
    /// Constructs new <see cref="Core"/> class.
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="pText"></param>
    /// <param name="pValue"></param>
    /// <param name="logger"></param>
    public Core(ISimpleMediator mediator, IProgress<string> pText, IProgress<int> pValue, SimpleLogger logger)
    {
        _mediator = mediator;
        _pText = pText;
        _pValue = pValue;
        _logger = logger;
        InputDirectory = RootPath;
        InitializeComponent();
    }

    /// <summary>
    /// Initialize component.
    /// </summary>
    private static void InitializeComponent()
    {
        // create directories
        CreateDirectories();
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
    private void ExtractSteamIdFromPathIfValid(string directoryPath)
    {
        var match = Regex.Match(directoryPath, PathPattern);
        if (match.Success) SetSteamId(match.Groups[1].Value, true);
    }

    #endregion

    #region INPUT_DIRECTORY

    public string InputDirectory { get; private set; }

    /// <summary>
    /// Validates a <paramref name="inputPath"/> string and sets the <see cref="InputDirectory"/> property.
    /// </summary>
    /// <param name="inputPath"></param>
    public void SetInputDirectory(string inputPath)
    {
        if (IsBusy) return;

        // checking for forbidden directory
        if (IsForbiddenDirectory(BackupPath)) return;
        
        // checking if directory exists
        var result = Directory.Exists(inputPath);
        if (result)
        {
            InputDirectory = inputPath;
            ExtractSteamIdFromPathIfValid(inputPath);
        }
        ReportProgress(result ? "The entered Input Folder Path is correct." : "The entered Input Folder Path was invalid.", 0);
        return;

        bool IsForbiddenDirectory(string path)
        {
            if (!inputPath.Contains(path)) return false;
            _mediator.Inform($"The entered path:\n\"{path}\", \ncannot be used as the Input Folder Path. \nThe path has not been updated.", "Forbidden directory", DialogType.Exclamation);
            return true;
        }
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

    private enum OperationType
    {
        Encryption,
        Decryption,
        Resigning
    }

    public Task DecryptAllAsync()
        => ProcessAsyncOperation(OperationType.Decryption, DecryptAll);

    public Task EncryptAllAsync()
        => ProcessAsyncOperation(OperationType.Encryption, EncryptAll);

    public Task ResignAllAsync()
        => ProcessAsyncOperation(OperationType.Resigning, ResignAll);

    private async Task ProcessAsyncOperation(OperationType operationType, OperationDelegate operationDelegate)
    {
        if (IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        try
        {
            _logger.Log(LogSeverity.Information, $"{operationType} has started. Selected game profile: {_gpService.GameProfile.GameTitle}.");
            _logger.Log(LogSeverity.Information, $"Provided Steam32_ID: {SteamId}");
            _logger.Log(LogSeverity.Information, "ID | FileName | MD5_Checksum | IsEncrypted | Steam32_ID");
            await AsyncOperation(operationType, operationDelegate);
            _logger.Log(LogSeverity.Information, $"{operationType} complete.");
        }
        catch (OperationCanceledException)
        {
            var message = $"{operationType} was interrupted by the user.";
            _pText.Report(message);
            _logger.Log(LogSeverity.Warning, message);
        }
        AbortOperation();
    }

    private Task AsyncOperation(OperationType operationType, OperationDelegate operationDelegate)
    {
        return Task.Run(() =>
        {
            // check if input directory exists
            if (!DoesDirectoryExists(InputDirectory)) return;
            
            // get the paths of "*.bin" files located in the input directory
            var files = Directory.GetFiles(InputDirectory, "*.bin", SearchOption.TopDirectoryOnly);

            // initialize backup
            SimpleBackup simpleBackup = new(BackupPath);
            if (files.Length > 0) simpleBackup.NewBackup();

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var processedFiles = 0;
            var progress = 0;
            ReportProgress($"[{progress}/{files.Length}] Processing files...", progress);
            
            Parallel.For((long)0, files.Length, po, ctr =>
            {
                // try to load file
                var dsssFile = new DsssAutoStrongFile(_deencryptor);
                var result = dsssFile.SetFileData(files[ctr]);
                if (!result.Result)
                {
                    _logger.Log(LogSeverity.Error, $"I_{ctr} -> {result.Description}");
                    goto ORDER_66;
                }
                
                // check file integrity
                result = dsssFile.DataHeader.CheckIntegrity(_deencryptor);
                if (!result.Result)
                {
                    _logger.Log(LogSeverity.Error, $"I_{ctr} -> {result.Description}");
                    goto ORDER_66;
                }
                
                // log info about the input file
                if (_logger is { IsEnabled: true, IsSilent: false }) 
                    _logger.Log(LogSeverity.Information, $"I_{ctr} | {Path.GetFileName(files[ctr])} | {Md5HashFromFile(files[ctr])} | {dsssFile.IsEncrypted()} | {dsssFile.DataHeader.GetSteamId(_deencryptor)}");

                // check operation type and adjust to it
                switch (operationType)
                {
                    case OperationType.Resigning:
                        break;
                    case OperationType.Decryption:
                        if (!dsssFile.IsEncrypted())
                        {
                            _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The file is already decrypted.");
                            goto ORDER_66;
                        }
                        break;
                    case OperationType.Encryption:
                    default:
                        if (dsssFile.IsEncrypted())
                        {
                            _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The file is already encrypted.");
                            goto ORDER_66;
                        }
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

                // log info about the output file
                if (_logger is { IsEnabled: true, IsSilent: false })
                    _logger.Log(LogSeverity.Information, $"O_{ctr} | {Path.GetFileName(files[ctr])} | {Md5HashFromFile(files[ctr])} | {dsssFile.IsEncrypted()} | {dsssFile.DataHeader.GetSteamId(_deencryptor)}");

                ORDER_66:
                Interlocked.Increment(ref progress);
                ReportProgress($"[{progress}/{files.Length}] Processing files...", (int)((double)progress / files.Length * 100));
            });
            simpleBackup.FinalizeBackup();

            var message = $"{operationType} done. Number of processed files: {processedFiles}.";
            _logger.Log(LogSeverity.Information, message);
            ReportProgress(message, 100);
        });
    }
    private static void DecryptAll(DsssAutoStrongFile dsssFile)
    {
        dsssFile.DecryptDataHeader();
        dsssFile.DecryptData();
    }

    private static void EncryptAll(DsssAutoStrongFile dsssFile)
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
    
    #endregion
}