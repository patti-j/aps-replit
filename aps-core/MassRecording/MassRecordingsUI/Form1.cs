using System.Data;
using System.Text;
using System.Timers;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using MassRecordings;

using static MassRecordings.SqlStrings.TableDefinitions;

namespace MassRecordingsUI;

public partial class Form1 : Form
{
    private readonly string m_userName;
    private MRSql m_mrSql;
    private Dictionary<string, string> m_keyPaths;
    private long m_currentSessionId;
    private int m_totalNumberPlayers;
    private System.Timers.Timer m_timer;
    private int m_numConfigs;
    private RunInstance m_runInstance;
    private StringBuilder m_topRecordingsWarn;
    private StringBuilder m_topRecordingsTime;
    private string m_warningsCompiled;

    public Form1()
    {
        InitializeComponent();
        simpleButton_Install.Visible = false;
        simpleButton_ShowWarnings.Visible = false;
        tabPane_MRUI.Pages[1].PageVisible = false;
        tabPane_MRUI.Pages[2].PageVisible = false;
        labelControl_AnalysisMessage.Text = "";
        m_userName = System.Net.Dns.GetHostName();
        //m_userName = Environment.MachineName;
        labelControl_HostName.Text += m_userName;
    }

    /// <summary>
    /// Retrieves and populates UI with all data from database and selects first configuration or configuration assigned to hostname if exists.
    /// </summary>
    private void LoadInstall()
    {
        LoadConfigurations(m_mrSql.GetAllHostConfigNameIdBaseId());
        labelControl_InstalledNote.Text = "Mass Recordings Installed.";
        labelControl_InstalledNote.Visible = true;
        simpleButton_Install.Visible = false;

        HostConfigurationMappings hostConfigMap = new ();
        DataTable currentMap = m_mrSql.GetHostConfigMap(m_userName);
        if (currentMap != null && currentMap.Rows.Count > 0)
        {
            string mapId = currentMap.Rows[0][hostConfigMap.ConfigurationId].ToString();
            imageComboBoxEdit_ConfigTitle.EditValue = Convert.ToInt32(mapId);
        }
        else
        {
            imageComboBoxEdit_ConfigTitle.EditValue = 0;
        }

        tabPane_MRUI.Pages[1].PageVisible = true;
        tabPane_MRUI.Pages[2].PageVisible = true;

        checkEdit_SelectNoErrors.Checked = true;
    }

    /// <summary>
    /// Retrieve and loads all configurations and base sessions for selection in UI.
    /// </summary>
    /// <param name="a_configurations"></param>
    private void LoadConfigurations(DataTable a_configurations)
    {
        m_numConfigs = a_configurations.Rows.Count;
        if (m_numConfigs > 0)
        {
            foreach (DataRow row in a_configurations.Rows)
            {
                imageComboBoxEdit_ConfigTitle.Properties.Items.Add(new ImageComboBoxItem(row[0].ToString(), Convert.ToInt32(row[1]), 0));
            }

            imageComboBoxEdit_ConfigTitle.EditValue = Convert.ToInt32(a_configurations.Rows[0][1]);
            imageComboBoxEdit_ConfigTitle.Enabled = true;
        }
        else
        {
            imageComboBoxEdit_ConfigTitle.Enabled = false;
        }
    }

    private void PopulateBaseIdComboBox(DataTable a_baseId)
    {
        int baseIdFirstValue = 0;
        if (a_baseId != null && a_baseId.Rows.Count > 0)
        {
            RunInstance runInstance = new ();
            foreach (DataRow row in a_baseId.Rows)
            {
                int index = a_baseId.Rows.IndexOf(row);
                if (index == 0)
                {
                    baseIdFirstValue = Convert.ToInt32(row[0]);
                }

                string latestAvailable = $"{row[runInstance.StartTime]}  | {row[runInstance.RunMode]} | ({row[runInstance.Configuration]})";
                imageComboBoxEdit_LatestBaseId.Properties.Items.Add(new ImageComboBoxItem(latestAvailable, Convert.ToInt32(row[0]), 0));
            }

            imageComboBoxEdit_LatestBaseId.EditValue = baseIdFirstValue;
        }
    }

