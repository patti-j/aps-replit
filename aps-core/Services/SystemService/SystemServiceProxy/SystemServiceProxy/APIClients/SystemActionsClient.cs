using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions.RequestsAndResponses;
using PT.Common.Encryption;
using PT.Common.Http;
using PT.Common.Http.Json;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.PackageDefinitions.PackageInterfaces;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.SystemServiceDefinitions;
using PT.SystemServiceDefinitions.Headers;

using InstanceKey = PT.Common.Http.InstanceKey;

namespace PT.SystemServiceProxy.APIClients;

/// <summary>
/// This is the we wrapper for the User Clients.
/// This will be used by the server and external clients
/// The Client Sessions will use this to broadcast and receive new actions waiting on the server session
/// </summary>
public class SystemActionsClient : PTSerializingHttpClient, ISystemServiceClient
{
    public SystemActionsClient(string a_instanceName, string a_instanceVersion, string a_systemServiceUri, string a_controller = "SystemService") : base($"api/{a_controller}/", new InstanceKey(a_instanceName, a_instanceVersion), a_systemServiceUri) { }

    public void Authenticate(string a_sessionToken)
    {
        base.Authenticate(new AuthenticationHeaderValue("SessionTokenScheme", a_sessionToken));
    }

    public byte[] Handshake(string a_publicKey)
    {
        byte[] response = MakeGetRequest<byte[]>("Handshake", "api/Login", new GetParam { Name = "a_publicKey", Value = a_publicKey });

        return response;
    }

    public LoggedInInstanceInfoAPI GetLoggedInInstanceInfo() //todo
    {
        LoggedInInstanceInfoAPI response = MakePostRequest<LoggedInInstanceInfoAPI>("getLoggedInInstanceInfo");
        return response;
    }

    public string GetSystemServiceURL()
    {
        string response = MakePostRequest<string>("GetSystemServiceURL");

        return response;
    }

    public byte[] GetLoggedInInstanceData()
    {
        LoggedInInstanceData response = MakePostRequest<LoggedInInstanceData>("GetLoggedInInstanceData");

        return response.EncryptedInstanceData;
    }

    public string GetLicenseKeyForInstance()
    {
        string response = MakePostRequest<string>("GetLicenseKeyForInstance");

        return response;
    }

    public StartupValsAdapter GetStartupVals()
    {
        StartupValsAdapter startupValsAdapter = MakePostRequest<StartupValsAdapter>("GetStartupVals");
        return startupValsAdapter;
    }

    public UserLoginResponse Login(string a_userName, string a_password, LoginType a_loginType, byte[] a_symmetricKey)
    {
        if (a_loginType == LoginType.User)
        {
            byte[] passwordHash = StringHasher.Hash(a_password, a_symmetricKey);
            BasicLoginRequest request = new()
            {
                UserName = a_userName,
                PasswordHash = passwordHash
            };
            UserLoginResponse response = MakePostRequest<UserLoginResponse>("LoginAsUser", request, "api/Login");
            Authenticate(response.SessionToken);

            return response;
        }

        if (a_loginType == LoginType.JWT)
        {
            TokenLoginRequest request = new()
            {
                Token = a_userName
            };
            UserLoginResponse response = MakePostRequest<UserLoginResponse>("LoginWithToken", request, "api/Login");
            Authenticate(response.SessionToken);

            return response;
        }

        //TODO: errors
        return null;
    }

    public SoftwareProductVersion GetServerProductVersion(string a_sessionToken)
    {
        SoftwareProductVersion response = MakeGetRequest<SoftwareProductVersion>("GetServerSoftwareVersion");

        return response;
    }

    public byte[] GetSystem(string a_sessionToken)
    {
        byte[] response = MakeGetRequest<ByteResponse>("GetScenarios").Content;

        if (response.Length == 0)
        {
            //if (PTSystem.Server)
            //{
            //    SimpleExceptionLogger.LogException(SimpleExceptionLogger.LOGIN_ERROR_LOG_NAME, new SystemController.LoginException("2979", new object[] { response.ExceptionMessage }), SimpleExceptionLogger.LOGIN_ERROR_TITLE);
            //    throw new SystemController.LoginException("2979", new object[] { response.ExceptionMessage });
            //}
        }
        //TODO: Add a progress bar, this may go in the GetSystem API call

        //TODO: Timing
        //timeToDownloadSystemFromServiceTicks.Stop();

        //if (response.SystemBytes.Length == 0)
        //{
        //if (!string.IsNullOrEmpty(response.ExceptionMessage))
        //{
        //    SimpleExceptionLogger.LogException(SimpleExceptionLogger.LOGIN_ERROR_LOG_NAME, new SystemController.LoginException("2967", new object[] { response.ExceptionMessage }), SimpleExceptionLogger.LOGIN_ERROR_TITLE);
        //    throw new PTException("4449", new object[] { "-95" });
        //}

        //SimpleExceptionLogger.LogException(SimpleExceptionLogger.LOGIN_ERROR_LOG_NAME, new SystemController.LoginException("2968"), SimpleExceptionLogger.LOGIN_ERROR_TITLE);
        //throw new PTException("4449", new object[] { "-100" });
        //}

        return response;
    }

    public IEnumerable<AssemblyPackageInfo> GetPackagesOnServer()
    {
        AssemblyPackageInfo[] response = MakePostRequest<AssemblyPackageInfo[]>("GetPackagesOnServer");

        return response;
    }

    public PackedAssembly GetPackedPackageAssembly(AssemblyPackageInfo packageVersionToGet)
    {
        PackedAssembly response = MakePostRequest<PackedAssembly>("GetPackedPackageAssembly", packageVersionToGet);

        return response;
    }

    public void Broadcast(byte[] a_transmissionByteArray, string a_sessionToken)
    {
        MakePostRequest<BoolResponse>("SendTransmission", a_transmissionByteArray);
    }

    public void LogOff(string a_sessionToken)
    {
        MakePostRequest<BoolResponse>("LogOff");
    }

    public GetTransmissionResponse GetNextTransmissionData(GetTransmissionRequest a_request)
    {
        GetTransmissionResponse response = MakePostRequest<GetTransmissionResponse>("RetrieveNextAction", a_request);

        return response;
    }

    public PerformImportResult RunImport(PerformImportRequest a_request)
    {
        return MakePostRequest<PerformImportResponse>("RunERPImport", a_request).Content;
    }

    public DataTableJson GetBrowseTable(string a_commandText)
    {
        return MakeGetRequest<DataTableJson>("GetBrowseTable", null, new GetParam { Name = "a_commandText", Value = a_commandText });
    }

    public ImportSettings GetImportSettings()
    {
        return MakeGetRequest<ImportSettings>("GetImportSettings");
    }
    
    public void SaveImportSettings(ImportSettings a_settings)
    {
        MakePostRequest<BoolResponse>("SaveImportSettings", a_settings);
    }

    public void SetTimeout(TimeSpan a_timeout)
    {
        // Once an HttpClient has been used, the timeout cannot be set. So, this method initializes a new client with existing values.
        AuthenticationHeaderValue existingAuthenticationHeaderValue = m_httpClient.DefaultRequestHeaders.Authorization;

        InitializeHttpClient(m_defaultController, m_httpClient.BaseAddress.ToString());
        m_httpClient.Timeout = a_timeout;
        m_httpClient.DefaultRequestHeaders.Authorization = existingAuthenticationHeaderValue;
    }
}