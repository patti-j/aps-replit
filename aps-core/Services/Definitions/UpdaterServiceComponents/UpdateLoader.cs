using System.Collections.Generic;
using PT.UpdaterServiceComponents;
using System;
using System.IO;
using System.Text;

namespace PT.UpdaterServiceComponents
{
    /// <summary>
    /// Summary description for UpdateLoader.
    /// </summary>
    /// 
    public class UpdateLoader
    {
        const string c_clientUpdaterChannelName = "APSClientUpdater";
        private readonly string m_clientUpdaterURL;

        public UpdateLoader(string a_clientUpdaterURL)
        {
            m_clientUpdaterURL = a_clientUpdaterURL;
        }

        /// <summary>
        /// Save user specific settings to the client updater services
        /// </summary>
        /// <param name="a_userId"></param>
        /// <param name="a_zippedBytes"></param>
        public void UploadUserFiles(string a_userId, byte[] a_zippedBytes)
        {
            PT.Common.RemotingUtils.RegisterHttpClientChannel(c_clientUpdaterChannelName);
            UpdateFileManager ufm = (UpdateFileManager)Activator.GetObject(typeof(UpdateFileManager), m_clientUpdaterURL);
            ufm.UpdateUserFiles(a_userId, a_zippedBytes);
        }

        /// <summary>
        /// a_lastUpdatedFiles need to be updated since it will constitue "the last updated files" for next time.
        /// </summary>
        /// <param name="a_baseDirectory">startup path for Client</param>
        /// <param name="a_lastUpdatedFiles">relative path and last modified date for each of the files client received the last time it contacted ClientUpdater Svc</param>
        public void DownloadFiles(string a_baseDirectory, ref Dictionary<string, long> a_lastUpdatedFiles)
        {
            try
            {
                if (!Directory.Exists(a_baseDirectory))
                {
                    Directory.CreateDirectory(a_baseDirectory);
                }

                PT.Common.RemotingUtils.RegisterHttpClientChannel(c_clientUpdaterChannelName);
                UpdateFileManager ufm = (UpdateFileManager)Activator.GetObject(typeof(UpdateFileManager), m_clientUpdaterURL);
                List<Tuple<string, long, byte[]>> updateFiles = ufm.GetFiles(a_lastUpdatedFiles);

                List<string> filesWithErrs = new List<string>();
                Exception exception = null;
                List<string> deletedFiles = new List<string>();

                foreach (Tuple<string, long, byte[]> pathDateContent in updateFiles)
                {
                    try
                    {
                        string fullPath = Path.Combine(a_baseDirectory, pathDateContent.Item1);

                        if (pathDateContent.Item3 != null) // file is still on the server
                        {
                            if (File.Exists(fullPath)) // file exists here, delete and recreate it
                            {
                                File.Delete(fullPath);
                            }
                            string directory = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            byte[] decompBytes = PT.Common.Compression.Optimal.Decompress(pathDateContent.Item3);
                            PT.Common.File.FileUtils.SaveBinaryFile(fullPath, decompBytes);

                            try
                            {
                                FileAttributes fileAttrs = File.GetAttributes(fullPath);
                                if ((fileAttrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                {
                                    fileAttrs = PT.Common.File.FileUtils.RemoveAttribute(fileAttrs, FileAttributes.ReadOnly);
                                    File.SetAttributes(fullPath, fileAttrs);
                                } 
                                File.SetLastWriteTimeUtc(fullPath, new DateTime(pathDateContent.Item2));
                            }
                            catch (Exception e)
                            {
                                throw new PT.APSCommon.PTException("2584", e, new object[] { fullPath, e.Message });
                            }

                            a_lastUpdatedFiles[pathDateContent.Item1] = pathDateContent.Item2;
                        }
                        else // file has been removed from server
                        {
                            if (File.Exists(fullPath)) // if exists delete it
                            {
                                File.Delete(fullPath);
                                deletedFiles.Add(fullPath);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        if (exception == null)
                        {
                            exception = err;
                        }
                        filesWithErrs.Add(pathDateContent.Item1);
                    }
                }

                foreach(string path in deletedFiles) // delete directories
                {
                    DeleteEmptyFolders(a_baseDirectory, Path.GetDirectoryName(path));
                }

                if (exception != null)
                {
                    StringBuilder errStr = new StringBuilder();
                    errStr.AppendLine(string.Format("Error saving files received from Client Updater in base directory '{0}' and with relative paths:", a_baseDirectory));
                    foreach(string relPath in filesWithErrs)
                    {
                        errStr.AppendLine(relPath);
                    }
                    throw new PT.APSCommon.PTException(errStr.ToString(), exception);
                }
            }
            catch (Exception e)
            {
                string alertsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Alerts");
                if (!Directory.Exists(alertsDirectory))
                {
                    Directory.CreateDirectory(alertsDirectory);
                }
                string fullLogPath = Path.Combine(alertsDirectory, "APS Client Updater.log");

                PT.Common.File.SimpleExceptionLogger.LogException(fullLogPath, e, "");
                throw;
            }
        }

        void DeleteEmptyFolders(string a_baseDirectory, string a_dir)
        {
            try
            {
                // if not empty, is a directory, not base directory and empty
                if (!string.IsNullOrEmpty(a_dir) && Directory.Exists(a_dir) && !string.Equals(a_dir, a_baseDirectory, StringComparison.OrdinalIgnoreCase) && Directory.GetFiles(a_dir).Length == 0)
                {
                    Directory.Delete(a_dir);
                    DeleteEmptyFolders(a_baseDirectory, Path.GetDirectoryName(a_dir)); // delete parent directory
                }
            }
            catch (Exception err)
            {
                string alertsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Alerts");
                if (!Directory.Exists(alertsDirectory))
                {
                    Directory.CreateDirectory(alertsDirectory);
                }
                string fullLogPath = Path.Combine(alertsDirectory, "APS Client Updater.log");

                PT.Common.File.SimpleExceptionLogger.LogException(fullLogPath, err, "");
            }
        }
    }
}
