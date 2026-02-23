using System.Windows.Forms;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Localization;
using PT.SystemServiceProxy.APIClients;

namespace PT.UI.Managers;

internal class ClientUpdaterFileRetriever
{
    private readonly string m_systemServiceUrl;
    private readonly string m_instanceName;
    private readonly string m_softwareVersion;
    private const string c_latestUpdateFileName = "LatestUpdateFileList.Old";

    internal ClientUpdaterFileRetriever(string a_instanceName, string a_softwareVersion, string a_systemServiceUrl)
    {
        m_instanceName = a_instanceName;
        m_softwareVersion = a_softwareVersion;
        m_systemServiceUrl = a_systemServiceUrl;
        m_instanceName = a_instanceName;
        m_softwareVersion = a_softwareVersion;
    }

    internal void RetrieveFiles(string a_workingFolder)
    {
        try
        {
            // read latest update files
            Dictionary<string, long> lastUpdatedFiles = new ();
            if (!Directory.Exists(a_workingFolder))
            {
                Directory.CreateDirectory(a_workingFolder);
            }

            string latestUpdateFilePath = Path.Combine(a_workingFolder, c_latestUpdateFileName);

            if (File.Exists(latestUpdateFilePath))
            {
                using (StreamReader sr = new (latestUpdateFilePath))
                {
                    string relPath;
                    while ((relPath = sr.ReadLine()) != null)
                    {
                        string fullPath = Path.Combine(a_workingFolder, relPath);
                        if (File.Exists(fullPath))
                        {
                            lastUpdatedFiles.Add(relPath, File.GetLastWriteTimeUtc(fullPath).Ticks);
                        }
                    }
                }
            }

            try
            {
                // get the latest update files
                SystemActionsClient clientSideSystemClient = new (m_instanceName, m_softwareVersion, m_systemServiceUrl);
                //systemClient.DownloadFiles(a_workingFolder, lastUpdatedFiles);
            }
            catch (Exception e)
            {
                string exceptionText;
                if (e.InnerException == null)
                {
                    exceptionText = string.Format("Exception Text={0}", e.Message);
                }
                else
                {
                    exceptionText = string.Format("Exception Text={0};\n\nInner ExceptionText={1}", e.Message, e.InnerException.Message);
                }

                if (exceptionText.Contains("proxy"))
                {
                    exceptionText = exceptionText + Localizer.GetString("\nAccording to the error that was returned, there's a high probability that APS communication is being blocked by a proxy server. Please add APS Server Name (or IP) and ports as exceptions to your proxy server. Please see Config Manager for port numbers.");
                }

                if (exceptionText.Contains("Access"))
                {
                    exceptionText = exceptionText + string.Format(Localizer.GetString("\nAccording to the error that was returned there may be a security limitation preventing the Client Updater from writing to the executable folder: {0}.  Please run the Client Updater as Administrator or provide file write/update access to the current user for this folder.  Also make sure that the APS server program files are not marked as Read-Only."), Application.StartupPath);
                }

                throw new PTException("4449", new object[] { "-195" }, false);
            }

            PT.Common.File.TextFile tf = new ();
            foreach (string relPath in lastUpdatedFiles.Keys)
            {
                tf.AppendText(relPath);
            }

            tf.WriteFile(latestUpdateFilePath);
        }
        catch (Exception e)
        {
            throw new PTException("4449", new object[] { "-200" }, false);
        }
    }

    public static void Delete(string a_fileName)
    {
        if (File.Exists(a_fileName))
        {
            File.SetAttributes(a_fileName, FileAttributes.Normal);
            File.Delete(a_fileName);
        }
    }

    //TODO: Packages

    #region Packages
    //const string c_downloaderPassword = "OptimizeThePlanet101#";
    //const string c_downloaderFileURL = @"http://apsportal.com/webinstaller/Packages";
    //const string c_downloaderUpdateSettingsURL = @"Http://apsportal.com/WebInstaller/PackageManagerSettings.xml";

    ///// <summary>
    ///// Downloads required packages for client.
    ///// </summary>
    ///// <param name="a_baseDirectory"></param>
    //public void DownloadPackageFiles(string a_baseDirectory)
    //{
    //    PackagesFolderPath = Path.Combine(a_baseDirectory, "Packages");
    //    string versionDir = Path.GetDirectoryName(a_baseDirectory);
    //    Version softwareVersion = new Version(versionDir.Substring(versionDir.LastIndexOf("\\") + 1));
    //    string packageSoftwareVersion = softwareVersion.Major.ToString() + "." + softwareVersion.Minor.ToString();
    //    try
    //    {
    //        WebClient client = new WebClient
    //        {
    //            Credentials = new NetworkCredential(c_downloaderUserName, c_downloaderPassword)
    //        };

    //        //check for local packages setup
    //        List<Tuple<string, string>> packagesDownloadList;

