using PT.Common.File;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.DTOs.Entities;

using Instance = PT.ServerManagerSharedLib.DTOs.Entities.Instance;
using InstanceKey = PT.Common.Http.InstanceKey;

namespace PT.PlanetTogetherAPI.Importing;

public class ImportUtilities
{
    private readonly IInstanceSettingsManager m_settingsManager;
    internal string WorkingDirectory { get; }
    internal string ServiceName { get; }
    internal StartupVals StartupVals { get; }

    public ImportUtilities(string a_serviceName, Instance a_initialInstanceSettings, IInstanceSettingsManager a_instanceSettingsManager)
    {
        ServiceName = a_serviceName;
        m_settingsManager = a_instanceSettingsManager;
        WorkingDirectory = a_initialInstanceSettings?.ServicePaths.InterfaceServiceWorkingDirectory;
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }

        StartupVals = GetBroadcasterConstructorValues();
    }

    public InstanceSettingsEntity GetInstance()
    {
        InstanceKey instanceKey = new (ServiceName);
        return m_settingsManager.GetInstanceSettingsEntity(instanceKey.InstanceName, instanceKey.SoftwareVersion);
    }

    public ErpDatabase GetErpSettings()
    {
        InstanceKey instanceKey = new (ServiceName);
        return m_settingsManager.GetErpDatabaseSettings(instanceKey.InstanceName, instanceKey.SoftwareVersion);
    }

    public StartupVals GetBroadcasterConstructorValues()
    {
        InstanceKey instanceKey = new (ServiceName);
        return m_settingsManager.GetStartupVals(instanceKey.InstanceName, instanceKey.SoftwareVersion);
    }

    private static string GetUrl(string a_url)
    {
        UriBuilder uriBuilder = new (a_url) { Scheme = "https" };
        return uriBuilder.Uri.AbsoluteUri;
    }

    private string LogDirectory => Path.Combine(WorkingDirectory, "Logs");

    private string ServiceLogMessageFilePath => Path.Combine(LogDirectory, "InterfaceService.Log");

    private string ServiceLogErrorFilePath => Path.Combine(LogDirectory, "InterfaceServiceExceptions.Log");

    private string SettingsErrorLogFilePath => Path.Combine(LogDirectory, "PTSettingSaver.log");

    private string ImportLogFilePath => Path.Combine(LogDirectory, "PTImporter.log");

    public void LogMessage(string a_description)
    {
        SimpleExceptionLogger.LogException(ServiceLogMessageFilePath, a_description);
    }

    public void LogError(string a_description, Exception a_e)
    {
        SimpleExceptionLogger.LogException(ServiceLogErrorFilePath, a_e, a_description);
    }

    internal void LogSettingsError(Exception a_e, string a_msg)
    {
        SimpleExceptionLogger.LogException(SettingsErrorLogFilePath, a_e, a_msg);
    }

    internal void LogSettingsError(string a_msg)
    {
        SimpleExceptionLogger.LogException(SettingsErrorLogFilePath, a_msg);
    }

    internal void LogImporterException(Exception e)
    {
        SimpleExceptionLogger.LogException(ImportLogFilePath, e);
    }

    internal void LogImporterException(ExceptionDescriptionInfo a_ei, ScenarioExceptionInfo a_sei)
    {
        SimpleExceptionLogger.LogException(ImportLogFilePath, a_ei, a_sei, "");
    }
}