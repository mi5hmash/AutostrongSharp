using System.Media;
using AutostrongSharp.Helpers;
using AutostrongSharpCore;
using AutostrongSharpCore.Helpers;
using static AutostrongSharpCore.Helpers.SimpleLogger;

namespace AutostrongSharp;

public partial class Form1 : Form
{
    // Program Core
    private readonly Core _programCore;

    public Form1()
    {
        var mediator = new SimpleMediatorWinForms();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pValue = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _programCore = new Core(mediator, pText, pValue, new SimpleLogger(new SimpleLoggerOptions(AppInfo.RootPath)
        {
            MaxLogFiles = 1,
            MinSeverityLevel = LogSeverity.Information,
            LoggedAppName = $"{AppInfo.Title} v{AppInfo.Version}"
        }));
        _programCore.ActivateLogger();

        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // set controls
        versionLabel.Text = $@"v{AppInfo.Version}";
        authorLabel.Text = $@"{AppInfo.Author} 2024";
        TBFilepath.Text = AppInfo.RootPath;
        TBSteamId.Text = @"0";
        backupCheckBox.Checked = _programCore.BackupEnabled;

        // transparent picture hack
        pb_AppIcon.Controls.Add(pb_GameProfileIcon);
        pb_GameProfileIcon.Location = new Point(0, pb_AppIcon.Height - pb_GameProfileIcon.Height);

        // populate the GameProfile ComboBox
        PopulateGameProfileComboBox();
    }

    #region SUPER_USER

    // Super User
    private const int SuperUserThreshold = 3;
    private bool _isSuperUser;
    private int _superUserClicks;

    private void VersionLabel_Click(object sender, EventArgs e)
    {
        if (_isSuperUser) return;

        _superUserClicks += 1;

        if (_superUserClicks >= SuperUserThreshold) return;

        // restart superUserTimer
        superUserTimer.Stop();
        superUserTimer.Start();
    }

    private void SuperUserTimer_Tick(object sender, EventArgs e)
    {
        superUserTimer.Stop();
        if (_superUserClicks >= SuperUserThreshold) EnableSuperUser();
        _superUserClicks = 0;
    }

    private void EnableSuperUser()
    {
        _isSuperUser = true;
        // things to unlock
        ButtonEncryptAll.Visible = true;
        ButtonDecryptAll.Visible = true;
        // play sound
        SystemSounds.Beep.Play();
    }

    #endregion

    #region GAME_PROFILE

    private void PopulateGameProfileComboBox()
    {
        comboBoxGameProfile.DataSource = _programCore.GetGameProfileFileList();
        comboBoxGameProfile.DisplayMember = "Name";
    }

    private void ComboBoxGameProfile_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        _programCore.SetNewGameProfile(comboBoxGameProfile.SelectedIndex);
        SetGameProfileIcon();
        comboBox.SelectedIndexChanged -= ComboBoxGameProfile_SelectedIndexChanged!;
        comboBox.SelectedIndex = _programCore.GetSelectedGameProfileFileIndex();
        comboBox.SelectedIndexChanged += ComboBoxGameProfile_SelectedIndexChanged!;
    }

    private void SetGameProfileIcon()
    {
        pb_GameProfileIcon.Visible = true;
        using MemoryStream ms = new(Convert.FromBase64String(_programCore.GetGameProfileIcon()));
        pb_GameProfileIcon.Image = Image.FromStream(ms);
        var appId = $"AppID: {_programCore.GetGameProfileSteamAppId()}";
        toolTip1.SetToolTip(pb_GameProfileIcon, appId);
    }

    private void Pb_GameProfileIcon_Click(object sender, EventArgs e)
        => _programCore.VisitGameWebsite();

    #endregion

    #region BACKUP

    private void BackupCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        _programCore.SetBackupEnabled(checkBox.Checked);
        // detach event to prevent stack overflow
        checkBox.CheckedChanged -= BackupCheckBox_CheckedChanged!;
        // update checkBox state
        checkBox.Checked = _programCore.BackupEnabled;
        // re-attach the event
        checkBox.CheckedChanged += BackupCheckBox_CheckedChanged!;
    }

    private void ButtonOpenBackupDir_Click(object sender, EventArgs e)
        => Core.OpenBackupDirectory();

    #endregion

    #region STEAM_ID

    private void TBSteamId_Leave(object sender, EventArgs e)
    {
        if (sender is not TextBox textBox) return;
        _programCore.SetSteamId(textBox.Text);
        textBox.Text = _programCore.SteamId.ToString();
    }

    #endregion

    #region INPUT_PATH

    private void ValidatePath(object sender)
    {
        if (sender is not TextBox textBox) return;
        _programCore.SetInputDirectory(textBox.Text);
        textBox.Text = _programCore.InputDirectory;
        TBSteamId.Text = _programCore.SteamId.ToString();
    }

    private void TBFilepath_Leave(object sender, EventArgs e)
        => ValidatePath(sender);

    private void TBFilepath_DragDrop(object sender, DragEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        var filePath = filePaths[0];
        if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
            filePath = Path.GetDirectoryName(filePath);
        textBox.Text = filePath;
        ValidatePath(textBox);
    }

    private void TBFilepath_DragOver(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.Copy;

    private void ButtonSelectDir_Click(object sender, EventArgs e)
    {
        if (_programCore.IsBusy) return;
        if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
        TBFilepath.Text = folderBrowserDialog1.SelectedPath;
        ValidatePath(TBFilepath);
    }

    #endregion

    #region OPERATIONS

    private void AuthorLabel_Click(object sender, EventArgs e)
        => AppInfo.VisitAuthorsGithub();

    private void ButtonAbort_Click(object sender, EventArgs e)
        => _programCore.AbortOperation();

    private delegate Task OperationDelegate();

    private async Task ProcessAsyncOperation(OperationDelegate operationDelegate)
    {
        if (_programCore.IsBusy) return;

        ButtonAbort.Visible = true;
        await operationDelegate();
        ButtonAbort.Visible = false;

        // play sound
        SoundPlayer sp = new(Properties.Resources.typewritter_machine);
        sp.Play();
    }

    private async void ButtonDecryptAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.DecryptAllAsync);

    private async void ButtonEncryptAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.EncryptAllAsync);

    private async void ButtonResignAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.ResignAllAsync);

    #endregion
    
}