    private void checkEdit_SelectRecent_CheckedChanged(object sender, EventArgs e)
    {
        CheckEdit button = sender as CheckEdit;

        if (button != null)
        {
            imageComboBoxEdit_LatestBaseId.Properties.Items.Clear();

            if (button == checkEdit_SelectNoErrors)
            {
                PopulateBaseIdComboBox(m_mrSql.GetAllBaseIdStartLoc());
            }

            if (button == checkEdit_SelectRecent)
            {
                PopulateBaseIdComboBox(m_mrSql.GetRecentBaseIdStartLoc());
            }
        }
    }

    /// <summary>
    /// Install mass recordings database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButtonInstall_Click(object sender, EventArgs e)
    {
        m_mrSql.CreateDatabase();
        LoadInstall();
    }

    /// <summary>
    /// Folder selection for Key Folder.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonEdit_KeyFolderPath_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        using (FolderBrowserDialog dialog = new ())
        {
            dialog.Description = "Select Key folder...";
            if (dialog.ShowDialog() != DialogResult.Cancel)
            {
                buttonEdit_KeyFolderPath.Text = dialog.SelectedPath;
            }
        }
    }

    /// <summary>
    /// Folder selection for Recordings Directory.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonEdit_RecordingsDir_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        using (FolderBrowserDialog dialog = new ())
        {
            dialog.Description = "Select Recording folder...";
            if (dialog.ShowDialog() != DialogResult.Cancel)
            {
                buttonEdit_RecordingsDir.Text = dialog.SelectedPath;
            }
        }
    }

    /// <summary>
    /// Folder selection for Master Copy path.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonEdit_MasterCopyPath_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        using (FolderBrowserDialog dialog = new ())
        {
            dialog.Description = "Select MasterCopy folder...";
            if (dialog.ShowDialog() != DialogResult.Cancel)
            {
                buttonEdit_MasterCopyPath.Text = dialog.SelectedPath;
            }
        }
    }

    /// <summary>
    /// Retrieves and displays in UI the selected configuration data.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void imageComboBoxEdit_ConfigTitle_SelectedIndexChanged(object sender, EventArgs e)
    {
        HostConfigurations hostConfiguration = new ();
        labelControl_ConfigInfoMessage.Text = imageComboBoxEdit_ConfigTitle.Text + " selected.";
        int id = (int)imageComboBoxEdit_ConfigTitle.EditValue;
        //TODO: host config has new column: may need to modify this
        DataTable configData = m_mrSql.GetSelectedConfigData(imageComboBoxEdit_ConfigTitle.Text, id);

        if (configData != null && configData.Rows.Count > 0)
        {
            textEdit_ConfigName.Text = configData.Rows[0][hostConfiguration.Name].ToString();
            buttonEdit_RecordingsDir.Text = configData.Rows[0][hostConfiguration.RecordingsDirectory].ToString();
            buttonEdit_MasterCopyPath.Text = configData.Rows[0][hostConfiguration.MasterCopyPath].ToString();
            buttonEdit_KeyFolderPath.Text = configData.Rows[0][hostConfiguration.KeyFolderPath].ToString();
            toggleSwitch_LoadCustomization.IsOn = Convert.ToBoolean(configData.Rows[0][hostConfiguration.LoadCustomization]);
            textEdit_CurrentBaseId.Text = configData.Rows[0][hostConfiguration.BaseSessionId].ToString();
            spinEdit_PlayerTimeOut.Value = Convert.ToInt32(TimeSpan.FromMilliseconds(Convert.ToDouble(configData.Rows[0][hostConfiguration.PlayerTimeOutMS])).TotalMinutes);
            spinEdit_MaxRestartCount.Value = Convert.ToInt32(configData.Rows[0][hostConfiguration.MaxRestartCount]);
        }
    }

    /// <summary>
    /// Creatres of all paths required fo run the selected configuration.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_UseThisConfig_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(imageComboBoxEdit_ConfigTitle.Text) && m_numConfigs != 0)
        {
            List<KeyValuePair<string, string>> paths = new ();
            paths.Add(new KeyValuePair<string, string>("Recordings", buttonEdit_RecordingsDir.Text));
            paths.Add(new KeyValuePair<string, string>("MasterCopy", buttonEdit_MasterCopyPath.Text));
            paths.Add(new KeyValuePair<string, string>("KeyFolder", buttonEdit_KeyFolderPath.Text));
            foreach (KeyValuePair<string, string> path in paths)
            {
                if (path.Value == "")
                {
                    Directory.CreateDirectory(m_keyPaths[path.Key]);
                }
                else
                {
                    Directory.CreateDirectory(path.Value);
                }
            }

            labelControl_ConfigInfoMessage.Text = "Configuration paths added.";
            m_mrSql.UpdateHostConfigMapping(Convert.ToInt32(imageComboBoxEdit_ConfigTitle.EditValue), m_userName);
        }
        else
        {
            labelControl_ConfigInfoMessage.Text = "No configuration selected.";
        }
    }

    /// <summary>
    /// Updates the selected configuration with currently specified values on form.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_SaveConfiguration_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(imageComboBoxEdit_ConfigTitle.Text) && m_numConfigs != 0)
        {
            m_mrSql.UpdateSelectedConfigData(textEdit_ConfigName.Text, buttonEdit_RecordingsDir.Text, buttonEdit_MasterCopyPath.Text, buttonEdit_KeyFolderPath.Text, toggleSwitch_LoadCustomization.IsOn, (double)spinEdit_PlayerTimeOut.Value, (int)spinEdit_MaxRestartCount.Value, (int)imageComboBoxEdit_ConfigTitle.EditValue);
            labelControl_ConfigInfoMessage.Text = "Changes for configuration id: " + imageComboBoxEdit_ConfigTitle.EditValue + " entered.";
        }
        else
        {
            labelControl_ConfigInfoMessage.Text = "No configuration selected.";
        }
    }

    /// <summary>
    /// Clears the UI components display values and gets latest values from database.
    /// </summary>
    private void ClearContents()
    {
        textEdit_ConfigName.Text = "";
        labelControl_InstalledNote.Text = "";
        buttonEdit_RecordingsDir.EditValue = null;
        buttonEdit_MasterCopyPath.EditValue = null;
        buttonEdit_KeyFolderPath.EditValue = null;
        toggleSwitch_LoadCustomization.EditValue = null;
        imageComboBoxEdit_ConfigTitle.Properties.Items.Clear();
        spinEdit_PlayerTimeOut.Value = 0;

        LoadConfigurations(m_mrSql.GetAllHostConfigNameIdBaseId());
        RefreshConfigurations();
    }

    /// <summary>
    /// Add default configuration to database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_AddNewConfig_Click(object sender, EventArgs e)
    {
        //string a_name, string a_recordingsDirectory, string a_ptComponentsLocation, string a_runMode, long a_baseSessionId, Boolean a_loadCustomization, string a_masterCopyPath, Double a_requiredFreeMemory, string a_keyFolderPath
        int newId = m_mrSql.AddNewHostConfig(m_userName, m_keyPaths["MasterCopy"]);

        ClearContents();
        labelControl_ConfigInfoMessage.Text = "New configuration was added.";
        imageComboBoxEdit_ConfigTitle.EditValue = newId;
    }

    /// <summary>
    /// Reloads UI data after change to database.
    /// </summary>
    private void RefreshConfigurations()
    {
        HostConfigurations hostConfig = new ();
        DataTable configData = m_mrSql.GetSelectedConfigData(imageComboBoxEdit_ConfigTitle.Text, Convert.ToInt32(imageComboBoxEdit_ConfigTitle.EditValue));
        if (configData != null && configData.Rows.Count > 0)
        {
            textEdit_ConfigName.Text = configData.Rows[0][hostConfig.Name].ToString();
            buttonEdit_RecordingsDir.Text = configData.Rows[0][hostConfig.RecordingsDirectory].ToString();
            buttonEdit_MasterCopyPath.Text = configData.Rows[0][hostConfig.MasterCopyPath].ToString();
            buttonEdit_KeyFolderPath.Text = configData.Rows[0][hostConfig.KeyFolderPath].ToString();
            toggleSwitch_LoadCustomization.IsOn = Convert.ToBoolean(configData.Rows[0][hostConfig.LoadCustomization]);
            textEdit_CurrentBaseId.Text = configData.Rows[0][hostConfig.BaseSessionId].ToString();
            spinEdit_PlayerTimeOut.Value = Convert.ToInt32(TimeSpan.FromMilliseconds(Convert.ToDouble(configData.Rows[0][hostConfig.PlayerTimeOutMS])).TotalMinutes);
        }
    }

    /// <summary>
    /// Delete the selected configuration from database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_Delete_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(imageComboBoxEdit_ConfigTitle.Text) && m_numConfigs != 0)
        {
            m_mrSql.DeleteSelectedConfig((int)imageComboBoxEdit_ConfigTitle.EditValue);
            ClearContents();
            labelControl_ConfigInfoMessage.Text = "Configuration was deleted.";
        }
        else
        {
            labelControl_ConfigInfoMessage.Text = "No configuration selected.";
        }
    }

    /// <summary>
    /// Update the selected configuration Base Session Id in database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_UpdateBaseId_Click(object sender, EventArgs e)
    {
        m_mrSql.UpdateHostConfigBaseId(Convert.ToInt64(imageComboBoxEdit_LatestBaseId.EditValue), imageComboBoxEdit_ConfigTitle.Text, Convert.ToInt32(imageComboBoxEdit_ConfigTitle.EditValue));
        DataTable updatedId = m_mrSql.GetUpdatedBaseId(Convert.ToInt32(imageComboBoxEdit_ConfigTitle.EditValue));
        if (updatedId != null && updatedId.Rows.Count > 0)
        {
            textEdit_CurrentBaseId.Text = updatedId.Rows[0][0].ToString();
        }

        ClearContents();
        labelControl_InstalledNote.Text = "Base Session Id updated.";
    }

    /// <summary>
    /// Clears installed message notice.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void imageComboBoxEdit_LatestBaseId_SelectedIndexChanged(object sender, EventArgs e)
    {
        labelControl_InstalledNote.Text = "";
    }

    /// <summary>
    /// Runs load verification.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Form1_Shown(object sender, EventArgs e)
    {
        LoadConfigurations();
    }

    /// <summary>
    /// Verifies configuration file contains user host key and database connection string.
    /// </summary>
    private void LoadConfigurations()
    {
        string dbConnectionString;
        SimpleConfiguration configuration;
        MRdbInstallation dbInst = new ();
        try
        {
            try
            {
                configuration = new SimpleConfiguration();
                dbConnectionString = configuration.LoadValue("DBConnectionString");
            }
            catch
            {
                throw new Exception("SimpleConfiguration failed");
            }

            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                try
                {
                    m_mrSql = new MRSql(dbConnectionString);
                }
                catch
                {
                    throw new Exception("MRSql failed");
                }

                labelControl_dbConnection.Text = $"Connected to: {dbConnectionString}";

                if (dbInst.InstallVerification)
                {
                    try
                    {
                        LoadInstall();
                    }
                    catch
                    {
                        throw new Exception("LoadInstall failed");
                    }
                }
                else
                {
                    simpleButton_Install.Visible = true;
                    labelControl_InstalledNote.Text = "Need to install database first.";
                    tabPane_MRUI.SelectedPage = tabNavigationPage_Install;
                }
            }
            else
            {
                labelControl_Title.Text = "Setup Error Detected";
                string caption = "Exception: <{0}></{0}> configuration missing.";
                string message = "Add database connection for user {0} to MassRecordings App.MR.config file.";
                DialogResult result = MessageBox.Show(string.Format(message, m_userName), string.Format(caption, m_userName), MessageBoxButtons.OK);

                if (result == DialogResult.OK)
                {
                    Application.Exit();
                }
            }

            string defaultMasterCopyPath = configuration.LoadValue("MasterCopy");
            m_keyPaths = new Dictionary<string, string>();
            m_keyPaths.Add("MasterCopy", defaultMasterCopyPath);
        }
        catch (Exception ex)
        {
            string caption = "Setup error detected.";
            DialogResult result = MessageBox.Show(ex.Message, caption, MessageBoxButtons.OK);

            if (result == DialogResult.OK)
            {
                Application.Exit();
            }
        }
    }

    /// <summary>
    /// Ends runnings processes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_EndProcesses_Click(object sender, EventArgs e)
    {
        MRUtilities mrKill = new ();
        mrKill.KillMRProcesses();
    }

    /// <summary>
    /// Creates a timer
    /// </summary>
    private void CreateTimer()
    {
        m_timer = new System.Timers.Timer(5000);
        m_timer.Elapsed += UpdateBar;
    }

    /// <summary>
    /// Updates progress bar info
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void UpdateBar(object source, ElapsedEventArgs e)
    {
        m_timer.Stop();
        int numberOfPlayersOn = m_mrSql.GetNumberOfPlayersOn(m_currentSessionId);
        int numberOfPlayersCompleted = m_mrSql.GetNumberOfPlayersCompleted(m_currentSessionId);
        int numberOfPlayersRemaining = m_totalNumberPlayers - numberOfPlayersOn - numberOfPlayersCompleted;

        if (InvokeRequired)
        {
            Invoke(new Action(() => SetProgressBarPosition(numberOfPlayersCompleted, numberOfPlayersOn, numberOfPlayersRemaining)));
        }
        else
        {
            SetProgressBarPosition(numberOfPlayersCompleted, numberOfPlayersOn, numberOfPlayersRemaining);
        }
    }

    /// <summary>
    /// Changes progress bar position.
    /// </summary>
    private void SetProgressBarPosition(int a_numberOfPlayersCompleted, int a_numberOfPlayersOn, int a_numberOfPlayersRemaining)
    {
        progressBarControl_CompletedPlayers.Position = a_numberOfPlayersCompleted;
        textEdit_NumberPlayersRun.Text = a_numberOfPlayersCompleted.ToString();
        textEdit_NumberPlayersOn.Text = a_numberOfPlayersOn.ToString();
        textEdit_NumberPlayersRemaining.Text = a_numberOfPlayersRemaining.ToString();

        DataTable endTime = m_mrSql.GetEndTime(m_currentSessionId);
        if (endTime != null && endTime.Rows.Count > 0)
        {
            textEdit_EndTime.Text = endTime.Rows[0][m_runInstance.EndTime].ToString();
        }

        if (!string.IsNullOrEmpty(textEdit_EndTime.Text))
        {
            labelControl_AnalysisMessage.Text = "Session completed";
        }
        else
        {
            m_timer.Start();
        }
    }

    /// <summary>
    /// Initialize progress bare
    /// </summary>
    private void SetupProgressBar(int a_totalNumberPlayers)
    {
        // Initializing progress bar properties
        progressBarControl_CompletedPlayers.Properties.PercentView = true;
        progressBarControl_CompletedPlayers.Properties.Maximum = a_totalNumberPlayers;
        progressBarControl_CompletedPlayers.Properties.Minimum = 0;
        progressBarControl_CompletedPlayers.Properties.PercentView = true;
        progressBarControl_CompletedPlayers.Position = 0;
        progressBarControl_CompletedPlayers.Visible = true;
        progressBarControl_CompletedPlayers.Properties.ShowTitle = true;
    }

    /// <summary>
    /// Performs action based on selected tab.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void tabPane_MRUI_SelectedPageChanged(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
    {
        if (tabPane_MRUI.SelectedPage == tabNavigationPage_Analysis)
        {
            labelControl_AnalysisMessage.Text = "";
            RunAnalysis(sender, e);
        }

        if (tabPane_MRUI.SelectedPage == tabNavigationPage_Results)
        {
            if (imageComboBoxEdit_RunInstances.Properties.Items.Count == 0)
            {
                LoadInstances();
            }
        }
    }

    /// <summary>
    /// Begins analysis of current session running.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RunAnalysis(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
    {
        try
        {
            SimpleConfiguration configuration = new ();
            string dbConnectionString = configuration.LoadValue("DBConnectionString");
            m_mrSql = new MRSql(dbConnectionString);
            m_currentSessionId = m_mrSql.GetCurrentSessionIdUsingEndTime();
            if (m_currentSessionId != -1)
            {
                m_totalNumberPlayers = m_mrSql.GetTotalNumberOfTests(m_currentSessionId);

                SetupProgressBar(m_totalNumberPlayers);
                CreateTimer();
                m_timer.Start();
                m_runInstance = new RunInstance();
                DataTable instanceData = m_mrSql.GetInstanceData(m_currentSessionId);
                if (instanceData != null && instanceData.Rows.Count > 0)
                {
                    textEdit_StartTime.Text = instanceData.Rows[0][m_runInstance.StartTime].ToString();
                    textEdit_RunLocation.Text = instanceData.Rows[0][m_runInstance.RunLocation].ToString();
                }
            }
            else
            {
                throw new Exception("No sessions currently running to analyze.");
            }
        }
        catch (Exception ex)
        {
            string caption = $"Exception: Analysis error - {ex.Message}";
            string message = "In Visual Studio Test Explorer: Select Run or Run All to run tests then Click Retry below to begin analysis\n\n Click Cancel to stop analysis";
            DialogResult result = MessageBox.Show(message, string.Format(caption, m_userName), MessageBoxButtons.RetryCancel);
            if (result == DialogResult.Retry)
            {
                labelControl_AnalysisMessage.Text = "";
                tabPane_MRUI_SelectedPageChanged(sender, e);
            }
        }
    }

    /// <summary>
    /// Begin load run instances process
    /// </summary>
    private void LoadInstances()
    {
        Task.Run(() => LoadInstanceData());
    }

    private void StartLoadingMarquee()
    {
        marqueeProgressBarControl_Loading.Visible = true;
    }

    /// <summary>
    /// Compile run instance data from database
    /// </summary>
    private void LoadInstanceData()
    {
        int runIdFirstValue = 0;
        DataTable instances = m_mrSql.GetRunInstances();
        if (instances != null && instances.Rows.Count > 0)
        {
            List<Tuple<string, int>> instancesRun = new ();
            RunInstance runInstance = new ();
            foreach (DataRow row in instances.Rows)
            {
                int index = instances.Rows.IndexOf(row);
                if (index == 0)
                {
                    runIdFirstValue = Convert.ToInt32(row[runInstance.InstanceId]);
                }

                string currentRun = $"{row[runInstance.InstanceId]}  | {row[runInstance.RunLocation]} | ({row[runInstance.StartTime]})";
                instancesRun.Add(new Tuple<string, int>(currentRun, Convert.ToInt32(row[runInstance.InstanceId])));
            }

            Invoke(new Action(() => DisplayInstanceData(instancesRun, runIdFirstValue)));
        }
    }

    /// <summary>
    /// Display  runinstance dropdown and select latest instance
    /// </summary>
    /// <param name="a_instancesRun"></param>
    /// <param name="a_runIdFirstValue"></param>
    private void DisplayInstanceData(List<Tuple<string, int>> a_instancesRun, int a_runIdFirstValue)
    {
        foreach (Tuple<string, int> instance in a_instancesRun)
        {
            imageComboBoxEdit_RunInstances.Properties.Items.Add(new ImageComboBoxItem(instance.Item1, instance.Item2, 0));
        }

        imageComboBoxEdit_RunInstances.EditValue = a_runIdFirstValue;
    }

    /// <summary>
    /// Gather top recordings data for time and warnings count.
    /// </summary>
    /// <param name="a_sessionId"></param>
    private void LoadTopRecordings(long a_sessionId)
    {
        Invoke(new Action(() => DisplayTopRecordings("Loading...", "Loading...")));
        //highest warnings
        DataTable topWarnings = m_mrSql.GetTopWarnings(a_sessionId);
        m_topRecordingsWarn = new StringBuilder();
        if (topWarnings != null && topWarnings.Rows.Count > 0)
        {
            foreach (DataRow row in topWarnings.Rows)
            {
                string warning = $"-----------{Environment.NewLine}Recording Path:{Environment.NewLine}{row["RecordingPath"]}{Environment.NewLine}{Environment.NewLine}Warning Count:{Environment.NewLine}{row["WarningCount"]}{Environment.NewLine}{Environment.NewLine}";
                m_topRecordingsWarn.Append(warning);
            }
        }

        //longest time
        DataTable topTimes = m_mrSql.GetTopTimes(a_sessionId);
        m_topRecordingsTime = new StringBuilder();
        if (topTimes != null && topTimes.Rows.Count > 0)
        {
            foreach (DataRow row in topTimes.Rows)
            {
                string time = $"-----------{Environment.NewLine}Recording Path:{Environment.NewLine}{row["RecordingPath"]}{Environment.NewLine}{Environment.NewLine}Seconds:{Environment.NewLine}{row["Seconds"]}{Environment.NewLine}{Environment.NewLine}";
                m_topRecordingsTime.Append(time);
            }
        }

        Invoke(new Action(() => DisplayTopRecordings(m_topRecordingsWarn.ToString(), m_topRecordingsTime.ToString())));
    }

    /// <summary>
    /// Display top recordings info on ui thread
    /// </summary>
    /// <param name="a_topRecordingsWarn"></param>
    /// <param name="a_topRecordingsTime"></param>
    private void DisplayTopRecordings(string a_topRecordingsWarn, string a_topRecordingsTime)
    {
        memoEdit_TopRecordingsWarnings.Text = a_topRecordingsWarn;
        memoEdit_TopRecordingsLength.Text = a_topRecordingsTime;
    }

    /// <summary>
    /// Changes selected run instance and triggers data display.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void imageComboBoxEdit_RunInstances_SelectedIndexChanged(object sender, EventArgs e)
    {
        StartLoadingMarquee();
        RefreshResults();
        simpleButton_ShowWarnings.Visible = false;
        simpleButton_GatherWarnings.Visible = true;
        simpleButton_GatherWarnings.Enabled = true;
        long sessionId = Convert.ToInt64(imageComboBoxEdit_RunInstances.EditValue);
        imageComboBoxEdit_RunInstances.Properties.ReadOnly = true;
        Task.Run(() => LoadSelectedInstance(sessionId));
        Task.Run(() => LoadTopRecordings(sessionId));
    }

    /// <summary>
    /// Gathers all instance data for display.
    /// </summary>
    /// <param name="a_sessionId"></param>
    private void LoadSelectedInstance(long a_sessionId)
    {
        int totalPlayers = m_mrSql.GetTotalNumberOfTests(a_sessionId);
        int numPlayersRun = m_mrSql.GetNumberOfPlayersCompleted(a_sessionId);
        int numWithErrors = m_mrSql.GetErrorRecordings(a_sessionId);
        string percentNoErrors = "";
        if (numPlayersRun > 0)
        {
            double percent = (double)(numPlayersRun - numWithErrors) / numPlayersRun * 100;
            percentNoErrors = percent.ToString("#.##");
        }

        DataTable warnings = m_mrSql.GetMRWarningsTable(a_sessionId);
        int numWarnings = warnings.Rows.Count;
        Invoke(new Action(() => DisplaySelectedInstance(totalPlayers, numPlayersRun, numWithErrors, percentNoErrors, numWarnings)));
        Invoke(new Action(() => HideLoadingMarquee()));
    }

    /// <summary>
    /// Hides loading marquee
    /// </summary>
    private void HideLoadingMarquee()
    {
        marqueeProgressBarControl_Loading.Visible = false;
    }

    /// <summary>
    /// Displays selected run instance data on ui thread.
    /// </summary>
    /// <param name="a_totalPlayers"></param>
    /// <param name="a_numPlayersRun"></param>
    /// <param name="a_numWithErrors"></param>
    /// <param name="a_percentNoErrors"></param>
    /// <param name="a_numWarnings"></param>
    private void DisplaySelectedInstance(int a_totalPlayers, int a_numPlayersRun, int a_numWithErrors, string a_percentNoErrors, int a_numWarnings)
    {
        textEdit_ResultsTotalNumberPlayers.Text = a_totalPlayers.ToString();
        textEdit_ResultsNumberPlayersRun.Text = a_numPlayersRun.ToString();
        textEdit_NumPlayersWithErrors.Text = a_numWithErrors.ToString();
        textEdit_PercentNoErrors.Text = a_percentNoErrors;
        textEdit_NumWarnings.Text = a_numWarnings.ToString();
        imageComboBoxEdit_RunInstances.Properties.ReadOnly = false;
    }

    /// <summary>
    /// Builds string of warnings data for run instance
    /// </summary>
    /// <param name="a_warnings"></param>
    /// <returns></returns>
    private string CompileWarnings(DataTable a_warnings)
    {
        StringBuilder warningDetails = new ();
        Invoke(new Action(() => ShowWarningsLoading()));
        if (a_warnings != null && a_warnings.Rows.Count > 0)
        {
            foreach (DataRow row in a_warnings.Rows)
            {
                string warning = $"----------------------{Environment.NewLine}Warning: {a_warnings.Rows.IndexOf(row)}{Environment.NewLine}{Environment.NewLine}Recording Path:{Environment.NewLine}{row["RecordingPath"]}{Environment.NewLine}{Environment.NewLine}Warning Message:{Environment.NewLine}{row["WarningMessage"]}{Environment.NewLine}{Environment.NewLine}Warning Type:{Environment.NewLine}{row["WarningType"]}{Environment.NewLine}{Environment.NewLine}----------------------";
                warningDetails.Append(warning);
            }
        }

        Invoke(new Action(() => ShowWarningsButton()));
        return warningDetails.ToString();
    }

    /// <summary>
    /// Shows warnings marquee
    /// </summary>
    private void ShowWarningsLoading()
    {
        marqueeProgressBarControl_GatherWarnings.Visible = true;
    }

    /// <summary>
    /// Sets component visibility on task completion.
    /// </summary>
    private void ShowWarningsButton()
    {
        simpleButton_GatherWarnings.Visible = false;
        simpleButton_ShowWarnings.Visible = true;
        marqueeProgressBarControl_GatherWarnings.Visible = false;
    }

    /// <summary>
    /// Clears ui components data.
    /// </summary>
    private void RefreshResults()
    {
        textEdit_ResultsTotalNumberPlayers.Text = "";
        textEdit_ResultsNumberPlayersRun.Text = "";
        textEdit_NumPlayersWithErrors.Text = "";
        textEdit_PercentNoErrors.Text = "";
        textEdit_NumWarnings.Text = "";
        memoEdit_TopRecordingsLength.Text = "";
        memoEdit_TopRecordingsWarnings.Text = "";
    }

    /// <summary>
    /// Opens popup displaying all warnings details.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void simpleButton_ShowWarnings_Click(object sender, EventArgs e)
    {
        WarningsPopup popup = new ();
        popup.DisplayWarnings(m_warningsCompiled);
        popup.Show();
    }

    /// <summary>
    /// Initiates warnings details retrieval from database.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void simpleButton_GatherWarnings_Click(object sender, EventArgs e)
    {
        simpleButton_GatherWarnings.Enabled = false;
        long sessionId = Convert.ToInt64(imageComboBoxEdit_RunInstances.EditValue);
        ShowWarningsLoading();
        m_warningsCompiled = await Task.Run(() => CompileWarnings(m_mrSql.GetMRWarningsTable(sessionId)));
        marqueeProgressBarControl_GatherWarnings.Visible = false;
    }
}