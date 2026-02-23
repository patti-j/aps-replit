using System.Net;

using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Http;
using PT.PackageDefinitions;
using PT.PackageDefinitions.DTOs;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.SystemServiceDefinitions;
using PT.SystemServiceDefinitions.Headers;

using SoftwareVersion = PT.Common.SoftwareVersion;

//TODO: Return not found on nulls
namespace PT.PlanetTogetherAPI.Controllers;

/// <summary>
/// This is the web wrapper for the ServerSessionManager.
/// This controller will accept the web request and forward it to the ServerSessionManager and return the result if applicable
/// A client running externally would not use or create this class.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SystemServiceController : SessionControllerBase
{
    //TODO: Take ServerSessionManager as a parameter from the middlewears

    private ServerSessionManager SessionManager => SystemController.ServerSessionManager;

    /// <summary>
    /// Broadcasts transmission to all clients.
    /// The body of this request is sent as an unserialized byte array.
    /// </summary>
    /// <param name="a_request"></param>
    /// <returns></returns>
    [HttpPost]
    [RequestSizeLimit(200000000)] // This endpoint could exceed the standard body size limit (~25MB) in cases like imports. Upping to 200MB.
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("SendTransmission")]
    [Consumes("application/octet-stream")]
    public ActionResult<BoolResponse> SendTransmission([FromBody] MemoryStream transmissionStream)
    {
        if (!Request.ContentLength.HasValue || Request.ContentLength.Value <= 0)
        {
            return BadRequest("No content in the request.");
        }

        SystemController.ServerSessionManager.TransmissionReceived(transmissionStream.ToArray(), UserToken);

        return Ok(new BoolResponse { Content = true });
    }

    /// <summary>
    /// Log of by removing the connection on ServerSessionManager.
    /// </summary>
    /// <param name="a_logOffRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("LogOff")]
    public ActionResult<BoolResponse> LogOff()
    {
        SystemController.ServerSessionManager.LogOff(UserToken);
        return Ok(new BoolResponse { Content = true });
    }

    /// <summary>
    /// Use this to login to SystemService. Creates a connection on the server on which transmissions will
    /// be broadcasted and returns the data required to create PTSystem.
    /// </summary>
    /// <param name="a_request"></param>
    /// <returns></returns>
    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetScenarios")]
    public ActionResult GetSystem()
    {
        byte[] systemBytes = SessionManager.GetStartupSystem(UserToken, out SoftwareVersion productVersion, out string o_exceptionMessage, out long waitDuration);

        return new FileStreamResult(new MemoryStream(systemBytes), "application/octet-stream");
    }

    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetAllUnloadedScenarioData")]
    public ActionResult GetAllUnloadedScenarioData()
    {
        byte[] closedScenarioData = SessionManager.GetAllUnloadedScenarioData(UserToken);

        if (closedScenarioData.IsNullOrEmpty())
        {
            return NoContent();
        }
        
        return new FileStreamResult(new MemoryStream(closedScenarioData), "application/octet-stream");
    }

    /// <summary>
    /// Returns the byte contents of scenarios.dat as encoded through a <see cref="BinaryFileWriter" />.
    /// These bytes may not align with the output of a <see cref="BinaryMemoryWriter" />, and thus the byte arrays aren't interchangeable.
    /// This method should only ever be needed if the intent is to directly write the scenarios to disk on the client side.
    /// Unlike <see cref="GetSystem" />, it does not create a connection on the server.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetScenariosForFileStream")]
    public ActionResult GetScenariosForFileStream(DownloadScenarioRequest a_downloadScenarioRequest)
    {
        byte[] scenarioBytes = SessionManager.RetrieveScenariosAsFileBytes(a_downloadScenarioRequest);

        return new FileStreamResult(new MemoryStream(scenarioBytes), "application/octet-stream");
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("UpdateUserLoadedScenarioIds")]
    public ActionResult<BoolResponse> UpdateUserLoadedScenarioIds(UpdateLoadedScenarioIdsRequest a_updateLoadedScenarioIdsRequest)
    {
        bool updateSuccess;
        try
        {
            updateSuccess = SessionManager.UpdateUserSessionLoadedScenarioIds(a_updateLoadedScenarioIdsRequest.IsAddId, new BaseId(a_updateLoadedScenarioIdsRequest.ScenarioId), UserToken);
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("UpdateUserLoadedScenarioIds", e);
            return BadRequest(e.Message);
        }
        return Ok(new BoolResponse { Content = updateSuccess });
    }
    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("UndoSetsByScenarioId")]
    public ActionResult<List<UndoAction>> UndoSetsByScenarioId(ApsWebServiceScenarioRequest a_request)
    {
        try
        {
            return Ok(SessionManager.GetScenarioUndoSets(new BaseId(a_request.ScenarioId), UserToken));
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("UndoSetsByScenarioIds", e);
            return BadRequest(e.Message);
        }
    }
    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("UndoIdByTransmissionNbr")]
    public ActionResult<GetUndoIdxByTransmissionNbrResponse> UndoIdByTransmissionNbr(GetUndoIdxByTransmissionNbrRequest a_request)
    {
        try
        {
            GetUndoIdxByTransmissionNbrResponse response = new GetUndoIdxByTransmissionNbrResponse();
            response.UndoIdx = SessionManager.GetUndoIdByTransmissionNbr(new BaseId(a_request.ScenarioId), a_request.TransmissionNbr);
            return Ok(response);
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("UndoIdByTransmissionNbr", e);
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("GetServerSoftwareVersion")]
    public ActionResult<SoftwareProductVersion> GetServerSoftwareVersion()
    {
        SoftwareVersion productVersion = SessionManager.ProductVersion;
        SoftwareProductVersion softwareProductVersion = new ()
        {
            Major = productVersion.Major,
            Minor = productVersion.Minor,
            Hotfix = productVersion.Hotfix,
            Revision = productVersion.Revision
        };

        return Ok(softwareProductVersion);
    }

    /// <summary>
    /// Returns serialized TransmissionArraySet with all the transmissions that have been broadcasted
    /// on the connection (a_connectionNbr) since the last receive.
    /// </summary>
    /// <param name="a_receiveRequest"></param>
    /// <returns></returns>
    [HttpPost]
    //[Authorize("AppUser,false")]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("RetrieveNextAction")]
    public ActionResult<GetTransmissionResponse> RetrieveNextAction(GetTransmissionRequest a_request)
    {
        try
        {
            return SystemController.ServerSessionManager.ReturnNextTransmissionContainer(UserToken, a_request.LastProcessedTransmissionId);
        }
        catch (Exception ex ) when (ex is ApiException)
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
    }

    /// <summary>
    /// Get serialized scenario
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetScenario")]
    public ActionResult GetScenario(GetScenarioRequest a_request)
    {
        byte[] scenarioBytes = SystemController.ServerSessionManager.GetScenarioBytes(a_request.ScenarioId);

        if (scenarioBytes == null)
        {
            return null;
        }
        return new FileStreamResult(new MemoryStream(scenarioBytes), "application/octet-stream");
    }


    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetLoggedInInstanceData")]
    public ActionResult<LoggedInInstanceData> GetLoggedInInstanceData()
    {
        ServerSessionManager ptBroadcaster = SystemController.ServerSessionManager;
        byte[] encryptedData = ptBroadcaster.GetLoggedInInstanceData();
        LoggedInInstanceData instanceData = new ()
        {
            EncryptedInstanceData = encryptedData
        };
        return Ok(instanceData);
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetPackedPackageAssembly")]
    public ActionResult<PackedAssembly> GetPackedPackageAssembly(AssemblyPackageInfo a_AssemblyPackageInfo)
    {
        ServerSessionManager ptBroadcaster = SystemController.ServerSessionManager;
        PackedAssembly packageAssembly = ptBroadcaster.GetPackedPackageAssembly(a_AssemblyPackageInfo);

        if (packageAssembly == null)
        {
            return NotFound($"Can not find {a_AssemblyPackageInfo.AssemblyFileName}:{a_AssemblyPackageInfo.Version}");
        }

        return Ok(packageAssembly);
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetPackagesOnServer")]
    public ActionResult<AssemblyPackageInfo[]> GetPackagesOnServer()
    {
        ServerSessionManager ptBroadcaster = SystemController.ServerSessionManager;
        return Ok(ptBroadcaster.GetPackagesOnServer());
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetLicenseKeyForInstance")]
    public ActionResult<string> GetLicenseKeyForInstance()
    {
        string path = System.IO.File.ReadAllText(PTSystem.WorkingDirectory.KeyFilePath);
        return Ok(path);
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetStartupVals")]
    public ActionResult<StartupValsAdapter> GetStartupVals()
    {
        StartupVals startupVals = SystemController.ServerSessionManager.ConstructorValues;
        return Ok(startupVals);
    }

    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetInterfaceSettings")]
    public ActionResult<StartupVals> GetInterfaceSettings()
    {
        StartupVals startupVals = SystemController.ServerSessionManager.ConstructorValues;
        return Ok(startupVals);
    }

    //Example package groupings file
    //{
    //    "Demo": ["Branding", "Core", "Demo"],
    //    "Integration": ["IntegrationMapping", "InventoryPlanning", "ProductRulesAndResourceEfficiency"],
    //    "Minimal": ["Branding", "Core"],
    //    "DropSheets": ["Branding", "Core", "DropSheets"],
    //}
    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetPackageGroupings")]
    public ActionResult<string> GetPackageGroupings()
    {
        string groupingsFilePath = Path.Combine(PTSystem.WorkingDirectory.PackagesPath, "PackageGroupings.json");
        string fileContent = System.IO.File.ReadAllText(groupingsFilePath);

        return Ok(fileContent);
    }

    [HttpGet]
    [Route("GetScenarioChecksum")]
    public ActionResult GetChecksum(string a_scenarioId, string a_transmissionId)
    {
        if (!int.TryParse(a_scenarioId, out int scenarioId))
        {
            //SimpleExceptionLogger.LogException("GetScenarioChecksum", e);
            return BadRequest("Scenario not found");
        }

        ChecksumValues checksum = SessionManager.GetChecksum(scenarioId, Guid.Parse(a_transmissionId)).Result;
        if (checksum == null)
        {
            SessionManager.LogMissingChecksum(new BaseId(a_scenarioId), Guid.Parse(a_transmissionId), UserToken);
            // TODO: Implement a cache of transmissionId values for all transmissions the serverside dispatcher has picked up.
            // TODO: Check for the current transmissionId in that cache - if not there, the transmission isn't merely unprocessed, it's missing.
            // TODO: Differentiate responses based on this, so the client knows if its worth retrying.
            return NotFound();
        }

        // We don't want to transfer the sizeable scenario description data, as that processing will happen serverside
        ChecksumValues checksumCopy = new ChecksumValues(
            checksum.ScenarioId,
            checksum.StartAndEndSums,
            checksum.ResourceJobOperationCombos,
            checksum.BlockCount,
            string.Empty,
            checksum.TransmissionId
            );

        byte[] checksumBytes;
        using (BinaryMemoryWriter writer = new ())
        {
            checksumCopy.Serialize(writer);
            checksumBytes = writer.GetBuffer();
        }

        return new FileStreamResult(new MemoryStream(checksumBytes), "application/octet-stream");
    }

    [HttpPost]
    [Consumes("application/octet-stream")]
    [Route("HandleChecksumDiscrepancy")]
    public ActionResult<BoolResponse> HandleChecksumDiscrepancy([FromBody] MemoryStream a_clientChecksumAndLogContentStream)
    {
        ChecksumValues clientChecksum;
        StringArrayList logContent;
        using (BinaryMemoryReader reader = new BinaryMemoryReader(a_clientChecksumAndLogContentStream.ToArray()))
        {
            clientChecksum = new ChecksumValues(reader);
            logContent = new StringArrayList(reader);
        }

        ChecksumValues serverChecksum = SessionManager.GetChecksum(clientChecksum.ScenarioId.Value, clientChecksum.TransmissionId).Result;
        if (serverChecksum == null)
        {
            // The server is somehow missing the corresponding transmission checksum.
            // Perhaps an error is more appropriate?
            return NotFound();
        }

        // Get scenario and user data for logging.
        // In the unlikely event that either of these are deleted before this method is called, we want to continue with as much diagnostic info as possible.
        Scenario s;
        string scenarioName = "Unknown";
        long scenarioId = BaseId.NULL_ID.Value;
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            s = sm.Find(clientChecksum.ScenarioId);
            scenarioName = s?.Name;
            scenarioId = s?.Id.Value ?? BaseId.NULL_ID.Value;
        }

        ConnectedUserData user = SessionManager.GetLoggedInUserData(UserToken);

        SessionManager.LogDesync(serverChecksum, clientChecksum, user, scenarioName, scenarioId, logContent);

        return Ok(new BoolResponse { Content = true });
    }

    [HttpGet]
    [Route("GetInstanceStatus")]
    public ActionResult<ServiceStatus> GetInstanceStatus()
    {
        ServiceStatus status = SessionManager.GetServiceStatus();
        return Ok(status);
    }

    [HttpGet]
    [Route("GetLoggedInUserData")]
    public ActionResult<ConnectedUserData[]> GetLoggedInUserData()
    {
        ApiLogger al = new ("GetLoggedInUserData", ControllerProperties.ApiDiagnosticsOn, TimeSpan.FromSeconds(30));
        al.LogEnter();

        ConnectedUserData[] activeUsers = SystemController.ServerSessionManager.GetLoggedInUserData();

        al.LogFinishAndReturn(EApsWebServicesResponseCodes.Success);
        return Ok(activeUsers);
    }

    [HttpPost]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("SaveScenarioToDisk")]
    public ActionResult<BoolResponse> SaveScenarioToDisk()
    {
        SessionManager.SaveToDisk();
        return Ok(new BoolResponse { Content = true });
    }

    /// <summary>
    /// Gets the content of a log file from the server.
    /// This will contain all log content for the duration of the instance, compared to the session-specific logs the client will have.
    /// </summary>
    /// <param name="a_logTitle">
    /// Log name - should be a value from <see cref="PackageEnums.ELogTypes" />.
    /// Matching client log can be acquired from <see cref="IErrorLogger.GetLogTitle()" />
    /// </param>
    /// <param name="a_showDetails"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetSystemLog")]
    public ActionResult<string> GetSystemLog(string a_logTitle, bool a_showDetails, int a_maxLogCount = 1000)
    {
        if (!Enum.TryParse(a_logTitle, out PackageEnums.ELogTypes logType))
        {
            throw new ArgumentException($"Provided invalid log title {a_logTitle}.");
        }

        if (logType != PackageEnums.ELogTypes.Interface)
        {
            throw new ArgumentException($"Invalid request for log '{a_logTitle}'. " +
                                        $"Only {PackageEnums.ELogTypes.Interface.ToString()} server log can be accessed.");
        }

        string log = SessionManager.GetLogContent(logType, a_showDetails);

        log = SimpleExceptionLogger.TruncateLogs(log, a_maxLogCount);

        return Ok(log);
    }

    /// <summary>
    /// Returns a map of all scenario permissions. For each scenario (by id), shows the permissions for each user and each group.
    /// </summary>
    /// <param name="a_groupId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetScenarioPermissions")]
    public ActionResult<ScenarioPermissionsResponse> GetScenarioPermissions()
    {
        List<ScenarioPermissionSet> scenarioPermissions = SessionManager.GetAllScenarioPermissions();

        return Ok(new ScenarioPermissionsResponse
        {
            ScenarioPermissionSets = scenarioPermissions.ToDictionary(x => x.ScenarioId, x => x)
        });
    }
}