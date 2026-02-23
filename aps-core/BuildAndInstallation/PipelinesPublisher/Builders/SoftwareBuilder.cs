using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

using PipelinesPublisher.Definitions;

using PT.Common.Directory;
using PT.Common.Exceptions;

namespace PipelinesPublisher.Builder;

internal class SoftwareBuilder : IBuilder
{
    public BuildPaths Paths { get; }
    public string BaseFolder { get; }
    public string Version { get; }
    public string CertPassword { get; }

    private readonly List<PackagePath> m_packagePaths = new ();
    private readonly List<PackagePath> m_extensionPaths = new ();
    private readonly List<PackagePath> m_samplesPaths = new ();

    public SoftwareBuilder(string[] a_args)
    {
        Console.WriteLine($"Loading {a_args.Length} parameters. Expecting 3");

        try
        {
            BaseFolder = a_args[1];
            Console.WriteLine($"Using base folder: {BaseFolder}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load args[1] for base folder");
            Console.WriteLine(e);
            throw;
        }

        try
        {
            CertPassword = a_args[2];
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load args[2] for cert password");
            Console.WriteLine(e);
            throw;
        }

        Version = AssemblyVersionChecker.GetAssemblyVersion().ToString();
        Paths = new BuildPaths(BaseFolder);
    }

    public void Run()
    {
        // PT Packages are zipped up with the rest of the Software in the Software folder
        // while the Extension Packages are placed into a folder called CustomerPackages 
        // that exists at the same level as the Software folder. 
        LoadPackagePaths();
        LoadExtensionPaths();
        LoadSamplePaths();
        CopyProgramFilesToDeploymentFolder();
        SignExecutables();
        // CopyProgramFilesToDeploymentFolder needs to happen before SignExecutables because
        // SignExecutables uses the deployment folder path for ProgramFiles. SignExecutables
        // uses the source path for the various DLLs and exe so the other ones need to happen
        // after to ensure that the proper files are signed.
        // TODO: Refactor the situation described above
        CopyPackagesToDeploymentFolder();
        ZipClientAndMoveToDeploymentFolder();
        ZipSoftwareRelease();
        //ZipIntegrationFilesToPublishFolder();
        CopyExtensionsToPublishFolder();
    }

    /// <summary>
    /// Moves files from the Deployment Folder to the Publish folder
    /// </summary>
    public void ZipSoftwareRelease()
    {
        if (!Directory.Exists(Paths.GetSoftwarePublishDirectory()))
        {
            Directory.CreateDirectory(Paths.GetSoftwarePublishDirectory());
        }

        List<string> dirsAndFilesToZip = Directory.GetDirectories(Paths.GetDeploymentVersionFolder()).ToList();
        // Grabs the Packages, ProgramFiles, and UpdateFiles folder/directories
        dirsAndFilesToZip.AddRange(Directory.GetFiles(Paths.GetDeploymentVersionFolder()));
        // Grabs client zip
        PT.Common.Compression.Zip.Create(dirsAndFilesToZip, Paths.GetSoftwarePublishZipName());
        // Creates the zip for the publish folder, which is what the Azure pipeline actually pushes to its artifacts
    }

    private void ZipClientAndMoveToDeploymentFolder()
    {
        List<string> dirsAndFilesToZip = Directory.GetDirectories(Paths.GetSourceFolderForClient()).ToList();
        dirsAndFilesToZip.AddRange(Directory.GetFiles(Paths.GetSourceFolderForClient()));
        PT.Common.Compression.Zip.Create(dirsAndFilesToZip, Path.Combine(Paths.GetDeploymentVersionFolder(), "Client.zip"));
    }

