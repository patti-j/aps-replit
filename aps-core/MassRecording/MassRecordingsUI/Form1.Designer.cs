using DevExpress.XtraEditors.Controls;
using PT.ComponentLibrary.Controls;

namespace MassRecordingsUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelControl_Title = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_Install = new DevExpress.XtraEditors.SimpleButton();
            this.tabPane_MRUI = new DevExpress.XtraBars.Navigation.TabPane();
            this.tabNavigationPage_Install = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.groupControl3 = new DevExpress.XtraEditors.GroupControl();
            this.groupControl_RadioGroup = new DevExpress.XtraEditors.GroupControl();
            this.checkEdit_SelectRecent = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit_SelectNoErrors = new DevExpress.XtraEditors.CheckEdit();
            this.labelControl_LatestBaseId = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_CurrentBaseId = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_UpdateBaseId = new DevExpress.XtraEditors.SimpleButton();
            this.imageComboBoxEdit_LatestBaseId = new DevExpress.XtraEditors.ImageComboBoxEdit();
            this.textEdit_CurrentBaseId = new DevExpress.XtraEditors.TextEdit();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.labelControl_dbConnection = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_InstalledNote = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_HostName = new DevExpress.XtraEditors.LabelControl();
            this.tabNavigationPage_Config = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.labelControl_maxProcessRestartCount = new DevExpress.XtraEditors.LabelControl();
            this.spinEdit_MaxRestartCount = new ScrollableSpinEdit();
            this.spinEdit_PlayerTimeOut = new ScrollableSpinEdit();
            this.labelControl_PlayerTimeOut = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_ConfigName = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_Delete = new DevExpress.XtraEditors.SimpleButton();
            this.buttonEdit_KeyFolderPath = new DevExpress.XtraEditors.ButtonEdit();
            this.simpleButton_SaveConfiguration = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl_KeyFolderPath = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_UseThisConfig = new DevExpress.XtraEditors.SimpleButton();
            this.textEdit_ConfigName = new DevExpress.XtraEditors.TextEdit();
            this.buttonEdit_RecordingsDir = new DevExpress.XtraEditors.ButtonEdit();
            this.toggleSwitch_LoadCustomization = new DevExpress.XtraEditors.ToggleSwitch();
            this.labelControl_RecordingsDir = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_LoadCustomization = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_MasterCopyPath = new DevExpress.XtraEditors.LabelControl();
            this.buttonEdit_MasterCopyPath = new DevExpress.XtraEditors.ButtonEdit();
            this.simpleButton_AddNewConfig = new DevExpress.XtraEditors.SimpleButton();
            this.imageComboBoxEdit_ConfigTitle = new DevExpress.XtraEditors.ImageComboBoxEdit();
            this.labelControl_ConfigInfoMessage = new DevExpress.XtraEditors.LabelControl();
            this.tabNavigationPage_Analysis = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.labelControl_Progress = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_NumberPlayersRun = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_NumberPlayersRun = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_RunLocation = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_RunLocation = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_EndTime = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_EndTime = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_StartTime = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_StartTime = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_AnalysisMessage = new DevExpress.XtraEditors.LabelControl();
            this.progressBarControl_CompletedPlayers = new DevExpress.XtraEditors.ProgressBarControl();
            this.textEdit_NumberPlayersRemaining = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_NumberPlayerRemaining = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_NumberPlayersOn = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_NumberPlayersOn = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_EndProcesses = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl_Status = new DevExpress.XtraEditors.LabelControl();
            this.tabNavigationPage_Results = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.simpleButton_ShowWarnings = new DevExpress.XtraEditors.SimpleButton();
            this.marqueeProgressBarControl_GatherWarnings = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this.marqueeProgressBarControl_Loading = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this.memoEdit_TopRecordingsLength = new DevExpress.XtraEditors.MemoEdit();
            this.labelControl_TopRecordingsLength = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton_GatherWarnings = new DevExpress.XtraEditors.SimpleButton();
            this.memoEdit_TopRecordingsWarnings = new DevExpress.XtraEditors.MemoEdit();
            this.labelControl_TopRecordingsWarnings = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_NumWarnings = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_NumWarnings = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_PercentNoErrors = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_PercentNoErrors = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_NumPlayersWithErrors = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_NumPlayersWithErrors = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_ResultsTotalNumberPlayers = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_ResultsTotalNumberPlayers = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_ResultsNumberPlayersRun = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_ResultsNumberPlayersRun = new DevExpress.XtraEditors.LabelControl();
            this.tabNavigationPage1 = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit2 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit3 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit4 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.progressBarControl1 = new DevExpress.XtraEditors.ProgressBarControl();
            this.textEdit5 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit6 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_Results = new DevExpress.XtraEditors.LabelControl();
            this.imageComboBoxEdit_RunInstances = new DevExpress.XtraEditors.ImageComboBoxEdit();
            this.behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.tabPane_MRUI)).BeginInit();
            this.tabPane_MRUI.SuspendLayout();
            this.tabNavigationPage_Install.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).BeginInit();
            this.groupControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_RadioGroup)).BeginInit();
            this.groupControl_RadioGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit_SelectRecent.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit_SelectNoErrors.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageComboBoxEdit_LatestBaseId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_CurrentBaseId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            this.tabNavigationPage_Config.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit_MaxRestartCount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit_PlayerTimeOut.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_KeyFolderPath.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_ConfigName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_RecordingsDir.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.toggleSwitch_LoadCustomization.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_MasterCopyPath.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageComboBoxEdit_ConfigTitle.Properties)).BeginInit();
            this.tabNavigationPage_Analysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumberPlayersRun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_RunLocation.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_EndTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_StartTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl_CompletedPlayers.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumberPlayersRemaining.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumberPlayersOn.Properties)).BeginInit();
            this.tabNavigationPage_Results.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marqueeProgressBarControl_GatherWarnings.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marqueeProgressBarControl_Loading.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit_TopRecordingsLength.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit_TopRecordingsWarnings.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumWarnings.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_PercentNoErrors.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumPlayersWithErrors.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_ResultsTotalNumberPlayers.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_ResultsNumberPlayersRun.Properties)).BeginInit();
            this.tabNavigationPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit4.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit5.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit6.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageComboBoxEdit_RunInstances.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl_Title
            // 
            this.labelControl_Title.Appearance.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl_Title.Appearance.Options.UseFont = true;
            this.labelControl_Title.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelControl_Title.Location = new System.Drawing.Point(0, 0);
            this.labelControl_Title.Name = "labelControl_Title";
            this.labelControl_Title.Size = new System.Drawing.Size(401, 23);
            this.labelControl_Title.TabIndex = 0;
            this.labelControl_Title.Text = "Mass Recordings Setup and Configuration";
            // 
            // simpleButton_Install
            // 
            this.simpleButton_Install.Appearance.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.simpleButton_Install.Appearance.Options.UseFont = true;
            this.simpleButton_Install.Location = new System.Drawing.Point(12, 84);
            this.simpleButton_Install.Name = "simpleButton_Install";
            this.simpleButton_Install.Size = new System.Drawing.Size(162, 38);
            this.simpleButton_Install.TabIndex = 1;
            this.simpleButton_Install.Text = "Install";
            this.simpleButton_Install.Click += new System.EventHandler(this.simpleButtonInstall_Click);
            // 
            // tabPane_MRUI
            // 
            this.tabPane_MRUI.Controls.Add(this.tabNavigationPage_Install);
            this.tabPane_MRUI.Controls.Add(this.tabNavigationPage_Config);
            this.tabPane_MRUI.Controls.Add(this.tabNavigationPage_Analysis);
            this.tabPane_MRUI.Controls.Add(this.tabNavigationPage_Results);
            this.tabPane_MRUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPane_MRUI.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPane_MRUI.Location = new System.Drawing.Point(0, 23);
            this.tabPane_MRUI.Name = "tabPane_MRUI";
            this.tabPane_MRUI.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            this.tabNavigationPage_Install,
            this.tabNavigationPage_Config,
            this.tabNavigationPage_Analysis,
            this.tabNavigationPage_Results});
            this.tabPane_MRUI.RegularSize = new System.Drawing.Size(512, 571);
            this.tabPane_MRUI.SelectedPage = this.tabNavigationPage_Install;
            this.tabPane_MRUI.Size = new System.Drawing.Size(512, 571);
            this.tabPane_MRUI.TabIndex = 3;
            this.tabPane_MRUI.Text = "tabPane1";
            this.tabPane_MRUI.SelectedPageChanged += new DevExpress.XtraBars.Navigation.SelectedPageChangedEventHandler(this.tabPane_MRUI_SelectedPageChanged);
            // 
            // tabNavigationPage_Install
            // 
            this.tabNavigationPage_Install.Caption = "Setup and Install";
            this.tabNavigationPage_Install.Controls.Add(this.groupControl3);
            this.tabNavigationPage_Install.Controls.Add(this.groupControl2);
            this.tabNavigationPage_Install.Controls.Add(this.labelControl_HostName);
            this.tabNavigationPage_Install.Name = "tabNavigationPage_Install";
            this.tabNavigationPage_Install.Size = new System.Drawing.Size(512, 544);
            // 
            // groupControl3
            // 
            this.groupControl3.Controls.Add(this.groupControl_RadioGroup);
            this.groupControl3.Controls.Add(this.labelControl_LatestBaseId);
            this.groupControl3.Controls.Add(this.labelControl_CurrentBaseId);
            this.groupControl3.Controls.Add(this.simpleButton_UpdateBaseId);
            this.groupControl3.Controls.Add(this.imageComboBoxEdit_LatestBaseId);
            this.groupControl3.Controls.Add(this.textEdit_CurrentBaseId);
            this.groupControl3.Location = new System.Drawing.Point(17, 181);
            this.groupControl3.Name = "groupControl3";
            this.groupControl3.Size = new System.Drawing.Size(463, 349);
            this.groupControl3.TabIndex = 21;
            this.groupControl3.Text = "Base Session";
            // 
            // groupControl_RadioGroup
            // 
            this.groupControl_RadioGroup.Controls.Add(this.checkEdit_SelectRecent);
            this.groupControl_RadioGroup.Controls.Add(this.checkEdit_SelectNoErrors);
            this.groupControl_RadioGroup.Location = new System.Drawing.Point(5, 46);
            this.groupControl_RadioGroup.Name = "groupControl_RadioGroup";
            this.groupControl_RadioGroup.Size = new System.Drawing.Size(278, 75);
            this.groupControl_RadioGroup.TabIndex = 22;
            this.groupControl_RadioGroup.Text = "Base Session Options";
            // 
            // checkEdit_SelectRecent
            // 
            this.checkEdit_SelectRecent.Location = new System.Drawing.Point(5, 23);
            this.checkEdit_SelectRecent.Name = "checkEdit_SelectRecent";
            this.checkEdit_SelectRecent.Properties.Caption = "Select from recent sessions";
            this.checkEdit_SelectRecent.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkEdit_SelectRecent.Properties.RadioGroupIndex = 0;
            this.checkEdit_SelectRecent.Size = new System.Drawing.Size(268, 19);
            this.checkEdit_SelectRecent.TabIndex = 20;
            this.checkEdit_SelectRecent.TabStop = false;
            this.checkEdit_SelectRecent.CheckedChanged += new System.EventHandler(this.checkEdit_SelectRecent_CheckedChanged);
            // 
            // checkEdit_SelectNoErrors
            // 
            this.checkEdit_SelectNoErrors.Location = new System.Drawing.Point(5, 48);
            this.checkEdit_SelectNoErrors.Name = "checkEdit_SelectNoErrors";
            this.checkEdit_SelectNoErrors.Properties.Caption = "Select from list of error free sessions";
            this.checkEdit_SelectNoErrors.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkEdit_SelectNoErrors.Properties.RadioGroupIndex = 0;
            this.checkEdit_SelectNoErrors.Size = new System.Drawing.Size(268, 19);
            this.checkEdit_SelectNoErrors.TabIndex = 21;
            this.checkEdit_SelectNoErrors.TabStop = false;
            this.checkEdit_SelectNoErrors.CheckedChanged += new System.EventHandler(this.checkEdit_SelectRecent_CheckedChanged);
            // 
            // labelControl_LatestBaseId
            // 
            this.labelControl_LatestBaseId.Location = new System.Drawing.Point(4, 217);
            this.labelControl_LatestBaseId.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_LatestBaseId.Name = "labelControl_LatestBaseId";
            this.labelControl_LatestBaseId.Size = new System.Drawing.Size(113, 13);
            this.labelControl_LatestBaseId.TabIndex = 4;
            this.labelControl_LatestBaseId.Text = "Available Base Sessions";
            // 
            // labelControl_CurrentBaseId
            // 
            this.labelControl_CurrentBaseId.Location = new System.Drawing.Point(4, 148);
            this.labelControl_CurrentBaseId.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_CurrentBaseId.Name = "labelControl_CurrentBaseId";
            this.labelControl_CurrentBaseId.Size = new System.Drawing.Size(174, 13);
            this.labelControl_CurrentBaseId.TabIndex = 5;
            this.labelControl_CurrentBaseId.Text = "Selected Configuration Base Session";
            // 
            // simpleButton_UpdateBaseId
            // 
            this.simpleButton_UpdateBaseId.Appearance.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.simpleButton_UpdateBaseId.Appearance.Options.UseFont = true;
            this.simpleButton_UpdateBaseId.Location = new System.Drawing.Point(3, 267);
            this.simpleButton_UpdateBaseId.Name = "simpleButton_UpdateBaseId";
            this.simpleButton_UpdateBaseId.Size = new System.Drawing.Size(199, 33);
            this.simpleButton_UpdateBaseId.TabIndex = 19;
            this.simpleButton_UpdateBaseId.Text = "Update Base Session Id";
            this.simpleButton_UpdateBaseId.Click += new System.EventHandler(this.simpleButton_UpdateBaseId_Click);
            // 
            // imageComboBoxEdit_LatestBaseId
            // 
            this.imageComboBoxEdit_LatestBaseId.Location = new System.Drawing.Point(4, 235);
            this.imageComboBoxEdit_LatestBaseId.Name = "imageComboBoxEdit_LatestBaseId";
            this.imageComboBoxEdit_LatestBaseId.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.imageComboBoxEdit_LatestBaseId.Size = new System.Drawing.Size(279, 20);
            this.imageComboBoxEdit_LatestBaseId.TabIndex = 17;
            this.imageComboBoxEdit_LatestBaseId.SelectedIndexChanged += new System.EventHandler(this.imageComboBoxEdit_LatestBaseId_SelectedIndexChanged);
            // 
            // textEdit_CurrentBaseId
            // 
            this.textEdit_CurrentBaseId.Enabled = false;
            this.textEdit_CurrentBaseId.Location = new System.Drawing.Point(4, 170);
            this.textEdit_CurrentBaseId.Margin = new System.Windows.Forms.Padding(2);
            this.textEdit_CurrentBaseId.Name = "textEdit_CurrentBaseId";
            this.textEdit_CurrentBaseId.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
            this.textEdit_CurrentBaseId.Size = new System.Drawing.Size(279, 20);
            this.textEdit_CurrentBaseId.TabIndex = 18;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.labelControl_dbConnection);
            this.groupControl2.Controls.Add(this.labelControl_InstalledNote);
            this.groupControl2.Controls.Add(this.simpleButton_Install);
            this.groupControl2.Location = new System.Drawing.Point(17, 48);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(463, 127);
            this.groupControl2.TabIndex = 20;
            this.groupControl2.Text = "Database Installation";
            // 
            // labelControl_dbConnection
            // 
            this.labelControl_dbConnection.Appearance.Options.UseTextOptions = true;
            this.labelControl_dbConnection.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelControl_dbConnection.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelControl_dbConnection.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl_dbConnection.Location = new System.Drawing.Point(12, 23);
            this.labelControl_dbConnection.Name = "labelControl_dbConnection";
            this.labelControl_dbConnection.Size = new System.Drawing.Size(446, 36);
            this.labelControl_dbConnection.TabIndex = 4;
            this.labelControl_dbConnection.Text = "Database Connection Info";
            // 
            // labelControl_InstalledNote
            // 
            this.labelControl_InstalledNote.Location = new System.Drawing.Point(12, 65);
            this.labelControl_InstalledNote.Name = "labelControl_InstalledNote";
            this.labelControl_InstalledNote.Size = new System.Drawing.Size(67, 13);
            this.labelControl_InstalledNote.TabIndex = 3;
            this.labelControl_InstalledNote.Text = "[Install result]";
            // 
            // labelControl_HostName
            // 
            this.labelControl_HostName.Appearance.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl_HostName.Appearance.Options.UseFont = true;
            this.labelControl_HostName.Location = new System.Drawing.Point(16, 19);
            this.labelControl_HostName.Name = "labelControl_HostName";
            this.labelControl_HostName.Size = new System.Drawing.Size(155, 23);
            this.labelControl_HostName.TabIndex = 2;
            this.labelControl_HostName.Text = "Machine Name: ";
            // 
            // tabNavigationPage_Config
            // 
            this.tabNavigationPage_Config.Caption = "Configuration";
            this.tabNavigationPage_Config.Controls.Add(this.groupControl1);
            this.tabNavigationPage_Config.Controls.Add(this.simpleButton_AddNewConfig);
            this.tabNavigationPage_Config.Controls.Add(this.imageComboBoxEdit_ConfigTitle);
            this.tabNavigationPage_Config.Controls.Add(this.labelControl_ConfigInfoMessage);
            this.tabNavigationPage_Config.Name = "tabNavigationPage_Config";
            this.tabNavigationPage_Config.Size = new System.Drawing.Size(512, 544);
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.labelControl_maxProcessRestartCount);
            this.groupControl1.Controls.Add(this.spinEdit_MaxRestartCount);
            this.groupControl1.Controls.Add(this.spinEdit_PlayerTimeOut);
            this.groupControl1.Controls.Add(this.labelControl_PlayerTimeOut);
            this.groupControl1.Controls.Add(this.labelControl_ConfigName);
            this.groupControl1.Controls.Add(this.simpleButton_Delete);
            this.groupControl1.Controls.Add(this.buttonEdit_KeyFolderPath);
            this.groupControl1.Controls.Add(this.simpleButton_SaveConfiguration);
            this.groupControl1.Controls.Add(this.labelControl_KeyFolderPath);
            this.groupControl1.Controls.Add(this.simpleButton_UseThisConfig);
            this.groupControl1.Controls.Add(this.textEdit_ConfigName);
            this.groupControl1.Controls.Add(this.buttonEdit_RecordingsDir);
            this.groupControl1.Controls.Add(this.toggleSwitch_LoadCustomization);
            this.groupControl1.Controls.Add(this.labelControl_RecordingsDir);
            this.groupControl1.Controls.Add(this.labelControl_LoadCustomization);
            this.groupControl1.Controls.Add(this.labelControl_MasterCopyPath);
            this.groupControl1.Controls.Add(this.buttonEdit_MasterCopyPath);
            this.groupControl1.Location = new System.Drawing.Point(19, 120);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(451, 394);
            this.groupControl1.TabIndex = 20;
            this.groupControl1.Text = "Configuration Properties";
            // 
            // labelControl_maxProcessRestartCount
            // 
            this.labelControl_maxProcessRestartCount.Location = new System.Drawing.Point(145, 280);
            this.labelControl_maxProcessRestartCount.Name = "labelControl_maxProcessRestartCount";
            this.labelControl_maxProcessRestartCount.Size = new System.Drawing.Size(131, 13);
            this.labelControl_maxProcessRestartCount.TabIndex = 23;
            this.labelControl_maxProcessRestartCount.Text = "Max Process Restart Count";
            // 
            // spinEdit_MaxRestartCount
            // 
            this.spinEdit_MaxRestartCount.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spinEdit_MaxRestartCount.Location = new System.Drawing.Point(145, 306);
            this.spinEdit_MaxRestartCount.Margin = new System.Windows.Forms.Padding(2);
            this.spinEdit_MaxRestartCount.Name = "spinEdit_MaxRestartCount";
            this.spinEdit_MaxRestartCount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinEdit_MaxRestartCount.Properties.Mask.EditMask = "f1";
            this.spinEdit_MaxRestartCount.Size = new System.Drawing.Size(95, 20);
            this.spinEdit_MaxRestartCount.TabIndex = 22;
            // 
            // spinEdit_PlayerTimeOut
            // 
            this.spinEdit_PlayerTimeOut.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spinEdit_PlayerTimeOut.Location = new System.Drawing.Point(5, 306);
            this.spinEdit_PlayerTimeOut.Margin = new System.Windows.Forms.Padding(2);
            this.spinEdit_PlayerTimeOut.Name = "spinEdit_PlayerTimeOut";
            this.spinEdit_PlayerTimeOut.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinEdit_PlayerTimeOut.Properties.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.spinEdit_PlayerTimeOut.Properties.Mask.EditMask = "f1";
            this.spinEdit_PlayerTimeOut.Size = new System.Drawing.Size(95, 20);
            this.spinEdit_PlayerTimeOut.TabIndex = 21;
            // 
            // labelControl_PlayerTimeOut
            // 
            this.labelControl_PlayerTimeOut.Location = new System.Drawing.Point(5, 280);
            this.labelControl_PlayerTimeOut.Name = "labelControl_PlayerTimeOut";
            this.labelControl_PlayerTimeOut.Size = new System.Drawing.Size(105, 13);
            this.labelControl_PlayerTimeOut.TabIndex = 20;
            this.labelControl_PlayerTimeOut.Text = "Player TimeOut (mins)";
            // 
            // labelControl_ConfigName
            // 
            this.labelControl_ConfigName.Location = new System.Drawing.Point(5, 35);
            this.labelControl_ConfigName.Name = "labelControl_ConfigName";
            this.labelControl_ConfigName.Size = new System.Drawing.Size(95, 13);
            this.labelControl_ConfigName.TabIndex = 5;
            this.labelControl_ConfigName.Text = "Configuration Name";
            // 
            // simpleButton_Delete
            // 
            this.simpleButton_Delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton_Delete.Location = new System.Drawing.Point(145, 352);
            this.simpleButton_Delete.Name = "simpleButton_Delete";
            this.simpleButton_Delete.Size = new System.Drawing.Size(115, 33);
            this.simpleButton_Delete.TabIndex = 19;
            this.simpleButton_Delete.Text = "Delete This Config";
            this.simpleButton_Delete.Click += new System.EventHandler(this.simpleButton_Delete_Click);
            // 
            // buttonEdit_KeyFolderPath
            // 
            this.buttonEdit_KeyFolderPath.Location = new System.Drawing.Point(5, 189);
            this.buttonEdit_KeyFolderPath.Name = "buttonEdit_KeyFolderPath";
            this.buttonEdit_KeyFolderPath.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.buttonEdit_KeyFolderPath.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.buttonEdit_KeyFolderPath.Size = new System.Drawing.Size(431, 20);
            this.buttonEdit_KeyFolderPath.TabIndex = 1;
            this.buttonEdit_KeyFolderPath.ButtonPressed += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.buttonEdit_KeyFolderPath_ButtonPressed);
            // 
            // simpleButton_SaveConfiguration
            // 
            this.simpleButton_SaveConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton_SaveConfiguration.Location = new System.Drawing.Point(281, 352);
            this.simpleButton_SaveConfiguration.Name = "simpleButton_SaveConfiguration";
            this.simpleButton_SaveConfiguration.Size = new System.Drawing.Size(155, 33);
            this.simpleButton_SaveConfiguration.TabIndex = 3;
            this.simpleButton_SaveConfiguration.Text = "Save Config Changes";
            this.simpleButton_SaveConfiguration.Click += new System.EventHandler(this.simpleButton_SaveConfiguration_Click);
            // 
            // labelControl_KeyFolderPath
            // 
            this.labelControl_KeyFolderPath.Location = new System.Drawing.Point(5, 169);
            this.labelControl_KeyFolderPath.Name = "labelControl_KeyFolderPath";
            this.labelControl_KeyFolderPath.Size = new System.Drawing.Size(76, 13);
            this.labelControl_KeyFolderPath.TabIndex = 2;
            this.labelControl_KeyFolderPath.Text = "Key Folder Path";
            // 
            // simpleButton_UseThisConfig
            // 
            this.simpleButton_UseThisConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton_UseThisConfig.Location = new System.Drawing.Point(8, 352);
            this.simpleButton_UseThisConfig.Name = "simpleButton_UseThisConfig";
            this.simpleButton_UseThisConfig.Size = new System.Drawing.Size(115, 33);
            this.simpleButton_UseThisConfig.TabIndex = 17;
            this.simpleButton_UseThisConfig.Text = "Use This Config";
            this.simpleButton_UseThisConfig.ToolTip = "Use the configuration when running MR on this computer";
            this.simpleButton_UseThisConfig.Click += new System.EventHandler(this.simpleButton_UseThisConfig_Click);
            // 
            // textEdit_ConfigName
            // 
            this.textEdit_ConfigName.Location = new System.Drawing.Point(5, 54);
            this.textEdit_ConfigName.Name = "textEdit_ConfigName";
            this.textEdit_ConfigName.Size = new System.Drawing.Size(431, 20);
            this.textEdit_ConfigName.TabIndex = 4;
            // 
            // buttonEdit_RecordingsDir
            // 
            this.buttonEdit_RecordingsDir.Location = new System.Drawing.Point(5, 100);
            this.buttonEdit_RecordingsDir.Name = "buttonEdit_RecordingsDir";
            this.buttonEdit_RecordingsDir.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.buttonEdit_RecordingsDir.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.buttonEdit_RecordingsDir.Size = new System.Drawing.Size(431, 20);
            this.buttonEdit_RecordingsDir.TabIndex = 6;
            this.buttonEdit_RecordingsDir.ButtonPressed += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.buttonEdit_RecordingsDir_ButtonPressed);
            // 
            // toggleSwitch_LoadCustomization
            // 
            this.toggleSwitch_LoadCustomization.Location = new System.Drawing.Point(4, 240);
            this.toggleSwitch_LoadCustomization.Margin = new System.Windows.Forms.Padding(2);
            this.toggleSwitch_LoadCustomization.Name = "toggleSwitch_LoadCustomization";
            this.toggleSwitch_LoadCustomization.Properties.OffText = " No";
            this.toggleSwitch_LoadCustomization.Properties.OnText = " Yes";
            this.toggleSwitch_LoadCustomization.Size = new System.Drawing.Size(104, 24);
            this.toggleSwitch_LoadCustomization.TabIndex = 14;
            // 
            // labelControl_RecordingsDir
            // 
            this.labelControl_RecordingsDir.Location = new System.Drawing.Point(5, 79);
            this.labelControl_RecordingsDir.Name = "labelControl_RecordingsDir";
            this.labelControl_RecordingsDir.Size = new System.Drawing.Size(100, 13);
            this.labelControl_RecordingsDir.TabIndex = 7;
            this.labelControl_RecordingsDir.Text = "Recordings Directory";
            // 
            // labelControl_LoadCustomization
            // 
            this.labelControl_LoadCustomization.Location = new System.Drawing.Point(5, 216);
            this.labelControl_LoadCustomization.Name = "labelControl_LoadCustomization";
            this.labelControl_LoadCustomization.Size = new System.Drawing.Size(93, 13);
            this.labelControl_LoadCustomization.TabIndex = 13;
            this.labelControl_LoadCustomization.Text = "Load Extension";
            // 
            // labelControl_MasterCopyPath
            // 
            this.labelControl_MasterCopyPath.Location = new System.Drawing.Point(5, 126);
            this.labelControl_MasterCopyPath.Name = "labelControl_MasterCopyPath";
            this.labelControl_MasterCopyPath.Size = new System.Drawing.Size(86, 13);
            this.labelControl_MasterCopyPath.TabIndex = 11;
            this.labelControl_MasterCopyPath.Text = "Master Copy Path";
            // 
            // buttonEdit_MasterCopyPath
            // 
            this.buttonEdit_MasterCopyPath.Location = new System.Drawing.Point(5, 146);
            this.buttonEdit_MasterCopyPath.Name = "buttonEdit_MasterCopyPath";
            this.buttonEdit_MasterCopyPath.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.buttonEdit_MasterCopyPath.Size = new System.Drawing.Size(431, 20);
            this.buttonEdit_MasterCopyPath.TabIndex = 10;
            this.buttonEdit_MasterCopyPath.ButtonPressed += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.buttonEdit_MasterCopyPath_ButtonPressed);
            // 
            // simpleButton_AddNewConfig
            // 
            this.simpleButton_AddNewConfig.Location = new System.Drawing.Point(19, 13);
            this.simpleButton_AddNewConfig.Name = "simpleButton_AddNewConfig";
            this.simpleButton_AddNewConfig.Size = new System.Drawing.Size(158, 33);
            this.simpleButton_AddNewConfig.TabIndex = 18;
            this.simpleButton_AddNewConfig.Text = "Add New Configuration";
            this.simpleButton_AddNewConfig.ToolTip = "Add a new configuration to the database";
            this.simpleButton_AddNewConfig.Click += new System.EventHandler(this.simpleButton_AddNewConfig_Click);
            // 
            // imageComboBoxEdit_ConfigTitle
            // 
            this.imageComboBoxEdit_ConfigTitle.Location = new System.Drawing.Point(24, 85);
            this.imageComboBoxEdit_ConfigTitle.Name = "imageComboBoxEdit_ConfigTitle";
            this.imageComboBoxEdit_ConfigTitle.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.imageComboBoxEdit_ConfigTitle.Size = new System.Drawing.Size(279, 20);
            this.imageComboBoxEdit_ConfigTitle.TabIndex = 16;
            this.imageComboBoxEdit_ConfigTitle.SelectedIndexChanged += new System.EventHandler(this.imageComboBoxEdit_ConfigTitle_SelectedIndexChanged);
            // 
            // labelControl_ConfigInfoMessage
            // 
            this.labelControl_ConfigInfoMessage.Location = new System.Drawing.Point(28, 66);
            this.labelControl_ConfigInfoMessage.Name = "labelControl_ConfigInfoMessage";
            this.labelControl_ConfigInfoMessage.Size = new System.Drawing.Size(97, 13);
            this.labelControl_ConfigInfoMessage.TabIndex = 0;
            this.labelControl_ConfigInfoMessage.Text = "Select Configuration";
            // 
            // tabNavigationPage_Analysis
            // 
            this.tabNavigationPage_Analysis.Caption = "Analysis";
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_Progress);
            this.tabNavigationPage_Analysis.Controls.Add(this.textEdit_NumberPlayersRun);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_NumberPlayersRun);
            this.tabNavigationPage_Analysis.Controls.Add(this.textEdit_RunLocation);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_RunLocation);
            this.tabNavigationPage_Analysis.Controls.Add(this.textEdit_EndTime);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_EndTime);
            this.tabNavigationPage_Analysis.Controls.Add(this.textEdit_StartTime);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_StartTime);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_AnalysisMessage);
            this.tabNavigationPage_Analysis.Controls.Add(this.progressBarControl_CompletedPlayers);
            this.tabNavigationPage_Analysis.Controls.Add(this.textEdit_NumberPlayersRemaining);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_NumberPlayerRemaining);
            this.tabNavigationPage_Analysis.Controls.Add(this.textEdit_NumberPlayersOn);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_NumberPlayersOn);
            this.tabNavigationPage_Analysis.Controls.Add(this.simpleButton_EndProcesses);
            this.tabNavigationPage_Analysis.Controls.Add(this.labelControl_Status);
            this.tabNavigationPage_Analysis.Name = "tabNavigationPage_Analysis";
            this.tabNavigationPage_Analysis.Size = new System.Drawing.Size(512, 544);
            // 
            // labelControl_Progress
            // 
            this.labelControl_Progress.Location = new System.Drawing.Point(22, 285);
            this.labelControl_Progress.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_Progress.Name = "labelControl_Progress";
            this.labelControl_Progress.Size = new System.Drawing.Size(46, 13);
            this.labelControl_Progress.TabIndex = 22;
            this.labelControl_Progress.Text = "Progress:";
            // 
            // textEdit_NumberPlayersRun
            // 
            this.textEdit_NumberPlayersRun.Location = new System.Drawing.Point(22, 219);
            this.textEdit_NumberPlayersRun.Name = "textEdit_NumberPlayersRun";
            this.textEdit_NumberPlayersRun.Properties.ReadOnly = true;
            this.textEdit_NumberPlayersRun.Size = new System.Drawing.Size(32, 20);
            this.textEdit_NumberPlayersRun.TabIndex = 21;
            // 
            // labelControl_NumberPlayersRun
            // 
            this.labelControl_NumberPlayersRun.Location = new System.Drawing.Point(22, 187);
            this.labelControl_NumberPlayersRun.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_NumberPlayersRun.Name = "labelControl_NumberPlayersRun";
            this.labelControl_NumberPlayersRun.Size = new System.Drawing.Size(113, 13);
            this.labelControl_NumberPlayersRun.TabIndex = 20;
            this.labelControl_NumberPlayersRun.Text = "# of Players Completed";
            // 
            // textEdit_RunLocation
            // 
            this.textEdit_RunLocation.Location = new System.Drawing.Point(22, 83);
            this.textEdit_RunLocation.Name = "textEdit_RunLocation";
            this.textEdit_RunLocation.Properties.ReadOnly = true;
            this.textEdit_RunLocation.Size = new System.Drawing.Size(128, 20);
            this.textEdit_RunLocation.TabIndex = 15;
            // 
            // labelControl_RunLocation
            // 
            this.labelControl_RunLocation.Location = new System.Drawing.Point(22, 65);
            this.labelControl_RunLocation.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_RunLocation.Name = "labelControl_RunLocation";
            this.labelControl_RunLocation.Size = new System.Drawing.Size(59, 13);
            this.labelControl_RunLocation.TabIndex = 14;
            this.labelControl_RunLocation.Text = "RunLocation";
            // 
            // textEdit_EndTime
            // 
            this.textEdit_EndTime.Location = new System.Drawing.Point(333, 83);
            this.textEdit_EndTime.Name = "textEdit_EndTime";
            this.textEdit_EndTime.Properties.ReadOnly = true;
            this.textEdit_EndTime.Size = new System.Drawing.Size(144, 20);
            this.textEdit_EndTime.TabIndex = 13;
            // 
            // labelControl_EndTime
            // 
            this.labelControl_EndTime.Location = new System.Drawing.Point(333, 65);
            this.labelControl_EndTime.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_EndTime.Name = "labelControl_EndTime";
            this.labelControl_EndTime.Size = new System.Drawing.Size(40, 13);
            this.labelControl_EndTime.TabIndex = 12;
            this.labelControl_EndTime.Text = "EndTime";
            // 
            // textEdit_StartTime
            // 
            this.textEdit_StartTime.Location = new System.Drawing.Point(171, 83);
            this.textEdit_StartTime.Name = "textEdit_StartTime";
            this.textEdit_StartTime.Properties.ReadOnly = true;
            this.textEdit_StartTime.Size = new System.Drawing.Size(141, 20);
            this.textEdit_StartTime.TabIndex = 11;
            // 
            // labelControl_StartTime
            // 
            this.labelControl_StartTime.Location = new System.Drawing.Point(171, 65);
            this.labelControl_StartTime.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_StartTime.Name = "labelControl_StartTime";
            this.labelControl_StartTime.Size = new System.Drawing.Size(46, 13);
            this.labelControl_StartTime.TabIndex = 10;
            this.labelControl_StartTime.Text = "StartTime";
            // 
            // labelControl_AnalysisMessage
            // 
            this.labelControl_AnalysisMessage.Location = new System.Drawing.Point(104, 26);
            this.labelControl_AnalysisMessage.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_AnalysisMessage.Name = "labelControl_AnalysisMessage";
            this.labelControl_AnalysisMessage.Size = new System.Drawing.Size(23, 13);
            this.labelControl_AnalysisMessage.TabIndex = 9;
            this.labelControl_AnalysisMessage.Text = "Alert";
            // 
            // progressBarControl_CompletedPlayers
            // 
            this.progressBarControl_CompletedPlayers.Location = new System.Drawing.Point(22, 324);
            this.progressBarControl_CompletedPlayers.Margin = new System.Windows.Forms.Padding(2);
            this.progressBarControl_CompletedPlayers.Name = "progressBarControl_CompletedPlayers";
            this.progressBarControl_CompletedPlayers.Size = new System.Drawing.Size(302, 19);
            this.progressBarControl_CompletedPlayers.TabIndex = 8;
            // 
            // textEdit_NumberPlayersRemaining
            // 
            this.textEdit_NumberPlayersRemaining.Location = new System.Drawing.Point(171, 219);
            this.textEdit_NumberPlayersRemaining.Name = "textEdit_NumberPlayersRemaining";
            this.textEdit_NumberPlayersRemaining.Properties.ReadOnly = true;
            this.textEdit_NumberPlayersRemaining.Size = new System.Drawing.Size(32, 20);
            this.textEdit_NumberPlayersRemaining.TabIndex = 7;
            // 
            // labelControl_NumberPlayerRemaining
            // 
            this.labelControl_NumberPlayerRemaining.Location = new System.Drawing.Point(171, 187);
            this.labelControl_NumberPlayerRemaining.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_NumberPlayerRemaining.Name = "labelControl_NumberPlayerRemaining";
            this.labelControl_NumberPlayerRemaining.Size = new System.Drawing.Size(111, 13);
            this.labelControl_NumberPlayerRemaining.TabIndex = 6;
            this.labelControl_NumberPlayerRemaining.Text = "# of Players Remaining";
            // 
            // textEdit_NumberPlayersOn
            // 
            this.textEdit_NumberPlayersOn.Location = new System.Drawing.Point(333, 219);
            this.textEdit_NumberPlayersOn.Name = "textEdit_NumberPlayersOn";
            this.textEdit_NumberPlayersOn.Properties.ReadOnly = true;
            this.textEdit_NumberPlayersOn.Size = new System.Drawing.Size(32, 20);
            this.textEdit_NumberPlayersOn.TabIndex = 5;
            // 
            // labelControl_NumberPlayersOn
            // 
            this.labelControl_NumberPlayersOn.Location = new System.Drawing.Point(333, 187);
            this.labelControl_NumberPlayersOn.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_NumberPlayersOn.Name = "labelControl_NumberPlayersOn";
            this.labelControl_NumberPlayersOn.Size = new System.Drawing.Size(76, 13);
            this.labelControl_NumberPlayersOn.TabIndex = 2;
            this.labelControl_NumberPlayersOn.Text = "# of Players On";
            // 
            // simpleButton_EndProcesses
            // 
            this.simpleButton_EndProcesses.Location = new System.Drawing.Point(22, 395);
            this.simpleButton_EndProcesses.Margin = new System.Windows.Forms.Padding(2);
            this.simpleButton_EndProcesses.Name = "simpleButton_EndProcesses";
            this.simpleButton_EndProcesses.Size = new System.Drawing.Size(104, 29);
            this.simpleButton_EndProcesses.TabIndex = 1;
            this.simpleButton_EndProcesses.Text = "End Processes";
            this.simpleButton_EndProcesses.Click += new System.EventHandler(this.simpleButton_EndProcesses_Click);
            // 
            // labelControl_Status
            // 
            this.labelControl_Status.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl_Status.Appearance.Options.UseFont = true;
            this.labelControl_Status.Location = new System.Drawing.Point(15, 21);
            this.labelControl_Status.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_Status.Name = "labelControl_Status";
            this.labelControl_Status.Size = new System.Drawing.Size(58, 19);
            this.labelControl_Status.TabIndex = 0;
            this.labelControl_Status.Text = "Status:";
            // 
            // tabNavigationPage_Results
            // 
            this.tabNavigationPage_Results.Caption = "Results";
            this.tabNavigationPage_Results.Controls.Add(this.simpleButton_ShowWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.marqueeProgressBarControl_GatherWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.marqueeProgressBarControl_Loading);
            this.tabNavigationPage_Results.Controls.Add(this.memoEdit_TopRecordingsLength);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_TopRecordingsLength);
            this.tabNavigationPage_Results.Controls.Add(this.simpleButton_GatherWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.memoEdit_TopRecordingsWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_TopRecordingsWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.textEdit_NumWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_NumWarnings);
            this.tabNavigationPage_Results.Controls.Add(this.textEdit_PercentNoErrors);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_PercentNoErrors);
            this.tabNavigationPage_Results.Controls.Add(this.textEdit_NumPlayersWithErrors);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_NumPlayersWithErrors);
            this.tabNavigationPage_Results.Controls.Add(this.textEdit_ResultsTotalNumberPlayers);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_ResultsTotalNumberPlayers);
            this.tabNavigationPage_Results.Controls.Add(this.textEdit_ResultsNumberPlayersRun);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_ResultsNumberPlayersRun);
            this.tabNavigationPage_Results.Controls.Add(this.tabNavigationPage1);
            this.tabNavigationPage_Results.Controls.Add(this.labelControl_Results);
            this.tabNavigationPage_Results.Controls.Add(this.imageComboBoxEdit_RunInstances);
            this.tabNavigationPage_Results.Margin = new System.Windows.Forms.Padding(2);
            this.tabNavigationPage_Results.Name = "tabNavigationPage_Results";
            this.tabNavigationPage_Results.Size = new System.Drawing.Size(512, 544);
            // 
            // simpleButton_ShowWarnings
            // 
            this.simpleButton_ShowWarnings.Location = new System.Drawing.Point(265, 418);
            this.simpleButton_ShowWarnings.Margin = new System.Windows.Forms.Padding(2);
            this.simpleButton_ShowWarnings.Name = "simpleButton_ShowWarnings";
            this.simpleButton_ShowWarnings.Size = new System.Drawing.Size(119, 33);
            this.simpleButton_ShowWarnings.TabIndex = 48;
            this.simpleButton_ShowWarnings.Text = "Show Warnings";
            this.simpleButton_ShowWarnings.Click += new System.EventHandler(this.simpleButton_ShowWarnings_Click);
            // 
            // marqueeProgressBarControl_GatherWarnings
            // 
            this.marqueeProgressBarControl_GatherWarnings.EditValue = "Loading...";
            this.marqueeProgressBarControl_GatherWarnings.Location = new System.Drawing.Point(396, 382);
            this.marqueeProgressBarControl_GatherWarnings.Margin = new System.Windows.Forms.Padding(2);
            this.marqueeProgressBarControl_GatherWarnings.Name = "marqueeProgressBarControl_GatherWarnings";
            this.marqueeProgressBarControl_GatherWarnings.Properties.ShowTitle = true;
            this.marqueeProgressBarControl_GatherWarnings.Size = new System.Drawing.Size(67, 21);
            this.marqueeProgressBarControl_GatherWarnings.TabIndex = 46;
            this.marqueeProgressBarControl_GatherWarnings.Visible = false;
            // 
            // marqueeProgressBarControl_Loading
            // 
            this.marqueeProgressBarControl_Loading.EditValue = "Loading...";
            this.marqueeProgressBarControl_Loading.Location = new System.Drawing.Point(402, 18);
            this.marqueeProgressBarControl_Loading.Margin = new System.Windows.Forms.Padding(2);
            this.marqueeProgressBarControl_Loading.Name = "marqueeProgressBarControl_Loading";
            this.marqueeProgressBarControl_Loading.Properties.ShowTitle = true;
            this.marqueeProgressBarControl_Loading.Size = new System.Drawing.Size(77, 17);
            this.marqueeProgressBarControl_Loading.TabIndex = 45;
            // 
            // memoEdit_TopRecordingsLength
            // 
            this.memoEdit_TopRecordingsLength.Location = new System.Drawing.Point(249, 76);
            this.memoEdit_TopRecordingsLength.Margin = new System.Windows.Forms.Padding(2);
            this.memoEdit_TopRecordingsLength.Name = "memoEdit_TopRecordingsLength";
            this.memoEdit_TopRecordingsLength.Properties.ReadOnly = true;
            this.memoEdit_TopRecordingsLength.Size = new System.Drawing.Size(218, 280);
            this.memoEdit_TopRecordingsLength.TabIndex = 42;
            // 
            // labelControl_TopRecordingsLength
            // 
            this.labelControl_TopRecordingsLength.Location = new System.Drawing.Point(249, 51);
            this.labelControl_TopRecordingsLength.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_TopRecordingsLength.Name = "labelControl_TopRecordingsLength";
            this.labelControl_TopRecordingsLength.Size = new System.Drawing.Size(142, 13);
            this.labelControl_TopRecordingsLength.TabIndex = 41;
            this.labelControl_TopRecordingsLength.Text = "Top Recordings - Time Length";
            // 
            // simpleButton_GatherWarnings
            // 
            this.simpleButton_GatherWarnings.Location = new System.Drawing.Point(265, 376);
            this.simpleButton_GatherWarnings.Margin = new System.Windows.Forms.Padding(2);
            this.simpleButton_GatherWarnings.Name = "simpleButton_GatherWarnings";
            this.simpleButton_GatherWarnings.Size = new System.Drawing.Size(119, 33);
            this.simpleButton_GatherWarnings.TabIndex = 39;
            this.simpleButton_GatherWarnings.Text = "Gather Warnings";
            this.simpleButton_GatherWarnings.Click += new System.EventHandler(this.simpleButton_GatherWarnings_Click);
            // 
            // memoEdit_TopRecordingsWarnings
            // 
            this.memoEdit_TopRecordingsWarnings.Location = new System.Drawing.Point(15, 75);
            this.memoEdit_TopRecordingsWarnings.Margin = new System.Windows.Forms.Padding(2);
            this.memoEdit_TopRecordingsWarnings.Name = "memoEdit_TopRecordingsWarnings";
            this.memoEdit_TopRecordingsWarnings.Properties.ReadOnly = true;
            this.memoEdit_TopRecordingsWarnings.Size = new System.Drawing.Size(218, 281);
            this.memoEdit_TopRecordingsWarnings.TabIndex = 37;
            // 
            // labelControl_TopRecordingsWarnings
            // 
            this.labelControl_TopRecordingsWarnings.Location = new System.Drawing.Point(15, 51);
            this.labelControl_TopRecordingsWarnings.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_TopRecordingsWarnings.Name = "labelControl_TopRecordingsWarnings";
            this.labelControl_TopRecordingsWarnings.Size = new System.Drawing.Size(161, 13);
            this.labelControl_TopRecordingsWarnings.TabIndex = 36;
            this.labelControl_TopRecordingsWarnings.Text = "Top Recordings - Warnings Count";
            // 
            // textEdit_NumWarnings
            // 
            this.textEdit_NumWarnings.Location = new System.Drawing.Point(15, 494);
            this.textEdit_NumWarnings.Name = "textEdit_NumWarnings";
            this.textEdit_NumWarnings.Properties.ReadOnly = true;
            this.textEdit_NumWarnings.Size = new System.Drawing.Size(51, 20);
            this.textEdit_NumWarnings.TabIndex = 35;
            // 
            // labelControl_NumWarnings
            // 
            this.labelControl_NumWarnings.Location = new System.Drawing.Point(70, 499);
            this.labelControl_NumWarnings.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_NumWarnings.Name = "labelControl_NumWarnings";
            this.labelControl_NumWarnings.Size = new System.Drawing.Size(69, 13);
            this.labelControl_NumWarnings.TabIndex = 34;
            this.labelControl_NumWarnings.Text = "# of Warnings";
            // 
            // textEdit_PercentNoErrors
            // 
            this.textEdit_PercentNoErrors.Location = new System.Drawing.Point(15, 464);
            this.textEdit_PercentNoErrors.Name = "textEdit_PercentNoErrors";
            this.textEdit_PercentNoErrors.Properties.ReadOnly = true;
            this.textEdit_PercentNoErrors.Size = new System.Drawing.Size(51, 20);
            this.textEdit_PercentNoErrors.TabIndex = 33;
            // 
            // labelControl_PercentNoErrors
            // 
            this.labelControl_PercentNoErrors.Location = new System.Drawing.Point(70, 469);
            this.labelControl_PercentNoErrors.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_PercentNoErrors.Name = "labelControl_PercentNoErrors";
            this.labelControl_PercentNoErrors.Size = new System.Drawing.Size(122, 13);
            this.labelControl_PercentNoErrors.TabIndex = 32;
            this.labelControl_PercentNoErrors.Text = "% Players With No Errors";
            // 
            // textEdit_NumPlayersWithErrors
            // 
            this.textEdit_NumPlayersWithErrors.Location = new System.Drawing.Point(15, 434);
            this.textEdit_NumPlayersWithErrors.Name = "textEdit_NumPlayersWithErrors";
            this.textEdit_NumPlayersWithErrors.Properties.ReadOnly = true;
            this.textEdit_NumPlayersWithErrors.Size = new System.Drawing.Size(51, 20);
            this.textEdit_NumPlayersWithErrors.TabIndex = 31;
            // 
            // labelControl_NumPlayersWithErrors
            // 
            this.labelControl_NumPlayersWithErrors.Location = new System.Drawing.Point(70, 438);
            this.labelControl_NumPlayersWithErrors.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_NumPlayersWithErrors.Name = "labelControl_NumPlayersWithErrors";
            this.labelControl_NumPlayersWithErrors.Size = new System.Drawing.Size(145, 13);
            this.labelControl_NumPlayersWithErrors.TabIndex = 30;
            this.labelControl_NumPlayersWithErrors.Text = "Number of Players With Errors";
            // 
            // textEdit_ResultsTotalNumberPlayers
            // 
            this.textEdit_ResultsTotalNumberPlayers.Location = new System.Drawing.Point(15, 373);
            this.textEdit_ResultsTotalNumberPlayers.Name = "textEdit_ResultsTotalNumberPlayers";
            this.textEdit_ResultsTotalNumberPlayers.Properties.ReadOnly = true;
            this.textEdit_ResultsTotalNumberPlayers.Size = new System.Drawing.Size(51, 20);
            this.textEdit_ResultsTotalNumberPlayers.TabIndex = 28;
            // 
            // labelControl_ResultsTotalNumberPlayers
            // 
            this.labelControl_ResultsTotalNumberPlayers.Location = new System.Drawing.Point(70, 378);
            this.labelControl_ResultsTotalNumberPlayers.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_ResultsTotalNumberPlayers.Name = "labelControl_ResultsTotalNumberPlayers";
            this.labelControl_ResultsTotalNumberPlayers.Size = new System.Drawing.Size(115, 13);
            this.labelControl_ResultsTotalNumberPlayers.TabIndex = 27;
            this.labelControl_ResultsTotalNumberPlayers.Text = "Total Number of Players";
            // 
            // textEdit_ResultsNumberPlayersRun
            // 
            this.textEdit_ResultsNumberPlayersRun.Location = new System.Drawing.Point(15, 402);
            this.textEdit_ResultsNumberPlayersRun.Name = "textEdit_ResultsNumberPlayersRun";
            this.textEdit_ResultsNumberPlayersRun.Properties.ReadOnly = true;
            this.textEdit_ResultsNumberPlayersRun.Size = new System.Drawing.Size(51, 20);
            this.textEdit_ResultsNumberPlayersRun.TabIndex = 25;
            // 
            // labelControl_ResultsNumberPlayersRun
            // 
            this.labelControl_ResultsNumberPlayersRun.Location = new System.Drawing.Point(70, 407);
            this.labelControl_ResultsNumberPlayersRun.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_ResultsNumberPlayersRun.Name = "labelControl_ResultsNumberPlayersRun";
            this.labelControl_ResultsNumberPlayersRun.Size = new System.Drawing.Size(110, 13);
            this.labelControl_ResultsNumberPlayersRun.TabIndex = 24;
            this.labelControl_ResultsNumberPlayersRun.Text = "Number of Players Run";
            // 
            // tabNavigationPage1
            // 
            this.tabNavigationPage1.Caption = "Analysis";
            this.tabNavigationPage1.Controls.Add(this.labelControl2);
            this.tabNavigationPage1.Controls.Add(this.textEdit1);
            this.tabNavigationPage1.Controls.Add(this.labelControl1);
            this.tabNavigationPage1.Controls.Add(this.textEdit2);
            this.tabNavigationPage1.Controls.Add(this.labelControl3);
            this.tabNavigationPage1.Controls.Add(this.textEdit3);
            this.tabNavigationPage1.Controls.Add(this.labelControl4);
            this.tabNavigationPage1.Controls.Add(this.textEdit4);
            this.tabNavigationPage1.Controls.Add(this.labelControl5);
            this.tabNavigationPage1.Controls.Add(this.labelControl6);
            this.tabNavigationPage1.Controls.Add(this.progressBarControl1);
            this.tabNavigationPage1.Controls.Add(this.textEdit5);
            this.tabNavigationPage1.Controls.Add(this.labelControl7);
            this.tabNavigationPage1.Controls.Add(this.textEdit6);
            this.tabNavigationPage1.Controls.Add(this.labelControl8);
            this.tabNavigationPage1.Controls.Add(this.simpleButton1);
            this.tabNavigationPage1.Controls.Add(this.labelControl9);
            this.tabNavigationPage1.Name = "tabNavigationPage1";
            this.tabNavigationPage1.Size = new System.Drawing.Size(493, 561);
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(22, 285);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(46, 13);
            this.labelControl2.TabIndex = 22;
            this.labelControl2.Text = "Progress:";
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(22, 219);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(32, 20);
            this.textEdit1.TabIndex = 21;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(22, 187);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(110, 13);
            this.labelControl1.TabIndex = 20;
            this.labelControl1.Text = "Number of Players Run";
            // 
            // textEdit2
            // 
            this.textEdit2.Location = new System.Drawing.Point(22, 116);
            this.textEdit2.Name = "textEdit2";
            this.textEdit2.Size = new System.Drawing.Size(128, 20);
            this.textEdit2.TabIndex = 15;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(22, 81);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(59, 13);
            this.labelControl3.TabIndex = 14;
            this.labelControl3.Text = "RunLocation";
            // 
            // textEdit3
            // 
            this.textEdit3.Location = new System.Drawing.Point(333, 116);
            this.textEdit3.Name = "textEdit3";
            this.textEdit3.Size = new System.Drawing.Size(144, 20);
            this.textEdit3.TabIndex = 13;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(333, 81);
            this.labelControl4.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(40, 13);
            this.labelControl4.TabIndex = 12;
            this.labelControl4.Text = "EndTime";
            // 
            // textEdit4
            // 
            this.textEdit4.Location = new System.Drawing.Point(171, 116);
            this.textEdit4.Name = "textEdit4";
            this.textEdit4.Size = new System.Drawing.Size(141, 20);
            this.textEdit4.TabIndex = 11;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(171, 81);
            this.labelControl5.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(46, 13);
            this.labelControl5.TabIndex = 10;
            this.labelControl5.Text = "StartTime";
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(104, 26);
            this.labelControl6.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(23, 13);
            this.labelControl6.TabIndex = 9;
            this.labelControl6.Text = "Alert";
            // 
            // progressBarControl1
            // 
            this.progressBarControl1.Location = new System.Drawing.Point(22, 324);
            this.progressBarControl1.Margin = new System.Windows.Forms.Padding(2);
            this.progressBarControl1.Name = "progressBarControl1";
            this.progressBarControl1.Size = new System.Drawing.Size(302, 19);
            this.progressBarControl1.TabIndex = 8;
            // 
            // textEdit5
            // 
            this.textEdit5.Location = new System.Drawing.Point(333, 219);
            this.textEdit5.Name = "textEdit5";
            this.textEdit5.Size = new System.Drawing.Size(32, 20);
            this.textEdit5.TabIndex = 7;
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(333, 187);
            this.labelControl7.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(140, 13);
            this.labelControl7.TabIndex = 6;
            this.labelControl7.Text = "Number of Players Remaining";
            // 
            // textEdit6
            // 
            this.textEdit6.Location = new System.Drawing.Point(171, 219);
            this.textEdit6.Name = "textEdit6";
            this.textEdit6.Size = new System.Drawing.Size(32, 20);
            this.textEdit6.TabIndex = 5;
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(171, 187);
            this.labelControl8.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(105, 13);
            this.labelControl8.TabIndex = 2;
            this.labelControl8.Text = "Number of Players On";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(22, 395);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(2);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(104, 29);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Text = "End Processes";
            // 
            // labelControl9
            // 
            this.labelControl9.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl9.Appearance.Options.UseFont = true;
            this.labelControl9.Location = new System.Drawing.Point(15, 21);
            this.labelControl9.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(58, 19);
            this.labelControl9.TabIndex = 0;
            this.labelControl9.Text = "Status:";
            // 
            // labelControl_Results
            // 
            this.labelControl_Results.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl_Results.Appearance.Options.UseFont = true;
            this.labelControl_Results.Location = new System.Drawing.Point(15, 14);
            this.labelControl_Results.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl_Results.Name = "labelControl_Results";
            this.labelControl_Results.Size = new System.Drawing.Size(66, 19);
            this.labelControl_Results.TabIndex = 21;
            this.labelControl_Results.Text = "Results:";
            // 
            // imageComboBoxEdit_RunInstances
            // 
            this.imageComboBoxEdit_RunInstances.Location = new System.Drawing.Point(105, 16);
            this.imageComboBoxEdit_RunInstances.Name = "imageComboBoxEdit_RunInstances";
            this.imageComboBoxEdit_RunInstances.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.imageComboBoxEdit_RunInstances.Size = new System.Drawing.Size(279, 20);
            this.imageComboBoxEdit_RunInstances.TabIndex = 18;
            this.imageComboBoxEdit_RunInstances.SelectedIndexChanged += new System.EventHandler(this.imageComboBoxEdit_RunInstances_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 594);
            this.Controls.Add(this.tabPane_MRUI);
            this.Controls.Add(this.labelControl_Title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Mass Recordings UI";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.tabPane_MRUI)).EndInit();
            this.tabPane_MRUI.ResumeLayout(false);
            this.tabNavigationPage_Install.ResumeLayout(false);
            this.tabNavigationPage_Install.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).EndInit();
            this.groupControl3.ResumeLayout(false);
            this.groupControl3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl_RadioGroup)).EndInit();
            this.groupControl_RadioGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit_SelectRecent.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit_SelectNoErrors.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageComboBoxEdit_LatestBaseId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_CurrentBaseId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            this.tabNavigationPage_Config.ResumeLayout(false);
            this.tabNavigationPage_Config.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit_MaxRestartCount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit_PlayerTimeOut.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_KeyFolderPath.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_ConfigName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_RecordingsDir.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.toggleSwitch_LoadCustomization.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonEdit_MasterCopyPath.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageComboBoxEdit_ConfigTitle.Properties)).EndInit();
            this.tabNavigationPage_Analysis.ResumeLayout(false);
            this.tabNavigationPage_Analysis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumberPlayersRun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_RunLocation.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_EndTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_StartTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl_CompletedPlayers.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumberPlayersRemaining.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumberPlayersOn.Properties)).EndInit();
            this.tabNavigationPage_Results.ResumeLayout(false);
            this.tabNavigationPage_Results.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marqueeProgressBarControl_GatherWarnings.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marqueeProgressBarControl_Loading.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit_TopRecordingsLength.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit_TopRecordingsWarnings.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumWarnings.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_PercentNoErrors.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_NumPlayersWithErrors.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_ResultsTotalNumberPlayers.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_ResultsNumberPlayersRun.Properties)).EndInit();
            this.tabNavigationPage1.ResumeLayout(false);
            this.tabNavigationPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit4.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit5.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit6.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageComboBoxEdit_RunInstances.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl_Title;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Install;
        private DevExpress.XtraBars.Navigation.TabPane tabPane_MRUI;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_Install;
        private DevExpress.XtraEditors.LabelControl labelControl_HostName;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_Config;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_Analysis;
        private DevExpress.XtraEditors.LabelControl labelControl_InstalledNote;
        private DevExpress.XtraEditors.LabelControl labelControl_KeyFolderPath;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit_KeyFolderPath;
        private DevExpress.XtraEditors.LabelControl labelControl_ConfigInfoMessage;
        private DevExpress.XtraEditors.SimpleButton simpleButton_SaveConfiguration;
        private DevExpress.XtraEditors.LabelControl labelControl_ConfigName;
        private DevExpress.XtraEditors.TextEdit textEdit_ConfigName;
        private DevExpress.XtraEditors.LabelControl labelControl_MasterCopyPath;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit_MasterCopyPath;
        private DevExpress.XtraEditors.LabelControl labelControl_RecordingsDir;
        private DevExpress.XtraEditors.ButtonEdit buttonEdit_RecordingsDir;
        private DevExpress.XtraEditors.ToggleSwitch toggleSwitch_LoadCustomization;
        private DevExpress.XtraEditors.LabelControl labelControl_LoadCustomization;
        private DevExpress.XtraEditors.SimpleButton simpleButton_UseThisConfig;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.SimpleButton simpleButton_AddNewConfig;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Delete;
        private DevExpress.XtraEditors.LabelControl labelControl_CurrentBaseId;
        private DevExpress.XtraEditors.LabelControl labelControl_LatestBaseId;
        private DevExpress.XtraEditors.ImageComboBoxEdit imageComboBoxEdit_LatestBaseId;
        private DevExpress.XtraEditors.SimpleButton simpleButton_UpdateBaseId;
        private DevExpress.XtraEditors.TextEdit textEdit_CurrentBaseId;
        private DevExpress.XtraEditors.ImageComboBoxEdit imageComboBoxEdit_ConfigTitle;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.LabelControl labelControl_dbConnection;
        private DevExpress.XtraEditors.GroupControl groupControl3;
        private DevExpress.XtraEditors.LabelControl labelControl_Status;
        private DevExpress.XtraEditors.SimpleButton simpleButton_EndProcesses;
        private DevExpress.XtraEditors.TextEdit textEdit_NumberPlayersOn;
        private DevExpress.XtraEditors.LabelControl labelControl_NumberPlayersOn;
        private DevExpress.XtraEditors.TextEdit textEdit_NumberPlayersRemaining;
        private DevExpress.XtraEditors.LabelControl labelControl_NumberPlayerRemaining;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl_CompletedPlayers;
        private DevExpress.XtraEditors.LabelControl labelControl_AnalysisMessage;
        private DevExpress.XtraEditors.TextEdit textEdit_EndTime;
        private DevExpress.XtraEditors.LabelControl labelControl_EndTime;
        private DevExpress.XtraEditors.TextEdit textEdit_StartTime;
        private DevExpress.XtraEditors.LabelControl labelControl_StartTime;
        private DevExpress.XtraEditors.TextEdit textEdit_RunLocation;
        private DevExpress.XtraEditors.LabelControl labelControl_RunLocation;
        private DevExpress.XtraEditors.SpinEdit spinEdit_PlayerTimeOut;
        private DevExpress.XtraEditors.LabelControl labelControl_PlayerTimeOut;
        private DevExpress.XtraEditors.TextEdit textEdit_NumberPlayersRun;
        private DevExpress.XtraEditors.LabelControl labelControl_NumberPlayersRun;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_Results;
        private DevExpress.XtraEditors.LabelControl labelControl_Progress;
        private DevExpress.XtraEditors.LabelControl labelControl_Results;
        private DevExpress.XtraEditors.ImageComboBoxEdit imageComboBoxEdit_RunInstances;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit textEdit2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit textEdit3;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.TextEdit textEdit4;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl1;
        private DevExpress.XtraEditors.TextEdit textEdit5;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.TextEdit textEdit6;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.LabelControl labelControl9;
        private DevExpress.XtraEditors.TextEdit textEdit_ResultsNumberPlayersRun;
        private DevExpress.XtraEditors.LabelControl labelControl_ResultsNumberPlayersRun;
        private DevExpress.XtraEditors.TextEdit textEdit_ResultsTotalNumberPlayers;
        private DevExpress.XtraEditors.LabelControl labelControl_ResultsTotalNumberPlayers;
        private DevExpress.XtraEditors.TextEdit textEdit_NumWarnings;
        private DevExpress.XtraEditors.LabelControl labelControl_NumWarnings;
        private DevExpress.XtraEditors.TextEdit textEdit_PercentNoErrors;
        private DevExpress.XtraEditors.LabelControl labelControl_PercentNoErrors;
        private DevExpress.XtraEditors.TextEdit textEdit_NumPlayersWithErrors;
        private DevExpress.XtraEditors.LabelControl labelControl_NumPlayersWithErrors;
        private DevExpress.XtraEditors.LabelControl labelControl_TopRecordingsWarnings;
        private DevExpress.XtraEditors.MemoEdit memoEdit_TopRecordingsWarnings;
        private DevExpress.XtraEditors.SimpleButton simpleButton_GatherWarnings;
        private DevExpress.XtraEditors.MemoEdit memoEdit_TopRecordingsLength;
        private DevExpress.XtraEditors.LabelControl labelControl_TopRecordingsLength;
        private DevExpress.XtraEditors.MarqueeProgressBarControl marqueeProgressBarControl_Loading;
        private DevExpress.XtraEditors.MarqueeProgressBarControl marqueeProgressBarControl_GatherWarnings;
        private DevExpress.XtraEditors.SimpleButton simpleButton_ShowWarnings;
        private DevExpress.XtraEditors.GroupControl groupControl_RadioGroup;
        private DevExpress.XtraEditors.CheckEdit checkEdit_SelectRecent;
        private DevExpress.XtraEditors.CheckEdit checkEdit_SelectNoErrors;
        private DevExpress.XtraEditors.LabelControl labelControl_maxProcessRestartCount;
        private DevExpress.XtraEditors.SpinEdit spinEdit_MaxRestartCount;
    }
}