    //        if (!Directory.Exists(PackagesFolderPath))
    //        {
    //            //create new packages directory and create full packages download list
    //            System.IO.Directory.CreateDirectory(PackagesFolderPath);
    //            GetPackagesList(client, null, out packagesDownloadList);
    //        }
    //        else
    //        {
    //            //create only local package update list
    //            Dictionary<string, string> localPackages;
    //            string[] localPackagesPaths = Directory.GetDirectories(PackagesFolderPath, "*Package*", SearchOption.TopDirectoryOnly);
    //            List<string> localPackagesList = new List<string>(localPackagesPaths);
    //            string[] legacyPackage = Directory.GetDirectories(PackagesFolderPath, "*Legacy*", SearchOption.TopDirectoryOnly);
    //            if (legacyPackage.Length == 1) localPackagesList.Add(legacyPackage[0]);

    //            if (localPackagesList.Count > 0)
    //            {
    //                localPackages = new Dictionary<string, string>();
    //                foreach (string localPackage in localPackagesList)
    //                {
    //                    DirectoryInfo package = new DirectoryInfo(localPackage);
    //                    DirectoryInfo[] packageVersions = package.GetDirectories();
    //                    foreach (DirectoryInfo version in packageVersions)
    //                    {
    //                        string packageName = package.Name;
    //                        string packageVersion = version.Name;
    //                        localPackages.Add(packageName, packageVersion);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                localPackages = null;
    //            }
    //            GetPackagesList(client, localPackages, out packagesDownloadList);
    //        }

    //        if (packagesDownloadList.Count > 0)
    //        {
    //            foreach (Tuple<string, string> package in packagesDownloadList)
    //            {
    //                string packageName = package.Item1.ToString();
    //                string packageVersion = package.Item2.ToString();
    //                string downloadPath = Path.Combine(c_downloaderFileURL, packageSoftwareVersion, packageName, packageVersion, packageName).Replace("\\", "/") + ".zip";
    //                try
    //                {
    //                    byte[] fileBytes = client.DownloadData(downloadPath);
    //                    ExtractPackageFilesToRootDirectory(fileBytes, packageName, packageVersion);
    //                }
    //                catch (Exception e)
    //                {
    //                    e.Data.Add("PackageDownloadException", $"Error Downloading Packages:{packageName} not downloaded.");
    //                    //TODO:
    //                    //MainForm.LogException(e);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Exception e = new Exception();
    //            e.Data.Add("PackageDownloadException", "No packages needing download.");
    //            //TODO:
    //            //MainForm.LogException(e);
    //        }

    //    }
    //    catch (Exception e)
    //    {
    //        //TODO:
    //        //MainForm.LogException(e);
    //    }
    //}

    //public string PackagesFolderPath { get; set; }

    //public void GetPackagesList(WebClient a_client, Dictionary<string, string> a_localPackage, out List<Tuple<string, string>> packagesList)
    //{
    //    packagesList = new List<Tuple<string, string>>();

    //    byte[] updateBytes = a_client.DownloadData(c_downloaderUpdateSettingsURL);

    //    System.IO.MemoryStream temp = new System.IO.MemoryStream(updateBytes);
    //    System.IO.StreamReader file = new System.IO.StreamReader(temp);

    //    //Read XML
    //    XPathDocument xmlDoc = new XPathDocument(file);
    //    XPathNavigator nav = xmlDoc.CreateNavigator();
    //    XPathNodeIterator nodes = nav.Select("/PackageManager/packages/package");
    //    foreach (XPathNavigator package in nodes)
    //    {
    //        package.MoveToChild("name", "");
    //        string packageName = package.InnerXml;
    //        package.MoveToNext();
    //        string currentVersion = package.InnerXml;

    //        //populate packages list 
    //        if (a_localPackage == null)
    //        {
    //            //get all packages
    //            packagesList.Add(new Tuple<string, string>(packageName, currentVersion));
    //        }
    //        else
    //        {
    //            //get only updates for local packages
    //            if (a_localPackage.TryGetValue(packageName, out string localVersion))
    //            {
    //                if (currentVersion != localVersion)
    //                {
    //                    packagesList.Add(new Tuple<string, string>(packageName, currentVersion));
    //                }
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// Extracts specific files in all sub directories of the zip into the directory specified
    ///// </summary>
    ///// <param name="a_fileBytes"></param>
    ///// <param name="a_destDir"></param>
    //public void ExtractPackageFilesToRootDirectory(byte[] a_fileBytes, string a_packageName, string a_packageVersion)
    //{
    //    string packageFolderPath = Path.Combine(PackagesFolderPath, a_packageName);
    //    string packageVersionPath = Path.Combine(packageFolderPath, a_packageVersion);
    //    string packageDirPath = Path.Combine(packageVersionPath, a_packageName);

    //    //clear previous package version path
    //    if (Directory.Exists(packageVersionPath)) PT.Common.Directory.DirectoryUtils.Delete(packageVersionPath);

    //    //write current package files
    //    FileUtils.ExtractPackagesToRootDirectory(a_fileBytes, packageDirPath);
    //}
    #endregion
}