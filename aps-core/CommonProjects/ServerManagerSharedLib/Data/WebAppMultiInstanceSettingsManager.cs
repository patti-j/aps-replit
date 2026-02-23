using PT.Common.Extensions;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs;
using PT.ServerManagerSharedLib.DTOs.Entities;

using PT.ServerManagerSharedLib.DTOs.Responses;

using System.Text.Json;

namespace PT.ServerManagerSharedLib.Data;

/// <summary>
/// Extension of the <see cref="WebAppInstanceSettingsManager"/> that has privileged abilities to manage multiple instances/ server data.
/// To allow this, it requires different authorization configured - a Server Auth token, rather than the Instance one used by its base class.
/// </summary>
public class WebAppMultiInstanceSettingsManager : WebAppInstanceSettingsManager, IMultiInstanceSettingsManager
{
    public WebAppMultiInstanceSettingsManager(string a_instanceId, string a_serverAuthToken, string env)
        : base(a_instanceId, a_serverAuthToken, env) {}

    protected override void SetAuth(string a_instanceId, string a_authToken)
    {
        m_httpClient.AddServerManagerAuthorization(a_authToken);
    }

    protected override APSInstanceEntity GetInstanceFromWebApp(string a_instanceName, string a_instanceVersion)
    {
        InstanceFromWebAppDto instanceDto = m_httpClient.GetInstanceByNameVersion(a_instanceName, a_instanceVersion).Result;
        if (instanceDto == null)
        {
            throw new Exception("No instance found");
        }

        return BuildInstanceFromDto(instanceDto);
    }

    /// <summary>
    /// Deletes an instance.
    /// The only arg required for this implementation is <see cref="a_instanceIdentifier"/> aka "PlanningAreaKey" -
    /// the Name/Version args were the old means of identifier an instance in the previous implementation, but are supported as well.
    /// </summary>
    /// <param name="a_instanceIdentifier"></param>
    /// <param name="a_instanceName"></param>
    /// <param name="a_instanceVersion"></param>
    /// <returns></returns>
    public bool DeleteInstance(string a_instanceIdentifier, string a_instanceName, string a_instanceVersion)
    {
        try
        {
            if (a_instanceIdentifier.IsNullOrEmpty())
            {
                APSInstanceEntity instance = GetInstanceFromWebApp(a_instanceName, a_instanceVersion);
                a_instanceIdentifier = instance.Settings.InstanceId;
            }

            string actionParams = JsonSerializer.Serialize(new WebInstanceIdentifierResponseParams()
            {
                PlanningAreaKey = a_instanceIdentifier
            });

            WebApiActionFromServer webAppAction = new WebApiActionFromServer()
            {
                Parameters = actionParams,
                Status = EActionRequestStatuses.NewFromServer.ToString(),
                ActionType = EServerActionTypes.DeletePlanningArea.ToString()
            };

            m_httpClient.SendNewAction(webAppAction).Wait();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Delete all instances for a particular server.
    /// </summary>
    /// <returns></returns>
    public bool DeleteAllInstances()
    {
        try
        {
            WebApiActionFromServer webAppAction = new WebApiActionFromServer()
            {
                Parameters = string.Empty,
                Status = EActionRequestStatuses.NewFromServer.ToString(),
                ActionType = EServerActionTypes.UnregisterServer.ToString()
            };

            m_httpClient.SendNewAction(webAppAction);

            return true;
        }
        catch
        {
            return false;
        }
    }


    public IEnumerable<APSInstanceEntity> GetInstances()
    {
        var configJsons = m_httpClient.GetAllInstancesFromWebApp().Result;
        if (configJsons == null)
        {
            throw new Exception("Failed to connect to WebApp");
        }
        var instances = configJsons.Select(x => { return BuildInstanceFromDto(x); });

        return instances;
    }

    /// <summary>
    /// Handles saving instances in a single bulk transaction.
    /// </summary>
    /// <param name="a_instances"></param>
    public void SaveInstances(IEnumerable<APSInstanceEntity> a_instances)
    {
        foreach (APSInstanceEntity instance in a_instances)
        {
           SaveInstance(instance);
        }
    }

    /// <summary>
    /// Writes Server-level config data to the database
    /// </summary>
    /// <param name="a_thumbprint"></param>
    /// <param name="a_serverManagerPath"></param>
    public void SaveServerwideSettings(ServerSettingsDto a_settings)
    {
        string paramsJson = JsonSerializer.Serialize(new WebUpdateServerSettingsRequestParams()
        {
            ServerSettings = a_settings
        });

        try
        {
            WebApiActionFromServer webAppAction = new WebApiActionFromServer()
            {
                Parameters = paramsJson,
                Status = EActionRequestStatuses.NewFromServer.ToString(),
                ActionType = EServerActionTypes.UpdateServerSettings.ToString()
            };

            m_httpClient.SendNewAction(webAppAction);
        }
        catch
        {
            // TODO: log
        }
    }

    public void SetAsBackup(APSInstanceEntity newVersion, APSInstanceEntity oldVersion)
    {
        string paramsJson = JsonSerializer.Serialize(new WebUpgradeInstanceResponseParams()
        {
            InstanceName = newVersion.PublicInfo.InstanceName,
            NewVersion = newVersion.PublicInfo.SoftwareVersion,
            OldVersion = oldVersion.PublicInfo.SoftwareVersion
        });

        try
        {
            WebApiActionFromServer webAppAction = new WebApiActionFromServer()
            {
                Parameters = paramsJson,
                Status = EActionRequestStatuses.NewFromServer.ToString(),
                ActionType = EServerActionTypes.UpgradePlanningArea.ToString()
            };

            m_httpClient.SendNewAction(webAppAction).Wait();
        }
        catch
        {
            // TODO: log
        }
    }
}