    public void SignExecutables()
    {
        Process signTool = Signing.GetSignToolProcess();

        List<string> filesToSign = new ();
        
        //Sign System
        string path = Paths.GetDeploymentProgramFilesFolderForSystem();
        string[] targetFiles = Directory.GetFiles(path, "PlanetTogether System.exe", SearchOption.TopDirectoryOnly);
        string targetFile = targetFiles[0].Quotation();
        Signing.SignTool(signTool, targetFile, CertPassword);

        //System DLLs
        filesToSign.Clear();
        targetFiles = Directory.GetFiles(path, "PlanetTogether System.dll", SearchOption.TopDirectoryOnly);
        filesToSign.Add(targetFiles[0].Quotation());
        targetFiles = Directory.GetFiles(path, "PT*.dll", SearchOption.TopDirectoryOnly);
        foreach (string systemFile in targetFiles)
        {
            filesToSign.Add(systemFile);
        }
        Signing.SignTool(signTool, filesToSign, CertPassword, "System Files");

        //3rd Party Library DLLs
        filesToSign.Clear();
        List<string> serverDllList = new ()
        {
            "LazyCache.AspNetCore.dll",
            "LazyCache.dll",
            "LZ4.dll",
            "LZ4pn.dll",
            "Quartz.dll",
            "Sentry.dll",
            "Dapper.dll",
        };

        foreach (string serverDllName in serverDllList)
        {
            targetFiles = Directory.GetFiles(path, serverDllName, SearchOption.TopDirectoryOnly);
            if (targetFiles.Length == 1)
            {
                filesToSign.Add(targetFiles[0]);
            }
            else
            {
                throw new PTException($"Failed to sign server DLL {path}\\{serverDllName}. It was not found");
            }
        }
        Signing.SignTool(signTool, filesToSign, CertPassword, "Server Library Files");

        //Sign Packages
        filesToSign.Clear();
        foreach (PackagePath packagePath in m_packagePaths)
        {
            targetFile = "\"" + Path.Combine(packagePath.PackageSourcePath, packagePath.PackageDll) + "\"";
            filesToSign.Add(targetFile);
        }
        Signing.SignTool(signTool, filesToSign, CertPassword, "Packages");
        
        //Sign Extensions
        filesToSign.Clear();
        foreach (PackagePath packagePath in m_extensionPaths)
        {
            targetFile = "\"" + Path.Combine(packagePath.PackageSourcePath, packagePath.PackageDll) + "\"";
            filesToSign.Add(targetFile);
        }

        //Sign Samples
        foreach (PackagePath packagePath in m_samplesPaths)
        {
            targetFile = "\"" + Path.Combine(packagePath.PackageSourcePath, packagePath.PackageDll) + "\"";
            filesToSign.Add(targetFile);
        }
        Signing.SignTool(signTool, filesToSign, CertPassword, "Extensions and Samples");

        //Sign Client
        filesToSign.Clear();
        path = Paths.GetSourceFolderForClient();
        targetFiles = Directory.GetFiles(path, "PlanetTogetherClient.exe", SearchOption.TopDirectoryOnly);
        targetFile = "\"" + targetFiles[0] + "\"";
        Signing.SignTool(signTool, targetFile, CertPassword);

        //Client DLLs
        filesToSign.Clear();
        targetFiles = Directory.GetFiles(path, "PlanetTogetherClient.dll", SearchOption.TopDirectoryOnly);
        filesToSign.Add(targetFiles[0]);

        targetFiles = Directory.GetFiles(path, "PT*.dll", SearchOption.TopDirectoryOnly);
        foreach (string systemFile in targetFiles)
        {
            filesToSign.Add(systemFile);
        }
        Signing.SignTool(signTool, filesToSign, CertPassword, "Client Files");

        //3rd Party Library DLLs
        filesToSign.Clear();
        List<string> clientDllList = new()
        {
            "LazyCache.AspNetCore.dll",
            "LazyCache.dll",
            "LZ4.dll",
            "LZ4pn.dll",
            "PlexityHide.GTP.dll",
            "Quartz.dll",
            "Sentry.dll",
            "Dapper.dll",
        };

        foreach (string clientDllName in clientDllList)
        {
            targetFiles = Directory.GetFiles(path, clientDllName, SearchOption.TopDirectoryOnly);
            if (targetFiles.Length == 1)
            {
                filesToSign.Add(targetFiles[0]);
            }
            else
            {
                throw new PTException($"Failed to sign server DLL {path}\\{clientDllName}. It was not found");
            }
        }
        Signing.SignTool(signTool, filesToSign, CertPassword, "Client Files");
    }

    private void CopyPackagesToDeploymentFolder()
    {
        foreach (PackagePath packagePath in m_packagePaths)
        {
            //if (comboBoxEdit_PackagesCategory.Text != "All" && !packagePath.Categories.Contains(comboBoxEdit_PackagesCategory.Text))
            //{
            //    continue;
            //}
            DirectoryUtils.ValidateDirectory(packagePath.PackageDestinationPath);
            File.Copy(Path.Combine(packagePath.PackageSourcePath, packagePath.PackageDll), Path.Combine(packagePath.PackageDestinationPath, packagePath.PackageDll), true);
        }
    }

