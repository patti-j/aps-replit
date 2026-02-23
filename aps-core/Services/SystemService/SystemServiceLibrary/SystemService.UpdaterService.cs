using System.IO.Compression;
using System.Xml;

using PT.Common.Exceptions;

namespace PT.SystemServiceLibrary;

public class SystemService
{
    private const string c_configFileName = "UpdateFiles.Config";
    private List<UpdateFile> m_updateFiles;
    private readonly string m_workingDirectory;
    private readonly XmlDocument m_doc;
    private readonly object m_fileGatheringLock = new ();

    private string AlertsDirectory => Path.Combine(m_workingDirectory, "Alerts");

    private string UpdateFilesDirectory => Path.Combine(m_workingDirectory, "UpdateFiles");

    private string LogFile => Path.Combine(m_workingDirectory, "UpdaterService.log");

    private string ConfigFile => Path.Combine(m_workingDirectory, c_configFileName);

    /// <summary>
    /// Initializes the UpdateFileManager. When GetFileList is called, a list of the current files is gathered.
    /// Calling GetFiles will return the files found the last time GetFileList was called.
    /// </summary>
    /// <param name="a_baseDirectory">The directory where the file are located.</param>
    /// <param name="a_aWorkingDirectory">The directory where the zip program is free to operate. Zip will operate in the zip subdirectory of this directory.</param>
    public SystemService(string a_workingDirectory)
    {
        m_workingDirectory = a_workingDirectory;

        if (!Directory.Exists(AlertsDirectory))
        {
            Directory.CreateDirectory(AlertsDirectory);
        }

        if (!File.Exists(ConfigFile))
        {
            string programFilesVersion = Path.Combine(a_workingDirectory, "UpdateFiles", c_configFileName);
            if (!File.Exists(programFilesVersion))
            {
                throw new FileNotFoundException("", programFilesVersion);
            }

            File.Copy(programFilesVersion, ConfigFile);
        }

        XmlTextReader reader = new (ConfigFile);
        m_doc = new XmlDocument();
        m_doc.Load(reader);
        reader.Close();
    }

    public void UpdateUserFiles(string a_userId, byte[] a_zippedBytes)
    {
        lock (m_fileGatheringLock)
        {
            try
            {
                string targetDir = Path.Combine(UpdateFilesDirectory, "UserSettings", a_userId);
                string tempFile = Path.GetTempFileName();
                //Remove whole directory to synch files. 
                if (Directory.Exists(targetDir))
                {
                    Common.File.FileUtils.DeleteDirectoryRecursivelyWithRetry(targetDir);
                }

                //Create a temp zip file, extract it, and then delete it.
                File.WriteAllBytes(tempFile, a_zippedBytes);
                ZipFile.ExtractToDirectory(tempFile, targetDir);
                File.Delete(tempFile);
            }
            catch (Exception e)
            {
                throw new PTException("4119", e);
            }
        }
    }

    /// <summary>
    /// Client calls this to get a list of files.
    /// </summary>
    /// <param name="a_clientFiles">files client has already received with their last modified date. Can be empty but not null</param>
    /// <returns></returns>
    public List<Tuple<string, long, byte[]>> GetFiles(Dictionary<string, long> a_clientFiles)
    {
        lock (m_fileGatheringLock)
        {
            // Load files into m_updateFiles array
            GetUpdateFiles();

            List<Tuple<string, long, byte[]>> files = new ();
            HashSet<string> allServerFiles = new ();

            foreach (UpdateFile loadedFile in m_updateFiles)
            {
                if (a_clientFiles.ContainsKey(loadedFile.m_relativePath)) // client has this file
                {
                    if (a_clientFiles[loadedFile.m_relativePath] != loadedFile.m_modifiedDate) // not the same version of the file, needs it
                    {
                        files.Add(new Tuple<string, long, byte[]>(loadedFile.m_relativePath, loadedFile.m_modifiedDate, loadedFile.m_compressed));
                    }
                }
                else // new
                {
                    files.Add(new Tuple<string, long, byte[]>(loadedFile.m_relativePath, loadedFile.m_modifiedDate, loadedFile.m_compressed));
                }

                if (!allServerFiles.Contains(loadedFile.m_relativePath))
                {
                    allServerFiles.Add(loadedFile.m_relativePath);
                }
            }

            // these files will be deleted on the client
            foreach (KeyValuePair<string, long> kv in a_clientFiles)
            {
                if (!allServerFiles.Contains(kv.Key)) // server doesn't have this. add Update file with null compressed bytes so it's removed.
                {
                    files.Add(new Tuple<string, long, byte[]>(kv.Key, kv.Value, null));
                }
            }

            return files;
        }
    }

    #region Load Functions
    /// <summary>
    /// Call this to load all files into memory based on xml document.
    /// </summary>
    private void GetUpdateFiles()
    {
        m_updateFiles = new List<UpdateFile>();

        try
        {
            LoadRecursiveDirectories(m_doc);
            LoadDirectories(m_doc);
            LoadFiles(m_doc);
        }
        catch (CommonException ex)
        {
            if (!Directory.Exists(AlertsDirectory))
            {
                Directory.CreateDirectory(AlertsDirectory);
            }

            Common.File.SimpleExceptionLogger.LogException(LogFile, ex, ex.Message);
        }
    }

    private void LoadRecursiveDirectories(XmlDocument doc)
    {
        XmlNodeList recursiveDirectoryNodes = doc.SelectNodes("Directories/RecursiveDirectories/Directory");

        for (int recursiveDirectoryNodeI = 0; recursiveDirectoryNodeI < recursiveDirectoryNodes.Count; ++recursiveDirectoryNodeI)
        {
            XmlNode recursiveDirectoryNode = recursiveDirectoryNodes[recursiveDirectoryNodeI];
            string directory = recursiveDirectoryNode.InnerText;
            LoadRecursiveDirectory(Path.Combine(UpdateFilesDirectory, directory));
        }
    }

    private void LoadRecursiveDirectory(string directory)
    {
        LoadDirectory(directory);

        string[] directories = Directory.GetDirectories(directory);
        for (int directoryI = 0; directoryI < directories.Length; ++directoryI)
        {
            LoadRecursiveDirectory(directories[directoryI]);
        }
    }

    private void LoadDirectories(XmlDocument doc)
    {
        XmlNodeList directoryNodes = doc.SelectNodes("Directories/Directories/Directory");
        for (int directoryNodeI = 0; directoryNodeI < directoryNodes.Count; ++directoryNodeI)
        {
            XmlNode directoryNode = directoryNodes[directoryNodeI];
            string directory = directoryNode.InnerText;
            LoadDirectory(Path.Combine(UpdateFilesDirectory, directory));
        }
    }

    private void LoadDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new PTException("2582", new object[] { directory });
        }

        string[] files = Directory.GetFiles(directory);
        for (int fileI = 0; fileI < files.Length; ++fileI)
        {
            UpdateFile updateFile = new (files[fileI]);
            m_updateFiles.Add(updateFile);
        }
    }

    private void LoadFiles(XmlDocument doc)
    {
        XmlNodeList fileNodes = doc.SelectNodes("Directories/Files/File");
        for (int fileNodeI = 0; fileNodeI < fileNodes.Count; ++fileNodeI)
        {
            XmlNode fileNode = fileNodes[fileNodeI];
            string fileName = fileNode.InnerText;
            UpdateFile updateFile = new (fileName);
            m_updateFiles.Add(updateFile);
        }
    }
    #endregion
}