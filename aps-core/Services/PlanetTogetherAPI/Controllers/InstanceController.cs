using Microsoft.AspNetCore.Mvc;

using PT.Common.Http;
using PT.Common.Sql;
using PT.Scheduler;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.ServerManagerSharedLib.Exceptions;
using PT.ServerManagerSharedLib.Helpers;

namespace PT.PlanetTogetherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : ControllerBase
{
    private InstanceActionsClient m_sac;
    private ServerSessionManager SessionManager => SystemController.ServerSessionManager;

    [HttpGet]
    [Route("GetInstanceStatus")]
    public ActionResult<ServiceStatus> GetInstanceStatus()
    {
        ServiceStatus status = SessionManager.GetServiceStatus();
        return Ok(status);
    }

    [HttpGet]
    [Route("GetThumbprint")]
    public ActionResult<string> GetThumbprint()
    {
        IInstanceSettingsManager instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(SessionManager.ConstructorValues.InstanceDatabaseConnectionString,
            Environment.MachineName, SessionManager.ConstructorValues.InstanceId, SessionManager.ConstructorValues.ApiKey, SessionManager.ConstructorValues.WebAppEnv);

        string thumbprint = instanceSettingsManager.GetCertificateThumbprint(SessionManager.ConstructorValues.InstanceName, SessionManager.ConstructorValues.SoftwareVersion);

        return Ok(thumbprint);
    }

    [HttpPost]
    [Route("TestConnectionString")]
    public ActionResult<BoolResponse> TestConnectionString(TestConnectionStringRequest a_connectionStringRequest)
    {
        if (!ConnectionValidation.ValidateConnectionString(a_connectionStringRequest.ConnectionString))
        {
            return BadRequest("Invalid connection string format");
        }

        if (!ConnectionValidation.TestConnectionString(a_connectionStringRequest.ConnectionString))
        {
            return BadRequest("Invalid connection parameters.");
        }

        return Ok(new BoolResponse { Content = true });
    }
}