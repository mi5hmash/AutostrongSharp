using System.Diagnostics;
using AutostrongSharp.Helpers;
using AutostrongSharpCore.Helpers;
using AutostrongSharpCore.Models;
using AutostrongSharpCore.Models.DSSS.AutoStrong;
using System.Text.RegularExpressions;

namespace AutostrongSharp;

public partial class Form1 : Form
{
    private readonly AutoStrongDeencryptor _deencryptor = new();
    private DsssGameProfileService _gameProfile = new();
    private CancellationTokenSource _cts = new();
    private bool _isBusy;
    private int _gameProfileIndex;

    private List<ProfileFile> _dataSource = new();

    private static string PathPattern => @$"\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}remote\{Path.DirectorySeparatorChar}win64_save\{Path.DirectorySeparatorChar}?$";

    private void ExtractSteamIdFromPathIfValid()
    {
        var match = Regex.Match(TBFilepath.Text, PathPattern);
        if (!match.Success) return;
        TBSteamId.Text = match.Groups[1].Value;
    }

    private void ValidateSteamId()
    {
        TBSteamId.Text = SteamIdFixer(TBSteamId.Text);
        return;

        static string SteamIdFixer(string textBoxText) => ulong.TryParse(textBoxText, out var result) ? ((uint)result).ToString() : "0";
    }

    private void ResetToolStrip()
    {
        toolStripProgressBar1.Value = 0;
        toolStripStatusLabel1.Text = @"Ready";
    }

