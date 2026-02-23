using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

using PT.APSCommon;
using PT.Common.Attributes.Assembly;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.Transmissions;

namespace PT.Scheduler.PackageDefs;

public class PackageAssemblyLoader : IPackageAssemblyLoader
{
    #region Properties
    private string m_packageOnDiskPath;
    public string PackagesOnDiskPath => m_packageOnDiskPath;
    private readonly List<AssemblyPackageInfo> m_packageInfos = new ();
    public bool HasPackages => m_packageInfos.Count > 0;
    private readonly string m_packageErrorLogPath;
    public string PackageErrorLogPath => m_packageErrorLogPath;
    public LicenseOptions GetPackageLicenseOptions => PTSystem.LicenseKey.PackageLicenseOptions;
    #endregion

    #region Events
    public event Action<PackageStateT> ValidLicensedPackagesChanged;
    #endregion

    public PackageAssemblyLoader(string a_packageOnDiskPath, string a_packageErrorLogPath)
    {
        m_packageOnDiskPath = a_packageOnDiskPath;
        m_packageErrorLogPath = a_packageErrorLogPath;
    }

    public PackedAssembly GetPackedAssembly(AssemblyPackageInfo a_AssemblyPackageInfo)
    {
        AssemblyPackageInfo assemblyPackageInfo = m_packageInfos.FirstOrDefault(x => x.AssemblyFileName == a_AssemblyPackageInfo.AssemblyFileName && x.Version == a_AssemblyPackageInfo.Version);

        if (assemblyPackageInfo == null)
        {
            //Package not found
            return null;
        }

        try
        {
            string packagePath = PTSystem.WorkingDirectory.PackagesPath;
            if (!Directory.Exists(packagePath))
            {
                return null; // no packages exist.
            }

            //licensing validation
            if (PTSystem.LicenseKey.Packages.Count == 0)
            {
                throw new PTHandleableException("Package license validation failed - No licensed scheduling packages found/loaded.", new object[] { PTSystem.LicenseKey.LicenseId });
            }

            try
            {
                //TODO: Serialize PackedAssembly and return bytes over the SOAP call
                byte[] packageBytes = File.ReadAllBytes(assemblyPackageInfo.PathOnDisk);
                return new PackedAssembly(assemblyPackageInfo, packageBytes);
            }
            catch (Exception a_e)
            {
                LogPackageError($"Assembly.LoadFrom({0}) error in GetPackedAssembly - {a_e.Message}");
            }
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(ServerSessionManager.PackageErrorsLogPath, e, "GetPackedAssembly error.");
        }

        return null;
    }

    public void UpdatePackageStates(PackageStateT a_packageStateT)
    {
        ValidateAndCreatePackageList();
        ValidLicensedPackagesChanged?.Invoke(a_packageStateT);
    }

    public AssemblyPackageInfo[] GetLatestPackageInfos()
    {
        //TODO: Find better way to filter object files out
        
        string[] dllFiles = Directory.GetFiles(m_packageOnDiskPath, "PT.*Package.dll", SearchOption.AllDirectories).Where(fileName => !fileName.Contains("\\obj\\")).ToArray();
        string[] dllFilesThatStartWithPlanetTogether = Directory.GetFiles(m_packageOnDiskPath, "PlanetTogether.*Package.dll", SearchOption.AllDirectories)
                                                                .Where(fileName => !fileName.Contains("\\obj\\"))
                                                                .ToArray();
        if (dllFilesThatStartWithPlanetTogether.Length > 0)
        {
            dllFiles = dllFiles.Concat(dllFilesThatStartWithPlanetTogether).ToArray();
        }

        Dictionary<string, AssemblyPackageInfo> fileVersionDictionary = new ();

        foreach (string file in dllFiles)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(file);

            //DateTime lastWriteTime = System.IO.File.GetLastWriteTime(file);

            AssemblyPackageInfo assemblyPackageInfo = new ()
            {
                AssemblyFileName = Path.GetFileName(file),
                PathOnDisk = file,
                Version = fileVersionInfo.FileVersion
            };
            assemblyPackageInfo.PackageName = assemblyPackageInfo.AssemblyFileName.Replace("PT.", string.Empty).Replace(".dll", string.Empty);

            if (!fileVersionDictionary.TryGetValue(assemblyPackageInfo.AssemblyFileName, out AssemblyPackageInfo matchedAssemblyPackageInfo))
            {
                fileVersionDictionary.Add(assemblyPackageInfo.AssemblyFileName, assemblyPackageInfo);
            }
            //We are trying to load the same package version twice
            //TODO: Throw exception
        }
        #if DEBUG
        //This if statement makes sure we are only copying the files over if they are in the build folders.  No need to copy them over if they are somewhere else
        //We do this so UI can rebuild while system service is running and not run into locked dlls
        //if (m_packageOnDiskPath.Contains("aps-packages"))
        //{
        //    fileVersionDictionary = CopyPackagesToTempDir(fileVersionDictionary);
        //}
        #endif

