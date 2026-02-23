using Microsoft.AspNetCore.Http;
using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APIDefinitions.RequestsAndResponses.DataDtos;
using PT.APSCommon;
using PT.ERPTransmissions;
using PT.PackageDefinitions.Settings;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SchedulerDefinitions.UserSettingTemplates;

namespace PT.PlanetTogetherAPI;

public class Helpers
{
    #region Validation
    internal static BaseId ValidateUser(ApsWebServiceRequestBase a_baseRequest, IEnumerable<UserDefs.EPermissions> a_requiredPermissions)
    {
        return ValidateUser(a_baseRequest.UserName, a_baseRequest.Password, a_requiredPermissions);
    }

    internal static BaseId ValidateUser(ApsWebServiceRequestBase a_baseRequest, bool a_adminRequired, List<string> a_permissionKeys)
    {
        try
        {
            using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, 5000))
            {
                User user = um.GetUserByName(a_baseRequest.UserName);
                if (user == null)
                {
                    throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidUserCredentials);
                }

                if (!user.Active)
                {
                    throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidUserCredentials);
                }

                UserPermissionSet userPermissionSet = um.GetUserPermissionSetById(user.UserPermissionSetId);
                UserPermissionValidator validator = userPermissionSet.GetPermissionsValidator();

                if (a_adminRequired && !validator.AdministerUsers)
                {
                    throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedRequiredAdminAccess);
                }

                foreach (string permissionKey in a_permissionKeys)
                {
                    if (!validator.ValidatePermission(permissionKey))
                    {
                        throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidUserPermissions);
                    }
                }

                return user.Id;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    internal static BaseId ValidateUser(string a_name, string a_password, IEnumerable<UserDefs.EPermissions> a_requiredPermissions)
    {
        try
        {
            IEnumerable<UserDefs.EPermissions> requiredPermissions = a_requiredPermissions as UserDefs.EPermissions[] ?? a_requiredPermissions.ToArray();
            if (requiredPermissions.Contains(UserDefs.EPermissions.RunInterface) && !PTSystem.LicenseKey.AllowImportLicense)
            {
                throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidLicense);
            }

            using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, 5000))
            {
                User user = um.GetUserByName(a_name);
                if (user == null)
                {
                    throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidUserCredentials);
                }

                if (!user.Active)
                {
                    throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidUserCredentials);
                }

                //TODO: V12 Permissions

                foreach (UserDefs.EPermissions permission in requiredPermissions)
                {
                    if (!ValidateUserPermission(permission, user))
                    {
                        throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidUserPermissions);
                    }
                }

                return user.Id;
            }
        }
        catch (AutoTryEnterException e)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.ValidationTimeout);
        }
    }

    private static bool ValidateUserPermission(UserDefs.EPermissions a_permission, User a_user)
    {
        //TODO: V12 Permissions
        //bool canReserveCtp = m_scenarioInfo.GetCurrentUserEditAccess(UserPermissionKeys.ReserveCTP) == EUserAccess.Edit;


        return true;
    }

    private static bool ValidateUserPermission(UserDefs.EPermissions a_permission, BaseId a_userId)
    {
        User user = null;
        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, 5000))
        {
            user = um.GetById(a_userId);
        }

        //TODO: V12 Permissions
        //if (user.UserPermissions.GetAccess(a_permission) < UserDefs.EPermissionAccessLevels.Live) 
        //{
        //    return false;
        //}

        return true;
    }

    internal static User CheckForADTemplate()
    {
        ApiLogger al = new ("ValidateKeyAndCheckForADTemplate", ControllerProperties.ApiDiagnosticsOn, TimeSpan.Zero);
        al.LogEnter();

        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, 5000))
        {
            User foundUser = null;
            for (int i = 0; i < um.Count; i++)
            {
                User u = um[i];
                if (u.Active && string.Compare(u.Name, "AdTemplate", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    foundUser = u;
                }
            }

            if (foundUser != null)
            {
                return foundUser;
            }
        }

        throw new Exception("AdTemplate user not found.  Please add one to sync an AD group");
    }
    #endregion

    #region Utility
    internal static UserT.User CopyFieldsFromUser(User a_fromUser)
    {
        string userPermissionGroup = "";
        string plantPermissionGroup = "";
        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, 1000))
        {
            userPermissionGroup = um.GetUserPermissionSetById(a_fromUser.UserPermissionSetId).Name;
            plantPermissionGroup = um.GetPlantPermissionSetById(a_fromUser.PlantPermissionsId).Name;
        }

        UserT.User user = new ()
        {
            TransmissionReceptionType = a_fromUser.TransmissionReceptionType,
            CompressionType = a_fromUser.CompressionType,
            UnlockUser = true,
            DisplayLanguage = a_fromUser.DisplayLanguage,
            AdvanceClockReportProgress = a_fromUser.ClockAdvanceAutoProgress,
            AdvanceClockFinishActivities = a_fromUser.ClockAdvanceAutoFinish,
            PlantPermissionGroup = userPermissionGroup,
            UserPermissionGroup = plantPermissionGroup //TODO: V12 verify this
        };

        user.Preferences = a_fromUser.UserPreferenceInfo;

        return user;
    }

    #endregion

    #region Pagination

    public static IEnumerable<T> PaginateDataResponse<T>(int a_limit, int a_offset, IEnumerable<T> a_itemDtos)
    where T : IDataDto
    {
        Func<T, string> keySelector = itemDto => itemDto.ExternalId;
        a_itemDtos = a_itemDtos.OrderBy(keySelector);

        if (a_offset > 0)
        {
            a_itemDtos = a_itemDtos.Skip(a_offset);
        }

        if (a_limit > 0)
        {
            a_itemDtos = a_itemDtos.Take(a_limit);
        }

        return a_itemDtos;
    }

    public static IEnumerable<T> PaginateDataResponse<T>(int a_limit, int a_offset, IEnumerable<T> a_itemDtos, Func<T, object> a_orderFunc)
    where T : class
    {
        a_itemDtos = a_itemDtos.OrderBy(a_orderFunc);

        if (a_offset > 0)
        {
            a_itemDtos = a_itemDtos.Skip(a_offset);
        }

        if (a_limit > 0)
        {
            a_itemDtos = a_itemDtos.Take(a_limit);
        }

        return a_itemDtos;
    }

    public static string GenerateNextPaginatedUrl(HttpRequest a_request, int a_limit, int a_offset, int a_totalCount)
    {
        int nextOffset = a_offset + a_limit;
        string nextPageUrl = null;

        if (nextOffset < a_totalCount)
        {
            //Modify current params, replacing offset value
            Dictionary<string, string> queryParams = a_request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            queryParams["offset"] = nextOffset.ToString();
            string updatedQueryParams = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            nextPageUrl =  $"{a_request.Scheme}://{a_request.Host}{a_request.Path}?{updatedQueryParams}";
        }

        return nextPageUrl;
    }

    public static string GeneratePreviousPaginatedUrl(HttpRequest a_request, int a_limit, int a_offset, int a_totalCount)
    {
        var previousOffset = Math.Max(a_offset - a_limit, 0);
        string previousPageUrl = null;

        if (a_offset > 0)
        {
            //Modify current params, replacing offset value
            Dictionary<string, string> queryParams = a_request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            queryParams["offset"] = previousOffset.ToString();
            string updatedQueryParams = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            previousPageUrl = $"{a_request.Scheme}://{a_request.Host}{a_request.Path}?{updatedQueryParams}";
        }

        return previousPageUrl;
    }

    #endregion

    #region Request Queries

    /// <summary>
    /// Checks whether the provided record matches the query substring requested.
    /// TODO: This currently checks the Name column - extend this if we want to query on other props.
    /// </summary>
    /// <param name="a_query"></param>
    /// <param name="a_record"></param>
    /// <returns></returns>
    public static bool IsItemInRequestQuery(string a_query, BaseObject a_record)
    {
        return a_query == null || a_record.Name.Contains(a_query, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region Field Selection

    internal static IncludedQueryFields ParseFields(string a_fieldsParam, string a_entityName)
    {
        HashSet<string> fieldList = new HashSet<string>();

        if (!string.IsNullOrWhiteSpace(a_fieldsParam))
        {
            fieldList = a_fieldsParam.Split(',').Select(f => f.Trim()).ToHashSet();
        }

        return new IncludedQueryFields(fieldList, a_entityName);
    }

    #endregion

    public static bool IsApiKeyValid(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("ApiKey", out var providedApiKey))
            return false;

        return string.Equals(providedApiKey, PTSystem.ApiKey, StringComparison.Ordinal);
    }
    public static (bool IsValid, BaseId Instigator, ApsWebServiceResponseBase ErrorResponse) TryGetInstigatorFromHeaders(HttpRequest request)
    {
        if (!IsApiKeyValid(request))
        {
            return (false, default, new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidUserCredentials, "Invalid API Key"));
        }

        long companyId = long.MinValue;
        if (request.Headers.TryGetValue("CompanyId", out var companyIdHeader))
        {
            long.TryParse(companyIdHeader, out companyId);
        }
        return (true, new BaseId(companyId), null);
    }


}

