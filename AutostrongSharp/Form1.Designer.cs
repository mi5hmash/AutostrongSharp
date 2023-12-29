namespace AutostrongSharp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            ButtonAbort = new Button();
            ButtonSelectDir = new Button();
            authorLabel = new Label();
            versionLabel = new Label();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            statusStrip1 = new StatusStrip();
            ButtonResignAll = new Button();
            LabelSteamId = new Label();
            LabelFilepath = new Label();
            pb_AppIcon = new PictureBox();
            ButtonPackAll = new Button();
            ButtonUnpackAll = new Button();
            TBFilepath = new TextBox();
            folderBrowserDialog1 = new FolderBrowserDialog();
            TBSteamId = new TextBox();
            pb_GameProfileIcon = new PictureBox();
            comboBoxGameProfile = new ComboBox();
            LabelGameProfile = new Label();
            toolTip1 = new ToolTip(components);
            ButtonOpenBackupDir = new Button();
            backupCheckBox = new CheckBox();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_AppIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pb_GameProfileIcon).BeginInit();
            SuspendLayout();
            // 
            // ButtonAbort
            // 
            ButtonAbort.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonAbort.ForeColor = Color.Brown;
            ButtonAbort.Location = new Point(295, 124);
            ButtonAbort.Name = "ButtonAbort";
            ButtonAbort.Size = new Size(55, 23);
            ButtonAbort.TabIndex = 10;
            ButtonAbort.Text = "ABORT";
            ButtonAbort.UseVisualStyleBackColor = true;
            ButtonAbort.Visible = false;
            ButtonAbort.Click += ButtonAbort_Click;
            // 
            // ButtonSelectDir
            // 
            ButtonSelectDir.AllowDrop = true;
            ButtonSelectDir.Location = new Point(340, 29);
            ButtonSelectDir.Name = "ButtonSelectDir";
            ButtonSelectDir.Size = new Size(30, 23);
            ButtonSelectDir.TabIndex = 2;
            ButtonSelectDir.Text = "📁";
            toolTip1.SetToolTip(ButtonSelectDir, "Open the Select Directory Window");
            ButtonSelectDir.UseVisualStyleBackColor = true;
            ButtonSelectDir.Click += ButtonSelectDir_Click;
            ButtonSelectDir.DragDrop += TBFilepath_DragDrop;
            ButtonSelectDir.DragOver += TBFilepath_DragOver;
            // 
            // authorLabel
            // 
            authorLabel.AutoSize = true;
            authorLabel.Font = new Font("Segoe UI", 7F);
            authorLabel.Location = new Point(423, 143);
            authorLabel.Name = "authorLabel";
            authorLabel.Size = new Size(75, 12);
            authorLabel.TabIndex = 35;
            authorLabel.Text = "Mi5hmasH 2023";
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Location = new Point(452, 128);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(46, 15);
            versionLabel.TabIndex = 34;
            versionLabel.Text = "v1.1.0.0";
            versionLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(39, 17);
            toolStripStatusLabel1.Text = "Ready";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(60, 16);
            toolStripProgressBar1.Step = 5;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBar1, toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 163);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(510, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 33;
            statusStrip1.Text = "statusStrip1";
            // 
            // ButtonResignAll
            // 
            ButtonResignAll.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonResignAll.Location = new Point(200, 124);
            ButtonResignAll.Name = "ButtonResignAll";
            ButtonResignAll.Size = new Size(89, 23);
            ButtonResignAll.TabIndex = 9;
            ButtonResignAll.Text = "RESIGN ALL";
            ButtonResignAll.UseVisualStyleBackColor = true;
            ButtonResignAll.Click += ButtonResignAll_Click;
            // 
            // LabelSteamId
            // 
            LabelSteamId.AutoSize = true;
            LabelSteamId.Location = new Point(12, 65);
            LabelSteamId.Name = "LabelSteamId";
            LabelSteamId.Size = new Size(66, 15);
            LabelSteamId.TabIndex = 25;
            LabelSteamId.Text = "Steam32 ID";
            // 
            // LabelFilepath
            // 
            LabelFilepath.AutoSize = true;
            LabelFilepath.Location = new Point(12, 11);
            LabelFilepath.Name = "LabelFilepath";
            LabelFilepath.Size = new Size(98, 15);
            LabelFilepath.TabIndex = 23;
            LabelFilepath.Text = "Input Folder Path";
            // 
            // pb_AppIcon
            // 
            pb_AppIcon.Image = Properties.Resources.logo;
            pb_AppIcon.Location = new Point(386, 10);
            pb_AppIcon.Name = "pb_AppIcon";
            pb_AppIcon.Size = new Size(112, 117);
            pb_AppIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pb_AppIcon.TabIndex = 20;
            pb_AppIcon.TabStop = false;
            // 
            // ButtonPackAll
            // 
            ButtonPackAll.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonPackAll.Location = new Point(104, 124);
            ButtonPackAll.Name = "ButtonPackAll";
            ButtonPackAll.Size = new Size(89, 23);
            ButtonPackAll.TabIndex = 8;
            ButtonPackAll.Text = "ENCRYPT ALL";
            ButtonPackAll.UseVisualStyleBackColor = true;
            ButtonPackAll.Click += ButtonEncryptAll_Click;
            // 
            // ButtonUnpackAll
            // 
            ButtonUnpackAll.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            ButtonUnpackAll.Location = new Point(12, 124);
            ButtonUnpackAll.Name = "ButtonUnpackAll";
            ButtonUnpackAll.Size = new Size(89, 23);
            ButtonUnpackAll.TabIndex = 7;
            ButtonUnpackAll.Text = "DECRYPT ALL";
            ButtonUnpackAll.UseVisualStyleBackColor = true;
            ButtonUnpackAll.Click += ButtonDecryptAll_Click;
            // 
            // TBFilepath
            // 
            TBFilepath.AllowDrop = true;
            TBFilepath.Location = new Point(12, 29);
            TBFilepath.Name = "TBFilepath";
            TBFilepath.Size = new Size(322, 23);
            TBFilepath.TabIndex = 1;
            TBFilepath.DragDrop += TBFilepath_DragDrop;
            TBFilepath.DragOver += TBFilepath_DragOver;
            TBFilepath.Leave += TBFilepath_Leave;
            // 
            // TBSteamId
            // 
            TBSteamId.Location = new Point(12, 83);
            TBSteamId.Name = "TBSteamId";
            TBSteamId.Size = new Size(118, 23);
            TBSteamId.TabIndex = 3;
            TBSteamId.Leave += TBSteamId_Leave;
            // 
            // pb_GameProfileIcon
            // 
            pb_GameProfileIcon.BackColor = Color.Transparent;
            pb_GameProfileIcon.Cursor = Cursors.Hand;
            pb_GameProfileIcon.Location = new Point(386, 95);
            pb_GameProfileIcon.Name = "pb_GameProfileIcon";
            pb_GameProfileIcon.Size = new Size(32, 32);
            pb_GameProfileIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pb_GameProfileIcon.TabIndex = 36;
            pb_GameProfileIcon.TabStop = false;
            toolTip1.SetToolTip(pb_GameProfileIcon, "AppID: ?");
            pb_GameProfileIcon.Visible = false;
            pb_GameProfileIcon.Click += Pb_GameProfileIcon_Click;
            // 
            // comboBoxGameProfile
            // 
            comboBoxGameProfile.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxGameProfile.FormattingEnabled = true;
            comboBoxGameProfile.Location = new Point(136, 83);
            comboBoxGameProfile.Name = "comboBoxGameProfile";
            comboBoxGameProfile.Size = new Size(234, 23);
            comboBoxGameProfile.Sorted = true;
            comboBoxGameProfile.TabIndex = 4;
            comboBoxGameProfile.SelectedIndexChanged += ComboBoxGameProfile_SelectedIndexChanged;
            // 
            // LabelGameProfile
            // 
            LabelGameProfile.AutoSize = true;
            LabelGameProfile.Location = new Point(136, 65);
            LabelGameProfile.Name = "LabelGameProfile";
            LabelGameProfile.Size = new Size(75, 15);
            LabelGameProfile.TabIndex = 38;
            LabelGameProfile.Text = "Game Profile";
            // 
            // ButtonOpenBackupDir
            // 
            ButtonOpenBackupDir.ForeColor = SystemColors.ControlDarkDark;
            ButtonOpenBackupDir.Location = new Point(340, 57);
            ButtonOpenBackupDir.Name = "ButtonOpenBackupDir";
            ButtonOpenBackupDir.Size = new Size(30, 23);
            ButtonOpenBackupDir.TabIndex = 6;
            ButtonOpenBackupDir.Text = "⚙";
            toolTip1.SetToolTip(ButtonOpenBackupDir, "Open the _BACKUP directory");
            ButtonOpenBackupDir.UseVisualStyleBackColor = true;
            ButtonOpenBackupDir.Click += ButtonOpenBackupDir_Click;
            // 
            // backupCheckBox
            // 
            backupCheckBox.AutoSize = true;
            backupCheckBox.Font = new Font("Segoe UI", 8.25F);
            backupCheckBox.Location = new Point(272, 61);
            backupCheckBox.Name = "backupCheckBox";
            backupCheckBox.Size = new Size(66, 17);
            backupCheckBox.TabIndex = 5;
            backupCheckBox.Text = "BACKUP";
            backupCheckBox.UseVisualStyleBackColor = true;
            backupCheckBox.CheckedChanged += BackupCheckBox_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(510, 185);
            Controls.Add(ButtonOpenBackupDir);
            Controls.Add(backupCheckBox);
            Controls.Add(LabelGameProfile);
            Controls.Add(comboBoxGameProfile);
            Controls.Add(pb_GameProfileIcon);
            Controls.Add(ButtonAbort);
            Controls.Add(authorLabel);
            Controls.Add(versionLabel);
            Controls.Add(statusStrip1);
            Controls.Add(ButtonResignAll);
            Controls.Add(LabelSteamId);
            Controls.Add(LabelFilepath);
            Controls.Add(pb_AppIcon);
            Controls.Add(ButtonPackAll);
            Controls.Add(ButtonUnpackAll);
            Controls.Add(ButtonSelectDir);
            Controls.Add(TBFilepath);
            Controls.Add(TBSteamId);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "Autostrong Sharp";
            Load += Form1_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_AppIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)pb_GameProfileIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button ButtonAbort;
        private Button ButtonSelectDir;
        private Label authorLabel;
        private Label versionLabel;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripProgressBar toolStripProgressBar1;
        private StatusStrip statusStrip1;
        private Button ButtonResignAll;
        private Label LabelSteamId;
        private Label LabelFilepath;
        private PictureBox pb_AppIcon;
        private Button ButtonPackAll;
        private Button ButtonUnpackAll;
        private TextBox TBFilepath;
        private FolderBrowserDialog folderBrowserDialog1;
        private TextBox TBSteamId;
        private PictureBox pb_GameProfileIcon;
        private ComboBox comboBoxGameProfile;
        private Label LabelGameProfile;
        private ToolTip toolTip1;
        private CheckBox backupCheckBox;
        private Button ButtonOpenBackupDir;
    }
}