    //private void ZipIntegrationFilesToPublishFolder()
    //{
    //    Console.WriteLine("Zipping integration files");
    //    if (!Directory.Exists(Paths.GetPTIntegrationPublishFolder()))
    //    {
    //        Directory.CreateDirectory(Paths.GetPTIntegrationPublishFolder());
    //    }

    //    //Zip client
    //    List<string> dirsAndFilesToZip = Directory.GetDirectories(Paths.GetSourceFolderForIntegrationFiles()).ToList();
    //    dirsAndFilesToZip.AddRange(Directory.GetFiles(Paths.GetSourceFolderForIntegrationFiles()));
    //    PT.Common.Compression.Zip.Create(dirsAndFilesToZip, Path.Combine(Paths.GetPTIntegrationPublishFolder(), "PlanetTogether.zip"));
    //}

    private void CopyProgramFilesToDeploymentFolder()
    {
        Console.WriteLine("Copying program files");

        //System
        FileUtil.CopyAllFiles(Paths.GetSourceFolderForSystem(), Paths.GetDeploymentProgramFilesFolderForSystem());
        if (!Directory.Exists(Paths.GetDeploymentVersionUpdateFilesFolder()))
        {
            Directory.CreateDirectory(Paths.GetDeploymentVersionUpdateFilesFolder());
        }

        //Update files
        FileUtil.CopyAllFiles(Paths.GetSourceFolderForUpdateFiles(), Paths.GetDeploymentVersionUpdateFilesFolder());
    }

    #region Packages
    private void LoadPackagePaths()
    {
        Console.WriteLine("Loading package paths");
        string[] packageRoots = Directory.GetDirectories(Paths.GetSourceFolderForPackages());
        foreach (string packageDirectory in packageRoots)
        {
            //Make sure the package has the necessary config file
            string ptConfigPath = Path.Combine(packageDirectory, "PTApp.config");
            if (!File.Exists(ptConfigPath))
            {
                Console.WriteLine($"Loading Package Paths:  There was no PTApp.config in {packageDirectory}");
                continue;
            }

            //Find package dll
            //We assume there is only 1 package per directory
            string[] dllFiles = Directory.GetFiles(packageDirectory, "*Package.dll", SearchOption.AllDirectories);
            if (dllFiles.Length == 0)
            {
                Console.WriteLine($"Loading Package Paths: There was no package dll found in {packageDirectory}");
                continue;
            }

            //Should we include in release? If not, continue
            try
            {
                XDocument xDocument = XDocument.Load(ptConfigPath);
                string value = xDocument?.Root?.Descendants()?.Where(x => x.Name == "IncludeInRelease").FirstOrDefault()?.Value;
                if (value != "true")
                {
                    //Not included
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Loading Package Paths: {e.Message}");
                continue;
            }

            string directoryName = Path.GetFileName(packageDirectory);

            //Parse out assemblies to add from PTApp.config
            string xmlFile = Path.Combine(packageDirectory, "PTApp.config");
            List<string> categories = GetCategories(xmlFile);

            List<string> filesToPack = FileUtil.GetFilesPack(xmlFile, true);

            //Use the assembly version to nest the package correctly
            Version version = AssemblyVersionChecker.GetAssemblyVersion().ToSimpleVersion();
            string destinationPath = Path.Combine(Paths.GetDeploymentVersionFolder(), "Packages", directoryName, $"{version.Minor}.{version.Revision}");

            PackagePath packagePath = new (Path.GetFileName(dllFiles[0]), packageDirectory, Path.Combine(packageDirectory, "bin", "Release"), destinationPath, filesToPack, categories);
            m_packagePaths.Add(packagePath);
        }
    }

    public static List<string> GetCategories(string a_xmlFilePath)
    {
        if (!File.Exists(a_xmlFilePath))
        {
            throw new Exception("BuildConfig file not found: " + a_xmlFilePath);
        }

        List<string> categories = new ();

        XPathDocument xd = new (a_xmlFilePath);
        XPathNavigator nav = xd.CreateNavigator();
        XPathNodeIterator ni;

        ni = nav.SelectDescendants("CategoryPackage", "", false);
        if (ni.MoveNext())
        {
            string assemblyList = ni.Current.Value;
            string[] assemblies = assemblyList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string assembly in assemblies)
            {
                string trimmedName = assembly.Trim();
                if (!string.IsNullOrEmpty(trimmedName))
                {
                    categories.Add(assembly.Trim());
                }
            }
        }

        return categories;
    }
    #endregion