internal class IncludedQueryFields
{
    internal IncludedQueryFields(HashSet<string> a_fieldList, string a_entityName)
    {
        // Prepend simple field names with the entity name
        string entityPrefix = $"{a_entityName}.";
        FieldList = a_fieldList
            .Select(field => field.Contains(".") ? 
                field : 
                field.Insert(0, entityPrefix)
            ).ToHashSet<string>();

        EntityName = a_entityName;
    }

    internal HashSet<string> FieldList { get; set; }
    internal string EntityName { get; set; }

    /// <summary>
    /// Determines whether an optional field should be loaded in for the DTO based on the request's params
    /// </summary>
    /// <param name="a_fieldName">The field which is being tested for inclusion.</param>
    /// <param name="a_isEntityField">If true, used to check if any reference to the entity exists (ie if Warehouses is a nested entity field,
    /// and we want to determine if any of its sub-properties should be further checked). Defaults to false; </param>
    /// <returns></returns>
    internal bool ShouldLoad(string a_fieldName, bool a_isEntityField = false)
    {
        return FieldList == null || FieldList.Count == 0 || // if nothing provided, assume all properties are coming in
               FieldList.Any(fieldToInclude =>
                   fieldToInclude.Equals($"{EntityName}.{a_fieldName}", StringComparison.InvariantCultureIgnoreCase) || // exact prop match
                   a_isEntityField && fieldToInclude.StartsWith($"{a_fieldName}.", StringComparison.InvariantCultureIgnoreCase)); // any nested ref, ie Warehouses.Description
    }

    /// <summary>
    /// Gets the subset of fields to include that are for this nested entity, and returns a new <see cref="IncludedQueryFields"/> ready for use for it.
    /// </summary>
    /// <param name="a_nestedEntityName"></param>
    /// <returns></returns>
    internal IncludedQueryFields GetIncludedFieldsForNestedEntity(string a_nestedEntityName)
    {
        HashSet<string> fieldsForNestedEntity = FieldList
            .Where(field => field.StartsWith($"{a_nestedEntityName}.", StringComparison.InvariantCultureIgnoreCase))
            .ToHashSet();

        return new IncludedQueryFields(fieldsForNestedEntity, a_nestedEntityName);
    }
}
  