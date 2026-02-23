using PT.Common.Sql.SqlServer;

namespace MassRecordingsTest;

public class MasterCopySync
{
    private static object s_dbLock = new ();

    /// <summary>
    /// Downloads MR Data to local disk from the shared drive
    /// </summary>
    /// <param name="a_sourcePath">Shared drive path</param>
    /// <param name="a_destPath">Local drive path</param>
    /// <param name="a_sessionId">The current session ID</param>
    /// <param name="a_recordingPath">Recording path to log in database in case on an exception</param>
    /// <param name="a_dbConnector">The database connector</param>
    public MasterCopySync(string a_sourcePath, string a_destPath, long a_sessionId, string a_recordingPath, DatabaseConnections a_dbConnector)
    {
        string fileName = "";
        string fileNameNoZip = "";

        try
        {
            int temp = a_recordingPath.LastIndexOf("\\");
            fileNameNoZip = a_recordingPath.Substring(temp + 1, a_recordingPath.Length - temp - 1);
            fileName = fileNameNoZip + ".zip";

            if (!File.Exists(Path.Combine(a_sourcePath, fileName)))
            {
                throw new Exception("The file could not be downloaded from the shared folder: " + a_recordingPath + ".zip");
            }

            //Create customer directory if it does not exist.
            bool customerExists = Directory.Exists(a_destPath);
            if (!customerExists)
            {
                Directory.CreateDirectory(a_destPath);
            }

            //If recording directory exists, look for .DAT file. If there is one, there is no need to copy from the shared folder
            if (Directory.Exists(Path.Combine(a_destPath, fileNameNoZip)))
            {
                string recordingFileExists = ProcessDirectory(Path.Combine(a_destPath, fileNameNoZip));
                if (!string.IsNullOrEmpty(recordingFileExists)) { }
            }

            ////Download recording from the shared folder and unzip the file in the local recording folder. Delete zip folder afterwards.
            else
            {
                File.Copy(Path.Combine(a_sourcePath, fileName), Path.Combine(a_destPath, fileName), true);

                string newDir = Path.Combine(a_destPath, fileNameNoZip);
                PT.Common.Compression.Zip.Extract(Path.Combine(a_destPath, fileName), newDir);
                PT.Common.File.FileUtils.Delete(Path.Combine(a_destPath, fileName));
            }
        }
        catch (Exception e)
        {
            if (File.Exists(Path.Combine(a_destPath, fileName)))
            {
                PT.Common.File.FileUtils.Delete(Path.Combine(a_destPath, fileName));
                FileInfo info = new (Path.Combine(a_destPath, fileNameNoZip));
                info.MoveTo(Path.Combine(a_destPath, "BAD_FOLDER_" + fileNameNoZip));
            }

            throw e;
        }
    }

    public static string ProcessDirectory(string targetDirectory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory);
        foreach (string fileName in fileEntries)
        {
            if (fileName.ToUpper().EndsWith(".DAT"))
            {
                return targetDirectory;
            }
        }

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
        {
            string dir = ProcessDirectory(subdirectory);
            if (!string.IsNullOrEmpty(dir))
            {
                return dir;
            }
        }

        return string.Empty;
    }
}