        return fileVersionDictionary.Select(x => x.Value).ToArray();
    }

    //Copy packages to a temp directory to avoid locking them when UI is rebuilding and system is running externally
    private Dictionary<string, AssemblyPackageInfo> CopyPackagesToTempDir(Dictionary<string, AssemblyPackageInfo> a_fileVersionDictionary)
    {
        m_packageOnDiskPath = Path.Combine(new DirectoryInfo(PTSystem.WorkingDirectory.Scenario).Parent.Parent.Parent.FullName, "Packages");
        PTSystem.WorkingDirectory.PackagesPath = m_packageOnDiskPath;
        if (!Directory.Exists(m_packageOnDiskPath))
        {
            Directory.CreateDirectory(m_packageOnDiskPath);
        }
        else
        {
            DirectoryInfo di = new (m_packageOnDiskPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        foreach (AssemblyPackageInfo assemblyPackageInfo in a_fileVersionDictionary.Values)
        {
            string newPackagePath = Path.Combine(m_packageOnDiskPath, assemblyPackageInfo.AssemblyFileName);
            File.Copy(assemblyPackageInfo.PathOnDisk, newPackagePath);
            assemblyPackageInfo.PathOnDisk = newPackagePath;
        }

        return a_fileVersionDictionary;
    }

    #region Functions for Core
    /// <summary>
    /// Load assemblies in order to read their attributes
    /// </summary>
    private AssemblyLoadContext LoadAssemblies(AssemblyPackageInfo[] a_packagePaths)
    {
        AssemblyLoadContext context = new PackageAssemblyContext("PackageContext", true);

        for (int i = 0; i < a_packagePaths.Length; i++)
        {
            string dllFilePath = a_packagePaths[i].PathOnDisk;
            try
            {
                //Exclusions
                if (!ValidPackagePath(dllFilePath))
                {
                    continue;
                }

                try
                {
                    if (PTSystem.Server)
                    {
                        //TODO: Validate Package Licensing
                        RunCertificateValidation(dllFilePath);
                    }
                }
                catch (PTValidationException e_v)
                {
                    LogPackageError($"Failed validation for package '{Path.GetFileName(dllFilePath)}'. Package will not be loaded - {e_v.Message}");
                    continue;
                }
                catch (Exception)
                {
                    LogPackageError($"Error loading package '{Path.GetFileName(dllFilePath)}' - Missing/Invalid certificate signature.");
                    continue;
                }

                try
                {
                    a_packagePaths[i].Assembly = context.LoadFromAssemblyPath(dllFilePath);
                }
                catch (Exception a_e)
                {
                    LogPackageError($"Assembly.LoadFrom({0}) error in LoadFromAssembliesHelper - {a_e.Message}");
                }
            }
            catch (Exception e) { }
        }

        return context;
    }

    /// <summary>
    /// Compiles and updates list of packages on disk and validates licensing for each. Any unlicensed packages will not be available for use by client.
    /// </summary>
    /// <param name="a_broadcaster"></param>
    /// <param name="a_loadDevPackages"></param>
    public void ValidateAndCreatePackageList()
    {
        AssemblyLoadContext context = null;
        m_packageInfos.Clear();
        try
        {
            if (!Directory.Exists(m_packageOnDiskPath))
            {
                return; // no packages exist.
            }

            AssemblyPackageInfo[] packagePaths = GetLatestPackageInfos();

            context = LoadAssemblies(packagePaths);

            PackageFrameworkVersion clientPackageFrameworkVersion = GetPackageFrameworkVersion(Assembly.GetCallingAssembly());


            foreach (AssemblyPackageInfo assemblyPackageInfo in packagePaths)
            {
                if (assemblyPackageInfo.Assembly == null)
                {
                    continue;
                }

                bool requiresLicensing = GetLicenseRequirement(assemblyPackageInfo.Assembly);
                int packageId = GetPackageId(assemblyPackageInfo.Assembly);
                //TODO: License key package validation
                if (requiresLicensing && !PTSystem.LicenseKey.Packages.Keys.Contains(packageId) && PTSystem.Server) //allows loading of all non-licensing required packages or licensed licensing-required packages
                {
                    LogPackageError($"Unable to load package assembly '{Path.GetFileName(assemblyPackageInfo.Assembly.FullName)}' - Missing/Invalid package license");
                    continue;
                }

                if (packageId == -1)
                {
                    DebugException.ThrowInDebug($"Error Loading Package: {assemblyPackageInfo.Assembly.Location} missing AssemblyInfo PackageId");
                    continue;
                }

                TargetPackageFrameworkVersion packageFrameworkVersion = GetTargetPackageFrameworkVersion(assemblyPackageInfo.Assembly);
                if (!packageFrameworkVersion.GetVersion().Equals(clientPackageFrameworkVersion.GetVersion()))
                {
                    LogPackageError($"Unable to load package assembly '{Path.GetFileName(assemblyPackageInfo.Assembly.FullName)}' - package framework version: {packageFrameworkVersion.GetVersion()} does not match client version: {clientPackageFrameworkVersion?.GetVersion()}");
                    continue;
                }

                AssemblyPackageInfo assemblyInfoToAdd = new ();
                assemblyInfoToAdd.PackageId = packageId;
                assemblyInfoToAdd.AssemblyTitleName = GetAssemblyTitle(assemblyPackageInfo.Assembly);
                assemblyInfoToAdd.AssemblyFileName = Path.GetFileName(assemblyPackageInfo.Assembly.Location);
                assemblyInfoToAdd.PathOnDisk = assemblyPackageInfo.Assembly.Location;

                string schedulingPackageName = GetSchedulingPackageName(assemblyPackageInfo.Assembly);
                assemblyInfoToAdd.IsSchedulingPackage = schedulingPackageName != null;
                assemblyInfoToAdd.SchedulingPackageName = schedulingPackageName;
                assemblyInfoToAdd.RequiresLicensing = requiresLicensing;
                assemblyInfoToAdd.Version = assemblyPackageInfo.Version;
                m_packageInfos.Add(assemblyInfoToAdd);
            }
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(ServerSessionManager.PackageErrorsLogPath, e, "LoadFromAssembliesHelper error.");
        }
        finally
        {
            if (context != null)
            {
                context.Unload();
                context = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
    #endregion

    public void ClearAssemblyInfos()
    {
        m_packageInfos.Clear();
    }

    public void AddAssemblyInfo(AssemblyPackageInfo a_packedAssemblyPackageInfo)
    {
        m_packageInfos.Add(a_packedAssemblyPackageInfo);
    }

    //Used when running internal
    public void UpdatePackageOnDiskPath(string a_newPath)
    {
        m_packageOnDiskPath = a_newPath;
    }

    #region Package Utility
    protected int GetPackageId(Assembly a_assembly)
    {
        object[] customAttributes = a_assembly.GetCustomAttributes(typeof(AssemblyPackageId), true);
        if (customAttributes == null || customAttributes.Length == 0 || !(customAttributes[0] is AssemblyPackageId packageIdAttr))
        {
            return -1;
        }

        return packageIdAttr.GetPackageId();
    }

    protected static bool GetLicenseRequirement(Assembly a_assembly)
    {
        object[] customAttributes = a_assembly.GetCustomAttributes(typeof(RequiresLicense), true);
        return customAttributes.Length != 0 && customAttributes[0] is RequiresLicense licenseExemptAttribute && licenseExemptAttribute.GetRequiresLicense();
    }

    protected static TargetPackageFrameworkVersion GetTargetPackageFrameworkVersion(Assembly a_assembly)
    {
        TargetPackageFrameworkVersion version = a_assembly.GetCustomAttribute<TargetPackageFrameworkVersion>();

        return version;
    }

    protected static PackageFrameworkVersion GetPackageFrameworkVersion(Assembly a_assembly)
    {
        PackageFrameworkVersion version = a_assembly.GetCustomAttribute<PackageFrameworkVersion>();

        return version;
    }

    protected string GetAssemblyTitle(Assembly a_assembly)
    {
        object[] customAttributes = a_assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
        if (customAttributes.Length != 0 && customAttributes[0] is AssemblyTitleAttribute packageTitleAttribute)
        {
            return packageTitleAttribute.Title;
        }

        DebugException.ThrowInDebug($"{a_assembly.Modules.First().Name} is missing 'AssemblyTitle' attribute containing package name in AssemblyInfo'", new Exception("Package Title Missing"));

        return "";
    }

    private static string GetSchedulingPackageName(Assembly a_assembly)
    {
        object[] customAttributes = a_assembly.GetCustomAttributes(typeof(AssemblySchedulingPackage), true);
        if (customAttributes.Length == 0 || !(customAttributes[0] is AssemblySchedulingPackage schedPackAttr))
        {
            return null;
        }

        return schedPackAttr.GetSchedulingPackageName();
    }
    #endregion

    #region Validation
    private readonly List<string> ExcludedPackages = new ()
    {
        "pt.packagedefinitions.dll",
        "pt.packagedefinitionsui.dll",
        "pt.packageresources.dll",
        "obj",
        #if DEBUG
        "release",
        #endif
    };

    public IEnumerable<AssemblyPackageInfo> GetValidatedAndLicensedPackageInfos()
    {
        return m_packageInfos;
    }

    public bool ValidPackagePath(string a_filePath)
    {
        foreach (string excludedPackage in ExcludedPackages)
        {
            if (a_filePath.ToLower().Contains(excludedPackage))
            {
                return false;
            }
        }

        return true;
    }

    protected void RunCertificateValidation(string a_dllFilePath)
    {
        //TODO: V12 - How to validate partner packages?
        return;

        Assembly asm = Assembly.LoadFile(a_dllFilePath);
        Module m = asm.GetModules()[0];

        //X509Certificate executingCert = m.GetSignerCertificate();

        // Do the preliminary validation.
        //X509Certificate2 primaryCert = new X509Certificate2(executingCert ?? throw new Exception());
        //X509CertificateValidator chainTrustValidator = X509CertificateValidator.ChainTrust;
        //chainTrustValidator.Validate(primaryCert);

        //validate certificate info
        //string subject = primaryCert.Subject;
        //if (subject == null || !(subject.Contains("CN=\"PlanetTogether, Inc.\"") || !subject.Contains("L=Encinitas")))
        //{
        //    throw new PTValidationException("Invalid package");
        //}
    }
    #endregion

    #region Error Logging
    public void PackageLoadException(PTPackageException a_exception)
    {
        SimpleExceptionLogger.LogException(ServerSessionManager.PackageErrorsLogPath, a_exception.GetExceptionFullMessage());
    }

    public void LogPackageError(string a_message)
    {
        try
        {
            SimpleExceptionLogger.LogPackageException(ServerSessionManager.PackageErrorsLogPath, a_message);
        }
        catch { }
    }
    #endregion
}