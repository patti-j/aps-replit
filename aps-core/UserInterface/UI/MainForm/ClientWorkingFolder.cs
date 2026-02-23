using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.APSCommon.Windows.Extensions;
using PT.Common.File;
using PT.Common.Localization;
using PT.PackageDefinitionsUI.Controls.MessageControl;

internal class ClientWorkingFolder
{
    internal ClientWorkingFolder(string a_instanceName)
    {
        InitializeAppDataPath();
        GetUnusedWorkingFolder(a_instanceName);
    }

    private string m_workingSessionFolderPath;

    public string WorkingSessionFolderPath => m_workingSessionFolderPath;

    private string m_workingFolderPath;

    internal string WorkingFolderPath => m_workingFolderPath;

    private static string s_userAppDataPath;

    /// <summary>
    /// Folder where Session folders are stored for client sessions.
    /// </summary>
    internal static string ClientDataFolder => s_userAppDataPath;

    /// <summary>
    /// Get the app data path based on the assembly version number.
    /// This is used instead of simply Application.UserAppDataPath so that the version number matches the display verion
    /// </summary>
    private static void InitializeAppDataPath()
    {
        try
        {
            s_userAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlanetTogether", "PlanetTogetherClient", AssemblyVersionChecker.GetAssemblyVersion().ToString());

            //It isn't necessary to use assembly version since Application takes that into account already. If a custom folder is needed, use code below as an example.
            //SoftwareVersion assemblyVersion = AssemblyVersionChecker.GetAssemblyVersion();
            //string appDataPath = Directory.GetParent(Application.UserAppDataPath).FullName;
            //s_userAppDataPath = Path.Combine(appDataPath, assemblyVersion.ToString());
        }
        catch
        {
            s_userAppDataPath = Application.UserAppDataPath;
        }
    }

    private void GetUnusedWorkingFolder(string a_instanceName)
    {
        int folderIndex = 1;
        bool haveFolder = false;
        string workingFolder = "";
        m_workingFolderPath = Path.Combine(ClientDataFolder, a_instanceName);
        //Get a new or unused folder
        while (!haveFolder)
        {
            workingFolder = Path.Combine(m_workingFolderPath, "Session" + folderIndex);
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
                haveFolder = true;
            }
            else if (!FolderInUse(workingFolder))
            {
                haveFolder = true;
            }
            else
            {
                folderIndex++;
            }
        }

        FlagFolderAsInUse(workingFolder);
        m_workingSessionFolderPath = workingFolder;

        //Do this after creating the new session so the previous session history is preserved if the same session folder is used.  Might help later to get logs.
        if (Directory.Exists(ClientDataFolder)) //may not have saved any sessions yet.
        {
            RemoveRemnantFolders();
        }
    }

    private bool FolderInUse(string folder)
    {
        string flagFile = GetFlagFileName(folder);
        if (File.Exists(flagFile))
        {
            //see if the file is really old and assumed therefore to be junk from a terminated session 
            DateTime fileLastWrite = File.GetLastWriteTime(flagFile);
            #if DEBUG
            if (fileLastWrite < DateTime.Now.AddDays(-2)) //accumulate lots of junk from stopping sessions in debugger
            {
                return false;
            }
            #endif
            if (fileLastWrite > DateTime.Now.AddDays(-7)) //assume junk if older than a week.
            {
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Removes the in-use flag.
    /// Keep the folder until the next session in case there are logs or other data that we want to diagnose problems.
    /// </summary>
    private void ClearFlagFile()
    {
        if (File.Exists(GetFlagFileName(WorkingSessionFolderPath)))
        {
            File.Delete(GetFlagFileName(WorkingSessionFolderPath));
        }
    }

    /// <summary>
    /// Remove session folders that are a result of a session crashing and leaving the in-use flag set or just an old session no longer in use.
    /// </summary>
    private void RemoveRemnantFolders()
    {
        //Iterate through the existing folders and look for folders with no in-use flag or an out-dated in-use flag.
        string[] clientFolders = Directory.GetDirectories(ClientDataFolder, "*Session*");
        for (int i = clientFolders.Length - 1; i >= 0; i--)
        {
            string folder = clientFolders.GetValue(i).ToString();
            if (!FolderInUse(folder))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception e)
                {
                    string msg = Localizer.GetErrorString("2443", new object[] { folder, e.Message }, true);
                    PTMessage message = e.CreatePTMessage();
                    message.Message = msg;
                    using (PTMessageForm messageForm = new (true))
                    {
                        messageForm.Show(message);
                    }
                    //m_messageProvider.ShowMessageBox(msg, "Folder Cleanup", MessageBoxButtons.OK, MessageBoxIcon.Warning, true);
                }
            }
        }
    }

    private string GetFlagFileName(string folder)
    {
        return Path.Combine(folder, "FolderInUseFlag.txt");
    }

    private void FlagFolderAsInUse(string a_folder)
    {
        TextFile flagFile = new ();
        flagFile.AppendText(string.Format("File created: {0}", DateTime.Now));
        flagFile.WriteFile(GetFlagFileName(a_folder));
    }

    #region IDisposable Members
    public void Dispose()
    {
        ClearFlagFile();
    }
    #endregion
}