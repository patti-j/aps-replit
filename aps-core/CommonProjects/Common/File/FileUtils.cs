using System.Diagnostics;
using System.IO.Compression;

namespace PT.Common.File;

/// <summary>
/// Summary description for FileUtils.
/// </summary>
public class FileUtils
{
    /// <summary>
    /// returns true if a_fileName is not null, whitespace, empty, larger than 255 characters or contains any invalid characters.
    /// It does not check whether the file already exists or can be writter or read.
    /// </summary>
    /// <param name="a_fileName"></param>
    /// <returns></returns>
    public static bool IsValidFileName(string a_fileName)
    {
        return !string.IsNullOrWhiteSpace(a_fileName) && a_fileName != "" && a_fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1 && a_fileName.Length < 255;
    }

    /// <summary>
    /// Save a stream to a temp file with an .htm extension and launches it to open it in a browser.
    /// </summary>
    /// <param name="a_inStream"></param>
    public static void OpenStreamDataInBrowser(Stream a_inStream)
    {
        // Create a temporary file to save to
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache),
            Guid.NewGuid() + ".htm");

        using (StreamWriter writer = System.IO.File.CreateText(filePath))
        {
            StreamReader reader = new (a_inStream);
            writer.Write(reader.ReadToEnd());
        }

        // Open the file in a browser
        Process.Start(filePath);
    }

    public static string CleanPath(string a_path)
    {
        a_path = a_path.Trim();
        a_path = a_path.TrimEnd(Path.AltDirectorySeparatorChar);
        return a_path;
    }

    public static byte[] GetBinaryFile(string a_path)
    {
        using (FileStream fs = System.IO.File.OpenRead(a_path))
        {
            byte[] file = new byte[fs.Length];
            int intLen = (int)fs.Length;
            fs.Read(file, 0, intLen);

            return file;
        }
    }

    public static void SaveBinaryFile(string a_path, byte[] a_binaryData)
    {
        using (FileStream fs = System.IO.File.Create(a_path))
        {
            fs.Write(a_binaryData, 0, a_binaryData.Length);
        }
    }

    public static void SaveStreamToFile(string a_fileFullPath, Stream a_stream)
    {
        if (a_stream.Length == 0)
        {
            return;
        }

        // Create a FileStream object to write a stream to a file     
        using (FileStream fileStream = System.IO.File.Create(a_fileFullPath, (int)a_stream.Length))
        {
            // Fill the bytes[] array with the stream data         
            byte[] bytesInStream = new byte[a_stream.Length];
            a_stream.Read(bytesInStream, 0, bytesInStream.Length);
            // Use FileStream object to write to the specified file         
            fileStream.Write(bytesInStream, 0, bytesInStream.Length);
        }
    }

    /// <summary>
    /// Return the full path of the currently executing application's configuration file.
    /// </summary>
    public static string AppConfigFileName => Environment.GetCommandLineArgs()[0] + ".config";

    /// <summary>
    /// Return the full path of the currently executing application.
    /// </summary>
    public static string AppFilePath => Environment.GetCommandLineArgs()[0];

    /// <summary>
    /// Validates essential parts of the App.config file.  Writes any errors to the DesktopPath.  Exits if the error is fatal.
    /// </summary>
    //public static void ValidateConfigFile()
    //{
    //    ValidateConfigFile(false);
    //}

    ///// <summary>
    ///// Validates essential parts of the App.config file.  Writes any errors to the DesktopPath.  Exits if the error is fatal.
    ///// </summary>
    //public static void ValidateConfigFile(bool a_ignoreWorkingDirectory)
    //{
    //    string error = "";
    //    try
    //    {
    //        Hashtable PTSystemConfig = (Hashtable)ConfigurationSettings.GetConfig("PTSystem");

    //        if (PTSystemConfig == null)
    //        {
    //            error = "'PTSystem' section not found in config file: " + AppConfigFileName;
    //            SimpleExceptionLogger.PTDefaultLog(null, error);
    //            Environment.Exit(-1);
    //        }

    //        if (!a_ignoreWorkingDirectory)
    //        {
    //            string workingDirectory = (string)PTSystemConfig["workingDirectory"];

    //            error = String.Format("Could not create the Working Directory at {0}, as specified in {1}.", workingDirectory,
    //                AppConfigFileName);

    //            if (!System.IO.Directory.Exists(workingDirectory))
    //            {
    //                System.IO.Directory.CreateDirectory(workingDirectory);
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        SimpleExceptionLogger.PTDefaultLog(e, error);
    //        throw;
    //    }
    //}

    /// <summary>
    /// Delete all the specified files. The files are deleted irregardless of their read-only setting. Their attributes are set to Normal before the delete is attempted.
    /// </summary>
    /// <param name="a_files">The files to delete.</param>
    public static void Delete(string[] a_files)
    {
        foreach (string element in a_files)
        {
            //Attributes can not be set on shortcuts
            if (!element.EndsWith(".lnk"))
            {
                System.IO.File.SetAttributes(element, FileAttributes.Normal);
            }

            System.IO.File.Delete(element);
        }
    }

    /// <summary>
    /// Delete a file regardless of its read-only setting. The files attributes are set to Normal before the delete is attempted (readonly is cleared).
    /// </summary>
    /// <param name="a_fileName"></param>
    public static void Delete(string a_fileName)
    {
        string[] files = new string[1];
        files[0] = a_fileName;
        Delete(files);
    }

    /// <summary>
    /// Attempts to delete a directory.
    /// This accounts for windows explorer and various file locks
    /// This does not account for readonly attribute
    /// </summary>
    /// <returns></returns>
    public static bool DeleteDirectoryRecursivelyWithRetry(string a_directory)
    {
        const int c_retryAttempts = 10;
        for (int attemptI = 0; attemptI < c_retryAttempts; attemptI++)
        {
            try
            {
                System.IO.Directory.Delete(a_directory, true);
            }
            catch (DirectoryNotFoundException)
            {
                //Directory was deleted
                return true;
            }
            catch (IOException)
            {
                // System.IO.IOException: The directory is not empty
                Thread.Sleep(50);
                continue;
            }

            return true;
        }

        //Directory could not be deleted
        return false;
    }

    /// <summary>
    /// Attempts to delete all files in a directory but not the directory.
    /// This accounts for windows explorer and various file locks
    /// This does not account for readonly attribute
    /// </summary>
    /// <returns></returns>
    public static bool DeleteFilesWithRetry(string a_directory, string a_searchPatern)
    {
        string[] files = System.IO.Directory.GetFiles(a_directory, a_searchPatern);
        int retryAttempts = files.Length * 5;
        for (int attemptI = 0; attemptI < retryAttempts; attemptI++)
        {
            try
            {
                foreach (string file in files)
                {
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (IOException)
            {
                // System.IO.IOException: The directory is not empty
                Thread.Sleep(50);
                continue;
            }

            return true;
        }

        //Files could not be deleted
        return false;
    }

    /// <summary>
    /// Clear the read-only attribute of a file without affecting any other attributes.
    /// </summary>
    /// <param name="a_filePath"></param>
    public static void RemoveReadOnlyAttribute(string a_filePath)
    {
        FileAttributes fa = System.IO.File.GetAttributes(a_filePath);
        int readOnly = (int)FileAttributes.ReadOnly & (int)fa;
        if (readOnly == (int)FileAttributes.ReadOnly)
        {
            int newFA = (int)fa & ~(int)FileAttributes.ReadOnly;
            System.IO.File.SetAttributes(a_filePath, (FileAttributes)newFA);
        }
    }

    /// <summary>
    /// given a_filePath it returns "a_filePath" or "a_filePath (x)" that corresponds to no file and
    /// x is an integer >= 2
    /// </summary>
    /// <param name="a_filePath"></param>
    /// <returns></returns>
    public static string GetUniqueFileName(string a_filePath)
    {
        int i = 2;
        while (System.IO.File.Exists(a_filePath))
        {
            string newName = string.Format("{0} ({1}).{2}", Path.GetFileNameWithoutExtension(a_filePath), i, Path.GetExtension(a_filePath));
            a_filePath = Path.Combine(Path.GetDirectoryName(a_filePath), newName);
            i++;
        }

        return a_filePath;
    }

    /// <summary>
    /// Copy a directory (and sub directories) to a new location, skipping any directories with names in a_dirsToSkip.
    /// </summary>
    public static void CopyDirectory(DirectoryInfo a_fromDir, DirectoryInfo a_toDir, HashSet<string> a_dirsToSkip, bool a_overwrite)
    {
        if (!a_fromDir.Exists)
        {
            return;
        }

        if (!a_toDir.Exists)
        {
            a_toDir.Create();
        }

        foreach (FileInfo fileInfo in a_fromDir.EnumerateFiles())
        {
            string toPath = Path.Combine(a_toDir.FullName, fileInfo.Name);
            if (System.IO.File.Exists(toPath))
            {
                if (a_overwrite)
                {
                    System.IO.File.Delete(toPath);
                    fileInfo.CopyTo(toPath);
                }
            }
            else
            {
                fileInfo.CopyTo(toPath);
            }
        }

        foreach (DirectoryInfo dirInfo in a_fromDir.EnumerateDirectories())
        {
            if (!a_dirsToSkip.Contains(dirInfo.Name))
            {
                DirectoryInfo newSubDirInfo = new (Path.Combine(a_toDir.FullName, dirInfo.Name));
                CopyDirectory(dirInfo, newSubDirInfo, a_dirsToSkip, a_overwrite);
            }
        }
    }

    public static FileAttributes RemoveAttribute(FileAttributes a_attrs, FileAttributes a_attrsToRemove)
    {
        return a_attrs & ~a_attrsToRemove;
    }

    #region Packages
    /// <summary>
    /// Extracts all package files in all sub directories of the zip into the directory specified
    /// </summary>
    /// <param name="a_fileBytes"></param>
    /// <param name="a_baseDirectory"></param>
    public static void ExtractPackagesToRootDirectory(byte[] a_fileBytes, string a_baseDirectory)
    {
        Directory.DirectoryUtils.ValidateDirectory(a_baseDirectory);

        using (MemoryStream stream = new (a_fileBytes))
        {
            using (ZipArchive zip = new (stream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.FullName.EndsWith("/"))
                    {
                        //This is a directory, skip.
                    }
                    else
                    {
                        string newPath = Path.Combine(a_baseDirectory, entry.Name);
                        entry.ExtractToFile(newPath, true);
                    }
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Checks validity of File Name. Replaces invalid characters with '_' and truncates string to a length
    /// of less than 255 characters if needed
    /// </summary>
    /// <param name="a_fileName"></param>
    /// <returns></returns>
    public static string GetFriendlyFileName(string a_fileName)
    {
        if (IsValidFileName(a_fileName))
        {
            return a_fileName;
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            a_fileName = a_fileName.Replace(invalidChar, '_');
        }

        if (a_fileName.Length >= 255)
        {
            a_fileName = a_fileName.Substring(0, 254);
        }

        return a_fileName;
    }
}