    private bool DoesInputDirectoryExists()
    {
        if (Directory.Exists(TBFilepath.Text)) return true;
        MessageBox.Show($"""Directory: "{TBFilepath.Text}" does not exists.""", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    private void PopulateGameProfileComboBox()
    {
        _dataSource = Directory.GetFiles(AppInfo.ProfilesPath, "*.bin", SearchOption.TopDirectoryOnly).Select(file => new ProfileFile(file)).ToList();
        comboBoxGameProfile.DataSource = _dataSource;
        comboBoxGameProfile.DisplayMember = "Name";
    }

    private void SetGameProfileIcon()
    {
        pb_GameProfileIcon.Visible = true;
        using MemoryStream ms = new(Convert.FromBase64String(_gameProfile.Base64AppIcon));
        pb_GameProfileIcon.Image = Image.FromStream(ms);
        var appId = $"AppID: {_gameProfile.SteamAppId}";
        toolTip1.SetToolTip(pb_GameProfileIcon, appId);
    }

    private bool WriteBytesToFile(string filePath, Span<byte> fileData)
    {
        do
        {
            if (TryWriteAllBytes(filePath, fileData)) return true;
            // Ask the user if they want to try again
            var dialogResult = MessageBox.Show($"""Failed to save the file: "{filePath}".{Environment.NewLine}It may be currently in use by another program.{Environment.NewLine}Would you like to try again?""", @"Failed to save the file", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.No) return false;
        } while (true);

        bool TryWriteAllBytes(string fPath, Span<byte> bytes)
        {
            try
            {
                File.WriteAllBytes(fPath, bytes.ToArray());
            }
            catch { return false; }
            return true;
        }
    }

    private static void CreateDirectories()
    {
        Directory.CreateDirectory(AppInfo.BackupPath);
    }

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // create directories
        Directory.CreateDirectory(AppInfo.ProfilesPath);
        CreateDirectories();

        // set controls
        TBFilepath.Text = AppInfo.RootPath;
        backupCheckBox.Checked = AppInfo.GetBackupEnabled();
        versionLabel.Text = $@"v{AppInfo.Version}";
        authorLabel.Text = $@"{AppInfo.Author} 2023";

        // transparent picture hack
        pb_AppIcon.Controls.Add(pb_GameProfileIcon);
        pb_GameProfileIcon.Location = new Point(0, pb_AppIcon.Height - pb_GameProfileIcon.Height);
        PopulateGameProfileComboBox();
    }

    private void comboBoxGameProfile_SelectedIndexChanged(object sender, EventArgs e)
    {
        var senderObj = (ComboBox)sender;
        if (_isBusy)
        {
            senderObj.SelectedIndex = _gameProfileIndex;
            return;
        }
        _gameProfile = new DsssGameProfileService();
        var result = _gameProfile.LoadGameProfile(_dataSource[comboBoxGameProfile.SelectedIndex].FullPath, _deencryptor);
        toolStripStatusLabel1.Text = result.Description;
        _gameProfileIndex = senderObj.SelectedIndex;
        SetGameProfileIcon();
    }

    private void pb_GameProfileIcon_Click(object sender, EventArgs e)
        => Process.Start(new ProcessStartInfo { FileName = $"https://store.steampowered.com/app/{_gameProfile.SteamAppId}/", UseShellExecute = true });

    private void backupCheckBox_CheckedChanged(object sender, EventArgs e)
        => AppInfo.ToggleBackupEnabled();

    private void ButtonSelectDir_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) TBFilepath.Text = folderBrowserDialog1.SelectedPath;
    }

    private void ButtonOpenBackupDir_Click(object sender, EventArgs e)
        => IoHelpers.OpenDirectory(Path.Combine(AppInfo.RootPath, AppInfo.BackupPath));

    private void TBFilepath_TextChanged(object sender, EventArgs e)
    {
        ResetToolStrip();
        ExtractSteamIdFromPathIfValid();
    }

    private void TBFilepath_DragDrop(object sender, DragEventArgs e)
    {
        if (!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        var filePath = filePaths[0];
        if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
            filePath = Path.GetDirectoryName(filePath);
        TBFilepath.Text = filePath;
    }

    private void TBFilepath_DragOver(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.Copy;

    private void ButtonAbort_Click(object sender, EventArgs e) => AbortOperation();
    private void AbortOperation()
    {
        _cts.Cancel();
        _cts.Dispose();
        ButtonAbort.Visible = false;
        _isBusy = false;
    }

    private async void ButtonDecryptAll_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _cts = new CancellationTokenSource();
        ButtonAbort.Visible = true;

        try
        {
            await DecryptAll(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task DecryptAll(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var decryptedFiles = 0;

            var files = Directory.GetFiles(TBFilepath.Text);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            // initialize backup
            BackupLogic backup = new(AppInfo.BackupPath);
            if (files.Length > 0) backup.NewBackup();

            var progress = 0;
            pText.Report($@"[{progress}/{files.Length}] Processing files...");
            pPercentage.Report(progress);
            Parallel.For((long)0, files.Length, po, (ctr) =>
            {
                // load file
                var dsssFile = new DsssAutoStrongFile(_deencryptor);
                var result = dsssFile.SetFileData(files[ctr]);
                if (!result.Result || !dsssFile.IsEncrypted()) goto ORDER_66;

                // backup file
                if (backupCheckBox.Checked) backup.Backup(files[ctr]);

                // decrypt file
                dsssFile.DecryptDataHeader();
                dsssFile.DecryptData();

                // save file
                var fileData = dsssFile.GetFileData();
                var writeResult = WriteBytesToFile(files[ctr], fileData);

                if (writeResult) decryptedFiles++;
                ORDER_66:
                Interlocked.Increment(ref progress);
                pText.Report($@"[{progress}/{files.Length}] Processing files...");
                pPercentage.Report((int)((double)progress / files.Length * 100));

            });

            // delete backup folder if no files were processed
            if (decryptedFiles == 0) backup.DeleteCurrentBackup();

            pText.Report($@"Decryption done. Number of decrypted files: {decryptedFiles}.");
            pPercentage.Report(100);
        });
    }

    private async void ButtonEncryptAll_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _cts = new CancellationTokenSource();
        ButtonAbort.Visible = true;

        try
        {
            await EncryptAll(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task EncryptAll(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var encryptedFiles = 0;

            var files = Directory.GetFiles(TBFilepath.Text);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            // initialize backup
            BackupLogic backup = new(AppInfo.BackupPath);
            if (files.Length > 0) backup.NewBackup();

            var progress = 0;
            pText.Report($@"[{progress}/{files.Length}] Processing files...");
            pPercentage.Report(progress);
            Parallel.For((long)0, files.Length, po, (ctr) =>
            {
                // load file
                var dsssFile = new DsssAutoStrongFile(_deencryptor);
                var result = dsssFile.SetFileData(files[ctr]);
                if (!result.Result || dsssFile.IsEncrypted()) goto ORDER_66;

                // backup file
                if (backupCheckBox.Checked) backup.Backup(files[ctr]);

                // encrypt file
                dsssFile.EncryptDataHeader();
                dsssFile.EncryptData();

                // save file
                var fileData = dsssFile.GetFileData();
                var writeResult = WriteBytesToFile(files[ctr], fileData);

                if (writeResult) encryptedFiles++;
                ORDER_66:
                Interlocked.Increment(ref progress);
                pText.Report($@"[{progress}/{files.Length}] Processing files...");
                pPercentage.Report((int)((double)progress / files.Length * 100));

            });

            // delete backup folder if no files were processed
            if (encryptedFiles == 0) backup.DeleteCurrentBackup();

            pText.Report($@"Encryption done. Number of encrypted files: {encryptedFiles}.");
            pPercentage.Report(100);
        });
    }

    private async void ButtonResignAll_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        ValidateSteamId();
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _cts = new CancellationTokenSource();
        ButtonAbort.Visible = true;

        try
        {
            await ResignAll(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task ResignAll(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var resignedFiles = 0;

            var files = Directory.GetFiles(TBFilepath.Text);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            // initialize backup
            BackupLogic backup = new(AppInfo.BackupPath);
            if (files.Length > 0) backup.NewBackup();

            var progress = 0;
            pText.Report($@"[{progress}/{files.Length}] Processing files...");
            pPercentage.Report(progress);
            Parallel.For((long)0, files.Length, po, (ctr) =>
            {
                // load file
                var dsssFile = new DsssAutoStrongFile(_deencryptor);
                var result = dsssFile.SetFileData(files[ctr]);
                if (!result.Result) goto ORDER_66;

                // backup file
                if (backupCheckBox.Checked) backup.Backup(files[ctr]);

                // resign file
                if (dsssFile.IsEncrypted())
                {
                    dsssFile.DecryptDataHeader();
                    dsssFile.DataHeader.Steam32Id = uint.TryParse(TBSteamId.Text, out var steamId) ? steamId : 0;
                    dsssFile.EncryptDataHeader();
                }
                else
                {
                    dsssFile.DataHeader.Steam32Id = uint.TryParse(TBSteamId.Text, out var steamId) ? steamId : 0;
                    dsssFile.EncryptDataHeader();
                    dsssFile.EncryptData();
                }

                // save file
                var fileData = dsssFile.GetFileData();
                var writeResult = WriteBytesToFile(files[ctr], fileData);

                if (writeResult) resignedFiles++;
                ORDER_66:
                Interlocked.Increment(ref progress);
                pText.Report($@"[{progress}/{files.Length}] Processing files...");
                pPercentage.Report((int)((double)progress / files.Length * 100));

            });

            // delete backup folder if no files were processed
            if (resignedFiles == 0) backup.DeleteCurrentBackup();

            pText.Report($@"Resigning done. Number of resigned files: {resignedFiles}.");
            pPercentage.Report(100);
        });
    }
}