    private void LoadExtensionPaths()
    {
        Console.WriteLine("Loading extension paths");
        string[] extensionRoots = Directory.GetDirectories(Paths.GetSourceFolderForExtensions());
        foreach (string extensionDirectory in extensionRoots)
        {
            //Find package dll
            string[] dllFiles = Directory.GetFiles(extensionDirectory, "*Package.dll", SearchOption.AllDirectories);
            if (dllFiles.Length == 0)
            {
                Console.WriteLine($"Loading Extension Paths: There was no extension dll found in {extensionDirectory}");
                continue;
            }

            string destinationDirectoryName = Path.GetFileName(extensionDirectory); // I feel like this might return an empty string based on documentation for GetFileName
            foreach (string dllFile in dllFiles)
            {
                string dllFileName = Path.GetFileName(dllFile);
                string packageDirectory = Path.GetDirectoryName(dllFile) ?? string.Empty;

                //Use the assembly version to nest the package correctly
                Version version = AssemblyVersionChecker.GetAssemblyVersion().ToSimpleVersion();
                string destinationPath = Path.Combine(Paths.GetExtensionPublishDirectory(), destinationDirectoryName, $"{version.Minor}.{version.Revision}");

                PackagePath packagePath = new(dllFileName, extensionDirectory, packageDirectory, destinationPath, new List<string>(), new List<string>());
                m_extensionPaths.Add(packagePath);
            }
        }
    }

    private void LoadSamplePaths()
    {
        Console.WriteLine("Loading sample package paths");
        string[] sampleRoots = Directory.GetDirectories(Paths.GetSourceFolderForSamples());
        foreach (string sampleDirectory in sampleRoots)
        {
            //Find package dll
            string[] dllFiles = Directory.GetFiles(sampleDirectory, "*Package.dll", SearchOption.AllDirectories);
            if (dllFiles.Length == 0)
            {
                Console.WriteLine($"Loading Sample Paths: There was no extension dll found in {sampleDirectory}");
                continue;
            }

            string destinationDirectoryName = Path.GetFileName(sampleDirectory); // I feel like this might return an empty string based on documentation for GetFileName
            foreach (string dllFile in dllFiles)
            {
                string dllFileName = Path.GetFileName(dllFile);
                string packageDirectory = Path.GetDirectoryName(dllFile) ?? string.Empty;

                //Use the assembly version to nest the package correctly
                Version version = AssemblyVersionChecker.GetAssemblyVersion().ToSimpleVersion();
                string destinationPath = Path.Combine(Paths.GetSamplePublishDirectory(), destinationDirectoryName, $"{version.Minor}.{version.Revision}");

                PackagePath packagePath = new(dllFileName, sampleDirectory, packageDirectory, destinationPath, new List<string>(), new List<string>());
                m_samplesPaths.Add(packagePath);
            }
        }
    }

    private void CopyExtensionsToPublishFolder()
    {
        //extensions are just Packages in V12
        foreach (PackagePath extensionPackagePath in m_extensionPaths)
        {
            DirectoryUtils.ValidateDirectory(extensionPackagePath.PackageDestinationPath);
            File.Copy(Path.Combine(extensionPackagePath.PackageSourcePath, extensionPackagePath.PackageDll), Path.Combine(extensionPackagePath.PackageDestinationPath, extensionPackagePath.PackageDll), true);
        } 
        
        foreach (PackagePath samplePackagePath in m_samplesPaths)
        {
            DirectoryUtils.ValidateDirectory(samplePackagePath.PackageDestinationPath);
            File.Copy(Path.Combine(samplePackagePath.PackageSourcePath, samplePackagePath.PackageDll), Path.Combine(samplePackagePath.PackageDestinationPath, samplePackagePath.PackageDll), true);
        }
    }
}