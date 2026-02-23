using PT.Common.Http;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.ServerManagerSharedLib.DTOs.Responses;

using InstanceKey = PT.Common.Http.InstanceKey;

namespace PT.ServerManagerAPIProxy.APIClients;

[Obsolete("Remove once ServerManager has been removed")]
public class InstanceActionsClient : PTHttpClient
{
    public InstanceActionsClient(string a_instanceName, string a_softwareVersion, string a_serverAgentUri) : base("Instances/", new InstanceKey(a_instanceName, a_softwareVersion), a_serverAgentUri)
    {
        //TODO: Create this when it's used. Also it appears to connect to the licenseserver.planettogether.net address just be creating this object and logging in, even though the class is never used.
        //m_licenseHelper = new LicenseHelper(c_licenseServicePath, c_keyServicePath);

    }

    public GenericResponse NotifyLicenseStatus(bool a_isReadonly)
    {
        SaveInstanceNotifyLicenseStatusRequest dto = new ();
        dto.ReadOnly = a_isReadonly;
        dto.InstanceName = m_instanceKey.InstanceName;
        dto.SoftwareVersion = m_instanceKey.SoftwareVersion;

        GenericResponse response = MakePostRequest<GenericResponse>("getNotifyLicenseStatus", dto);

        return response;
    }
}