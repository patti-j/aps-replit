using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace PT.ServerManagerSharedLib.Utils
{
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
            return !string.IsNullOrWhiteSpace(a_fileName) && a_fileName != "" && a_fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1 && a_fileName.Length < 255;
        }

        // <summary>
        /// Copies files from the source folder to the destination folder.  If folders don't exist they're created.
        /// </summary>
        /// <param name="a_sourceFolder"></param>
        /// <param name="a_destinationFolder"></param>
        public static void CopyAllFiles(string a_sourceFolder, string a_destinationFolder)
        {
            RoboCopyResult result = RoboCopy($"\"{a_sourceFolder}\"", $"\"{a_destinationFolder}\"");
            Console.WriteLine(result.Log);
            if (result.ExitCode >= 8)
            {
                //File copy error
                Console.WriteLine("Error on file copy from {0}  to  {1}", a_sourceFolder, a_destinationFolder);
            }
        }

        private class RoboCopyResult
        {
            public int ExitCode;
            public string Log;
        }

        private static RoboCopyResult RoboCopy(string a_sourceFolder, string a_destinationFolder)
        {
            //Use Robocopy
            RoboCopyResult result = new RoboCopyResult();
            string procArgs = a_sourceFolder + " " + a_destinationFolder;
            procArgs += " /mir /copy:DT /R:5 /W:2 /np /ns /njh /njs";
            Process robocopy = new Process();
            robocopy.StartInfo.Arguments = procArgs;
            robocopy.StartInfo.FileName = "robocopy";
            robocopy.StartInfo.UseShellExecute = false;
            robocopy.StartInfo.RedirectStandardOutput = true;
            robocopy.StartInfo.CreateNoWindow = true;
            robocopy.Start();

            result.Log = robocopy.StandardOutput.ReadToEnd();
            robocopy.WaitForExit();
            result.ExitCode = robocopy.ExitCode;
            return result;
        }
        /// <summary>
        /// Save a stream to a temp file with an .htm extension and launches it to open it in a browser.
        /// </summary>
        /// <param name="a_inStream"></param>
        public static void OpenStreamDataInBrowser(Stream a_inStream)
        {
            // Create a temporary file to save to
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache),
                Guid.NewGuid() + ".htm");

            using (var writer = System.IO.File.CreateText(filePath))
            {
                var reader = new StreamReader(a_inStream);
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
        public static string AppConfigFileName
        {
            get { return (Environment.GetCommandLineArgs())[0] + ".config"; }
        }

        /// <summary>
        /// Return the full path of the currently executing application.
        /// </summary>
        public static string AppFilePath
        {
            get { return Environment.GetCommandLineArgs()[0]; }
        }

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
        public static void Delete(String[] a_files)
        {
            foreach (string element in a_files)
            {
                System.IO.File.SetAttributes(element, FileAttributes.Normal);
                System.IO.File.Delete(element);
            }
        }

        /// <summary>
        /// Delete a file irregardless of its read-only setting. The files attributes are set to Normal before the delete is attempted (readonly is cleared).
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
                int newFA = (int)fa & (~(int)FileAttributes.ReadOnly);
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
            if (!a_fromDir.Exists) return;

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
                    DirectoryInfo newSubDirInfo = new DirectoryInfo(Path.Combine(a_toDir.FullName, dirInfo.Name));
                    CopyDirectory(dirInfo, newSubDirInfo, a_dirsToSkip, a_overwrite);
                }
            }
        }

        public static FileAttributes RemoveAttribute(FileAttributes a_attrs, FileAttributes a_attrsToRemove)
        {
            return a_attrs & ~a_attrsToRemove;
        }

        public class TextFile
        {
            private string a_filePath;
            private FileShare read;

            public TextFile(string a_filePath, FileShare read)
            {
                this.a_filePath = a_filePath;
                this.read = read;

                StringBuilder builder = new StringBuilder();

                foreach (string line in File.ReadLines(a_filePath))
                {
                    builder.AppendLine(line);
                }

                Text = builder.ToString();
            }

            public string Text { get; set; }
        }

        public static void Extract(byte[] a_archiveBytes, string a_destDir, bool a_overwriteFiles)
        {
            if (!System.IO.Directory.Exists(a_destDir))
            {
                System.IO.Directory.CreateDirectory(a_destDir);
            }

            List<Exception> exceptions = new List<Exception>();

            using (MemoryStream stream = new MemoryStream(a_archiveBytes))
            {
                using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    IEnumerable<ZipArchiveEntry> sortedEntries = zip.Entries.OrderBy<ZipArchiveEntry, int>(entry => entry.FullName.Length);

                    foreach (ZipArchiveEntry entry in sortedEntries)
                    {
                        string fullPath = Path.Combine(a_destDir, entry.FullName);
                        try
                        {
                            if (fullPath.Replace("/", "\\").EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                            {
                                //This is a directory, create it if it doesn't exist.
                                string dir = Path.GetDirectoryName(fullPath);
                                if (!System.IO.Directory.Exists(dir))
                                {
                                    System.IO.Directory.CreateDirectory(dir);
                                }
                            }
                            else
                            {
                                entry.ExtractToFile(fullPath, a_overwriteFiles);
                            }

                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                        }
                    }
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}