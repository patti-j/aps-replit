using System.Net;

using LazyCache;
using Microsoft.AspNetCore.Mvc;
using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.Common.File;
using PT.PlanetTogetherAPI.APIs;
using PT.PlanetTogetherAPI.Cacheing;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using EApsWebServicesResponseCodes = PT.APIDefinitions.RequestsAndResponses.EApsWebServicesResponseCodes;
using Inventory = PT.APIDefinitions.RequestsAndResponses.DataDtos.Inventory;
using Item = PT.APIDefinitions.RequestsAndResponses.DataDtos.Item;

namespace PT.PlanetTogetherAPI.Controllers;

[Route("api/[controller]")]
public class CtpController : ServerControllerBase
{
    private const int c_lockTimeoutMs = 5000; // TODO: what's a good value here? How long should the API try to wait for a release before informing the caller the scenario is in use?
    private readonly IScenarioDataCacheManager<InventoryLookup> m_itemCacheManager;

    public CtpController(IAppCache a_cache)
    {
        m_itemCacheManager = new CtpItemsCacheManager(a_cache, c_lockTimeoutMs);
    }


    // Based on MESController
    [HttpPost]
    [Route("Request")]
    public CtpResponse RequestCtp([FromBody] CtpRequest a_ctpRequest)
    {
        ApiLogger al = new("CTP", ControllerProperties.ApiDiagnosticsOn, a_ctpRequest.TimeoutDuration);
        al.LogEnter();

        BaseId instigator;
        try
        {
            // TODO: This is from the MESController, but isn't implemented. I can see how UserDefinitionKeys might be used in its place, but the only relevant one I see is ReserveCtp, which shouldn't block this whole process. Not sure what to do here.
            // TODO: Either way, nothing is implemented to check permissions, so this will only fail based on security settings (some of which we may not care about for webapp users?)
            instigator = Helpers.ValidateUser(a_ctpRequest.UserName, a_ctpRequest.Password, new List<UserDefs.EPermissions>()); 
        }
        catch (WebServicesErrorException wsErr)
        {
            al.LogWebExceptionAndReturn("Add,Helpers.ValidateUser", wsErr);
            return new CtpResponse
            {
                ReturnCode = wsErr.Code,
                ErrorMessage = $"Error validating user credentials: {wsErr.Code}"
            };
        }

        try
        {
            WebServiceProcessors.CtpProcessor ctpProcessor = new(instigator);
            CtpResponse response = ctpProcessor.ProcessRequest(a_ctpRequest);
            response.InstanceId = a_ctpRequest.InstanceId;
            return al.LogCtpResponseAndReturn(response);
        }
        catch (WebServicesErrorException wsErr)
        {
            return al.LogCtpResponseAndReturn(new CtpResponse
            {
                ReturnCode = wsErr.Code,
                ErrorMessage = $"Error processing CtpRequest: {wsErr.Code}"
            });
        }
        catch (Exception err)
        {
            return al.LogCtpResponseAndReturn(new CtpResponse
            {
                ReturnCode = EApsWebServicesResponseCodes.Failure,
                ErrorMessage = $"Error processing CtpRequest: {err}"
            });
        }
    }


    // TODO: Use ScenarioPermissions in header
    /// <summary>
    /// Gets all item entities as needed for a CTP request. Accepts querying, providing a subset of fields to return, and pagination.
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="query"></param>
    /// <param name="fields">The fields to populate in the return value. Omitting this will return all fields. Otherwise, non-provided fields will return as null in the model (Or the appropriate default for non-null types.).</param>
    /// <param name="limit"></param>
    /// <param name="offset"></param>
    /// <returns>200 if successful with all requested Items, 400 if no scenario exists for this id, and 423 if loading the cache was required and the call timed out attempted to get a lock on data.</returns>
    [HttpGet]
    [Route("Inventory")]
    public ActionResult<InventoryResponse> GetCtpInventory(long scenarioId, string query = null, string fields = null, int limit = 50, int offset = 0)
    {
        List<Inventory> dtos = new List<Inventory>();

        try
        {
            InventoryLookup inventoriesLookup = m_itemCacheManager.GetOrLoadCache(scenarioId);

            foreach ((Scheduler.Inventory Inventory, List<Warehouse> WarehousesContainingItem) lookup in inventoriesLookup.Lookup.Values)
            {
                if (Helpers.IsItemInRequestQuery(query, lookup.Inventory.Item)) // Use Item's ExternalId to filter, since Inventory doesn't have one
                {
                    dtos.Add(DtoMapper.ToInventoryDto(lookup.Inventory, lookup.WarehousesContainingItem, Helpers.ParseFields(fields, nameof(Item))));
                }
            }
        }
        catch (AutoTryEnterException ex)
        {
            return new StatusCodeResult((int)HttpStatusCode.Locked);
        }
        catch (PTNoScenarioException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("CtpController.GetItems", e);
            return BadRequest($"An error occured when getting Items for CTP.");
        }

        int totalCount = dtos.Count();

        dtos = Helpers.PaginateDataResponse(limit, offset, dtos, dtos => dtos.Item.ExternalId).ToList();

        InventoryResponse inventoryResponse = new InventoryResponse()
        {
            Inventories = dtos,
            NextPageUrl = Helpers.GenerateNextPaginatedUrl(Request, limit, offset, totalCount),
            PreviousPageUrl = Helpers.GeneratePreviousPaginatedUrl(Request, limit, offset, totalCount),
            TotalCount = totalCount
        };

        return Ok(inventoryResponse);
    }

    /// <summary>
    /// Alerts the Server that data requests are incoming, so that this data can be cached.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <returns>200 if successful, 400 if no scenario exists for this id, and 423 if the call timed out attempted to get a lock on data to cache.</returns>
    [HttpPost]
    [Route("Items/Cache")]
    public IActionResult CacheItems([FromBody] ApsWebServiceScenarioRequest request)
    {
        try
        {
            m_itemCacheManager.LoadCache(request.ScenarioId);
            return Ok();
        }
        catch (AutoTryEnterException ex)
        {
            return new StatusCodeResult((int)HttpStatusCode.Locked);
        }  
        catch (PTNoScenarioException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Release cache data. This will require the next call to reload the cache.
    /// To avoid the need to check potentially locked data, this does not validate the existence of the scenario for the provided Id.
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("Items/ReleaseCache")]
    public IActionResult ReleaseCacheItems([FromBody] ApsWebServiceScenarioRequest request)
    {
        m_itemCacheManager.ClearCache(request.ScenarioId);

        return Ok();
    }

    /// <summary>
    /// Gets the returnable fields for the items entity.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("Items/fields")]
    public IActionResult GetItemFields()
    {
        return Ok(new Item().SelectableFields);
    }
}