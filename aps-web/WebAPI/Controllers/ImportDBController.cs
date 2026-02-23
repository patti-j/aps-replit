using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using WebAPI.Common;
using WebAPI.DAL;

namespace WebAPI.Controllers
{
    [Route("api/import/")]
    [ApiController]
    public class ImportDBController : ControllerBase
    {
        private readonly ILogger<ImportDBController> _logger;
        private string m_errorString;
        private readonly CompanyDBService _companyDbService;

        public ImportDBController(ILogger<ImportDBController> logger, CompanyDBService companyDbService)
        {
            _logger=logger;
            _companyDbService=companyDbService;
        }
        [HttpPut("UpdateTableData/v1")]
        public IActionResult UpdateTableDataV1()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());

                string requestBody;
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = reader.ReadToEndAsync().Result;
                }

                StringValues tableName = string.Empty;
                StringValues queryString = string.Empty;
                Request.Headers.TryGetValue("TableName", value: out tableName);
                Request.Headers.TryGetValue("QueryString", value: out queryString);
                if (queryString == string.Empty)
                {
                    m_errorString = "Querystring not found in header";
                    return BadRequest(m_errorString);
                }
                if (!CommonMethods.UpdateTable(requestBody, tableName, queryString, ConnectionString, out m_errorString)) return BadRequest(m_errorString);

                return Ok("Success");

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpDelete("DeleteTableData/v1")]
        public IActionResult CustomTableDeleteDataV1()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());
                string requestBody;
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = reader.ReadToEndAsync().Result;
                }

                if (!CommonMethods.DeleteTableData(requestBody, Request.Headers["TableName"], out m_errorString, ConnectionString)) return BadRequest(m_errorString);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Success");
        }
        [HttpDelete("DeleteAllTableData/v1")]
        public IActionResult DeleteAllTableDataV1()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());

                if (!CommonMethods.DeleteAllTableData(Request.Headers["TableName"], out m_errorString, ConnectionString)) return BadRequest(m_errorString);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Success");
        }

        [HttpPost("CreateTable/v1")]
        public IActionResult CreateTableV1()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());

                // Read the request body as a string
                string requestBody;
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = reader.ReadToEndAsync().Result;
                }
                if (!CommonMethods.CreateCustomTable(requestBody, Request.Headers["TableName"], out m_errorString, ConnectionString)) return BadRequest(m_errorString);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Success");
        }

        [HttpPost("ImportDataToTable/v1")]
        public IActionResult ImportDataToTableV1()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());

                // Read the request body as a string
                string requestBody;
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = reader.ReadToEndAsync().Result;
                }
                if (!CommonMethods.ImportToCustomTable(requestBody, Request.Headers["TableName"], out m_errorString, ConnectionString)) return BadRequest(m_errorString);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Success");
        }
        [HttpPost("TriggerImport/v1")]
        public async Task<IActionResult> TriggerImportV1()
        {
            try
            {
                Request.Headers.Add("TableName", "Test"); //This is added to pass validation
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);

                await CommonMethods.TriggerImport(Request, _companyDbService);
            }
            catch (TaskCanceledException ex)
            {
                return Ok("The import operation timed out. Check PlanetTogether for result of long-running action");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Success");
        }

        [HttpGet("GetRecordsFromTable/v1")]
        public IActionResult GetRecordsFromTableV1()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());

                StringValues tableName = string.Empty;
                StringValues queryString = string.Empty;
                StringValues limit = string.Empty;
                StringValues offset = string.Empty;
                StringValues orderBy = string.Empty;
                Request.Headers.TryGetValue("TableName", value: out tableName);
                Request.Headers.TryGetValue("QueryString", value: out queryString);
                Request.Headers.TryGetValue("Limit", value: out limit);
                Request.Headers.TryGetValue("Offset", value: out offset);
                Request.Headers.TryGetValue("OrderBy", value: out orderBy);
                return Ok(CommonMethods.GetRecordsAsJson(tableName, queryString, limit, offset, orderBy, ConnectionString));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("GetRecordsFromTable/v2")]
        public IActionResult GetRecordsFromTableV2()
        {
            try
            {
                if (!CommonMethods.ValidateIntegrationApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());
                StringValues tableName = string.Empty;
                StringValues queryString = string.Empty;
                StringValues limit = string.Empty;
                StringValues offset = string.Empty;
                StringValues orderBy = string.Empty;
                Request.Headers.TryGetValue("TableName", value: out tableName);
                Request.Headers.TryGetValue("QueryString", value: out queryString);
                Request.Headers.TryGetValue("Limit", value: out limit);
                Request.Headers.TryGetValue("Offset", value: out offset);
                Request.Headers.TryGetValue("OrderBy", value: out orderBy);
                return Ok(CommonMethods.GetRecordsAsJsonV2(tableName, queryString, limit, offset, orderBy, ConnectionString));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("GetInstanceSchemaMetaData/v1")]
        public IActionResult GetInstanceSchemaMetaDataV1()
        {
            try
            {
                StringValues instanceName = string.Empty;
                if (!CommonMethods.ValidateBasicApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);
                if(!Request.Headers.TryGetValue("InstanceName", value: out instanceName)) return BadRequest("InstanceName missing in headers") ;
                string ConnectionString = CommonMethods.GetCompanyDBConnectionString(
                    Request.Headers["CompanyId"].ToString(), _companyDbService, Request.Headers["CompanyAPIKey"].ToString(),
                    Request.Headers["InstanceName"].ToString());

                StringValues schema = string.Empty;
                
                if (!Request.Headers.TryGetValue("Schema", value: out schema)) return BadRequest("Schema missing in headers");
               
                return Ok(CommonMethods.GetInstanceSchemaMetaDataAsJson(schema, ConnectionString));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("GetInstances/v1")]
        public IActionResult GetInstancesV1()
        {
            try
            {
                if (!CommonMethods.ValidateBasicApiRequest(out m_errorString, Request.Headers, _companyDbService)) return BadRequest(m_errorString);

                return Ok(_companyDbService.GetInstances(Request.Headers["CompanyId"].ToString()));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }

}
