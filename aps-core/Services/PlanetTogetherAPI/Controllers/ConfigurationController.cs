using Microsoft.AspNetCore.Mvc;

using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Helpers;

namespace PT.PlanetTogetherAPI.Controllers;

[UseInConfigMode]
[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private ServerSessionManager m_sessionManager => SystemController.ServerSessionManager;
    private IInstanceSettingsManager m_settingsManager;

    public ConfigurationController()
    {
        m_settingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_sessionManager.ConstructorValues.InstanceDatabaseConnectionString,
            Environment.MachineName, m_sessionManager.ConstructorValues.InstanceId, m_sessionManager.ConstructorValues.ApiKey, m_sessionManager.ConstructorValues.WebAppEnv);
    }

    // TODO: implement if needed, or remove if we end up managing instance creation without APIs
    //TODO: What auth is required (beyond local host constraint)?
    ///// <summary>
    ///// Returns the full collection of an instance's settings.
    ///// </summary>
    ///// <returns></returns>
    //[HttpPost]
    //[LocalHostConstraint]
    //[Route("GetInstanceSettings")]
    //public ActionResult<APSInstanceEntity> GetInstanceSettings()
    //{
    //    

    //    return Ok(new BoolResponse { Content = true });
    //}

    ///// <summary>
    ///// Saves the instance's settings to the corresponding instance database.
    ///// Consists of an <see cref="APSInstanceEntity"/> containing all instance-specific settings, along with
    ///// <see cref="ServerWideInstanceSettings"/> that represent the settings common to all instances managed by a particular server.
    ///// The latter will be updated automatically at any later time when the managing server updates those values.
    ///// </summary>
    ///// <param name="a_saveInstanceSettingsRequest"></param>
    ///// <returns></returns>
    //[HttpPost]
    //[LocalHostConstraint]
    //[Route("CreateInstanceSettings")]
    //public ActionResult<BoolResponse> CreateInstanceSettings(CreateInstanceSettingsRequest a_saveInstanceSettingsRequest)
    //{
    //    if (a_saveInstanceSettingsRequest.InstanceSettings.PublicInfo.InstanceId != m_sessionManager.ConstructorValues.InstanceId)
    //    {
    //        return BadRequest("Cannot update settings for a different instance.");
    //    }

    //    m_settingsManager.SaveInstance(a_saveInstanceSettingsRequest.InstanceSettings);

    //    m_settingsManager.CreateInstanceConnectionFile(a_saveInstanceSettingsRequest.InstanceSettings, m_sessionManager.ConstructorValues.InstanceDatabaseConnectionString);

    //    if (a_saveInstanceSettingsRequest.ServerWideInstanceSettings != null)
    //    {
    //        m_settingsManager.SaveServerwideSettings(a_saveInstanceSettingsRequest.ServerWideInstanceSettings);
    //    }

    //    return Ok(new BoolResponse { Content = true });
    //}

    ///// <summary>
    ///// Updates the instance's settings.
    ///// It is expected that these settings have already been created in the database; if initializing settings for the first time, use <see cref="CreateInstanceSettings"/>
    ///// </summary>
    ///// <param name="a_instanceSettings"></param>
    ///// <returns></returns>
    //[HttpPost]
    //[LocalHostConstraint]
    //[Route("SaveInstanceSettings")]
    //public ActionResult<BoolResponse> SaveInstanceSettings(APSInstanceEntity a_instanceSettings)
    //{
    //    if (a_instanceSettings.PublicInfo.InstanceId != m_sessionManager.ConstructorValues.InstanceId)
    //    {
    //        return BadRequest("Cannot update settings for a different instance.");
    //    }

    //    APSInstanceEntity existingInstanceRecord = m_settingsManager.GetInstance(m_sessionManager.ConstructorValues.InstanceName, m_sessionManager.ConstructorValues.SoftwareVersion);

    //    if (existingInstanceRecord != null)
    //    {
    //        return BadRequest($"Settings already exist for the instance {a_instanceSettings.PublicInfo.InstanceId}. To update, use the /SaveInstanceSettings route.");
    //    }

    //    m_settingsManager.SaveInstance(a_instanceSettings);

    //    return Ok(new BoolResponse { Content = true });
    //}
}