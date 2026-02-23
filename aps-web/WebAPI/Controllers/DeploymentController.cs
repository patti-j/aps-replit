using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using WebAPI.DAL;
using WebAPI.RequestsAndResponses;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeploymentController : ControllerBase
    {
        private readonly AzureTableService m_tableService;
        private readonly AzureBlobService m_blobService;
        private readonly ILogger<DeploymentController> m_logger;

        public DeploymentController(AzureTableService a_service, ILogger<DeploymentController> a_logger, AzureBlobService a_blobService)
        {
            m_tableService = a_service;
            m_logger = a_logger;
            m_blobService = a_blobService;
        }

        /// <summary>
        /// Get a list of Client Agent versions from Azure Table metadata Storage
        /// </summary>
        /// <returns>The list of versions</returns>
        [HttpGet("GetClientAgentVersions")]
        public async Task<ActionResult<StandardVersionResponse>> GetClientAgentVersions()
        {
            try
            {
                IEnumerable<StandardVersionEntity> versions = await m_tableService.GetAllClientAgentVersions();
                StandardVersionResponse response = new StandardVersionResponse(versions);
                return Ok(response);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetClientAgentVersions)}'");
                // TODO: Testing out returning problem (500) on serverside errors. Review this if/when we integrate PTHttpClient, I feel like it may not work as intended.
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get a list of Server Agent versions from Azure Table metadata Storage
        /// </summary>
        /// <returns>The list of versions</returns>
        [HttpGet("GetServerAgentVersions")]
        public async Task<ActionResult<StandardVersionResponse>> GetServerAgentVersions()
        {
            try
            {
                IEnumerable<StandardVersionEntity> versions = await m_tableService.GetAllServerAgentVersions();
                StandardVersionResponse response = new StandardVersionResponse(versions);
                return Ok(response);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetServerAgentVersions)}'");
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get a list of Server Agent versions from Azure Table metadata Storage
        /// </summary>
        /// <returns>The list of versions</returns>
        [HttpGet("GetSoftwareVersions")]
        public async Task<ActionResult<StandardVersionResponse>> GetSoftwareVersions()
        {
            try
            {
                IEnumerable<StandardVersionEntity> versions = await m_tableService.GetAllSoftwareVersions();
                StandardVersionResponse response = new StandardVersionResponse(versions);
                return Ok(response);
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetSoftwareVersions)}'");
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get a specific Client Agent version from Azure Table Storage
        /// </summary>
        /// <param name="version">The version to be downloaded. If not provided, defaults to latest.</param>
        /// <returns>The list of versions</returns>
        [HttpGet("GetClientAgent")]
        public async Task<ActionResult> GetClientAgent(string version = null)
        {
            if (version.IsNullOrEmpty())
            {
                IEnumerable<StandardVersionEntity> versions = await m_tableService.GetAllClientAgentVersions();
                version = versions.OrderByDescending(x => x.VersionDate).FirstOrDefault()?.VersionNumber;
            }

            if (!Version.TryParse(version, out _))
            {
                return BadRequest("Please provide a 4 part version in the form 'x.x.x.x'");
            }

            try
            {
                Stream agentZip = await m_blobService.GetClientAgentZip(version);

                if (agentZip == null)
                {
                    return BadRequest($"Client Agent version {version} does not exist.");
                }

                return File(agentZip, "application/octet-stream", "PlanetTogetherClientAgent.zip"); // Return stream directly, with suggested filename
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetClientAgent)}'");
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get a specific Server Agent version from Azure Table Storage.
        /// </summary>
        /// <param name="version">The version to be downloaded. If not provided, defaults to latest.</param>
        /// <returns>The list of versions</returns>
        [HttpGet("GetServerAgent")]
        public async Task<ActionResult> GetServerAgent(string version = null)
        {
            if (version.IsNullOrEmpty())
            {
                IEnumerable<StandardVersionEntity> versions = await m_tableService.GetAllServerAgentVersions();
                version = versions.OrderByDescending(x => x.VersionDate).FirstOrDefault()?.VersionNumber;
            }

            if (!Version.TryParse(version, out _))
            {
                return BadRequest("Please provide a 4 part version in the form 'x.x.x.x'");
            }

            try
            {
                Stream agentZip = await m_blobService.GetServerAgentZip(version);

                if (agentZip == null)
                {
                    return BadRequest($"Server Agent version {version} does not exist.");
                }

                return File(agentZip, "application/octet-stream", "ServerAgent.zip"); // Return stream directly, with suggested filename
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetServerAgent)}'");
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get a specific Software version from Azure Table Storage.
        /// </summary>
        /// <param name="version">The version to be downloaded. If not provided, defaults to latest.</param>
        /// <returns>The list of versions</returns>
        [HttpGet("GetSoftwareZip")]
        public async Task<ActionResult> GetSoftwareZip(string version = null)
        {
            if (version.IsNullOrEmpty())
            {
                IEnumerable<StandardVersionEntity> versions = await m_tableService.GetAllSoftwareVersions();
                version = versions.OrderByDescending(x => x.VersionDate).FirstOrDefault()?.VersionNumber;
            }

            if (!Version.TryParse(version, out _))
            {
                return BadRequest("Please provide a 4 part version in the form 'x.x.x.x'");
            }

            try
            {
                Stream agentZip = await m_blobService.GetSoftwareZip(version);

                if (agentZip == null)
                {
                    return BadRequest($"Software version {version} does not exist.");
                }

                return File(agentZip, "application/octet-stream", "ServerAgent.zip"); // Return stream directly, with suggested filename
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetSoftwareZip)}'");
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get the Server WebInstaller.
        /// </summary>
        /// <returns>The list of versions</returns>
        [HttpGet("GetWebInstallerServer")]
        public async Task<ActionResult> GetWebInstallerServer()
        {
            try
            {
                string installerName = "PlanetTogetherFull12.exe";
                Stream installerZip = await m_blobService.GetWebInstallerExe(installerName);

                return File(installerZip, "application/octet-stream", "ServerAgent.zip"); // Return stream directly, with suggested filename
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetServerAgent)}'");
                return Problem("An error occured during the request.");
            }
        }

        /// <summary>
        /// Get the Client WebInstaller.
        /// </summary>
        /// <returns>The list of versions</returns>
        [HttpGet("GetWebInstallerClient")]
        public async Task<ActionResult> GetWebInstallerClient()
        {
            try
            {
                string installerName = "PlanetTogetherClient12.exe";
                Stream installerZip = await m_blobService.GetWebInstallerExe(installerName);

                return File(installerZip, "application/octet-stream", "ServerAgent.zip"); // Return stream directly, with suggested filename
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, $"An error occured on endpoint '{nameof(GetServerAgent)}'");
                return Problem("An error occured during the request.");
            }
        }
    }
}