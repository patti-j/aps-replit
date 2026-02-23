namespace PipelinesPublisher;

internal class BuildPaths
{
    private readonly string m_basePath;
    internal const string c_extensionsDestinationFolderName = "CustomerPackages";

    public string BasePath => m_basePath;
    private readonly string m_version;

    internal BuildPaths(string a_basePath)
    {
        m_version = AssemblyVersionChecker.GetAssemblyVersion().ToString();
        m_basePath = a_basePath;
    }

    #region Deployment Folders
    internal string GetDeploymentFolder()
    {
        return Path.Combine(m_basePath, "Deployment");
    }

    internal string GetDeploymentFolderForServerManager()
    {
        return Path.Combine(GetDeploymentFolder(), "ServerManager");
    }

    internal string GetDeploymentVersionFolder()
    {
        return Path.Combine(GetDeploymentFolder(), m_version);
    }

    internal string GetSoftwarePublishZipName()
    {
        return Path.Combine(GetSoftwarePublishDirectory(), $"{m_version}.zip");
    }

    internal string GetSoftwarePublishDirectory()
    {
        return Path.Combine(GetPublishFolder(), "Software");
    }

    internal string GetExtensionPublishDirectory()
    {
        return Path.Combine(GetPublishFolder(), c_extensionsDestinationFolderName);
    }

    internal string GetSamplePublishDirectory()
    {
        return Path.Combine(GetPublishFolder(), "SamplePackages");
    }

    //internal string GetPTIntegrationPublishFolder()
    //{
    //    return Path.Combine(GetPublishFolder(), "IntegrationFiles");
    //}

    internal string GetPublishFolder()
    {
        return Path.Combine(m_basePath, "Publish", "PTLocalInstallationFiles");
    }

    internal string GetDeploymentProgramFilesFolder()
    {
        return Path.Combine(GetDeploymentVersionFolder(), "ProgramFiles");
    }

    internal string GetDeploymentProgramFilesFolderForSystem()
    {
        return Path.Combine(GetDeploymentProgramFilesFolder(), "System");
    }

    internal string GetDeploymentVersionUpdateFilesFolder()
    {
        return Path.Combine(GetDeploymentVersionFolder(), "UpdateFiles");
    }
    #endregion

    #region Source Folders
    //internal string GetSourceFolderForIntegrationFiles()
    //{
    //    return Path.Combine(m_basePath, "DeploymentFiles", "IntegrationFiles", "PlanetTogether");
    //}

    internal string GetSourceFolderForSystem()
    {
        return Path.Combine(m_basePath, "Software");
    }

    internal string GetSourceFolderForUpdateFiles()
    {
        return Path.Combine(m_basePath, "UpdateFiles");
    }

    internal string GetSourceFolderForClient()
    {
        return Path.Combine(m_basePath, "Client");
    }

    internal string GetSourceFolderForPackages()
    {
        return Path.Combine(m_basePath, "Packages");
    }

    internal string GetSourceFolderForExtensions()
    {
        return Path.Combine(m_basePath, "Extensions");
    }

    internal string GetSourceFolderForSamples()
    {
        return Path.Combine(m_basePath, "Samples");
    }
    #endregion
}