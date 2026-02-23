using Microsoft.Extensions.Logging;

using ServerManagerWebLibrary.KeyService;
using ServerManagerWebLibrary.LicenseService;
using System.Runtime.Serialization;
using System.ServiceModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.Exceptions;
using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.Helpers
{
    [DataContract(Name = "LicenseHelper", Namespace = "http://www.planettogether.com")]
    public class LicenseHelper
    {
        private readonly InstanceInfo m_instanceInfo;
        private readonly ICommonLogger m_errorLogger;
        private readonly string m_serialCode;
        private readonly string m_keyFolder;
        private readonly string m_keyFile;
        const string c_primaryServiceUrl = "https://licenseserver.planettogether.net:45455";
        const string c_secondaryServiceUrl = "https://licenseserver2.planettogether.net:45455";
        const string c_keyServicePath = "/KeyService.svc";
        const string c_licenseServicePath = "/LicenseService.svc";

        private readonly string[] m_licenseSvcUrls;
        private readonly string[] m_keySvcUrls;

        private int m_currentKeySvcIndex = 0;
        private int m_currentLicenseSvcIndex = 0;

        private KeyServiceClient m_keyClient = null;
        private LicenseServiceClient m_licenseClient;

        private double m_pingInterval;
        private double m_timeoutInterval;

        public double PingInterval => m_pingInterval;
        public double TimeoutInterval => m_timeoutInterval;
        

        public LicenseHelper(ICommonLogger a_errorLogger,string a_instanceName, string a_instanceVersion, string a_serialCode, string a_keyFolder, string a_keyFile)
        {
            m_instanceInfo = GetInstanceInfo(a_instanceName, a_instanceVersion);
            m_errorLogger = a_errorLogger;
            m_serialCode = a_serialCode;
            m_keyFolder = a_keyFolder;
            m_keyFile = a_keyFile;
            m_licenseSvcUrls = new[] { c_primaryServiceUrl + c_licenseServicePath, c_secondaryServiceUrl + c_licenseServicePath };
            m_keySvcUrls = new[] { c_primaryServiceUrl + c_keyServicePath, c_secondaryServiceUrl + c_keyServicePath };
        }

        /// <summary>
        /// construct old, annoying instance info class
        /// </summary>
        /// <param name="a_integrationCode"></param>
        /// <returns></returns>
        private InstanceInfo GetInstanceInfo(string a_instanceName, string a_instanceVersion)
        {
            string integrationCode = "";

            InstanceInfo instanceInfo = new InstanceInfo
            {
                InstanceName = a_instanceName,
                APSVersionNumber = a_instanceVersion,
                IntegrationCode = integrationCode,
                SessionId = 0, //TODO: Figure out how this worked m_instanceLicenseInfo.SessionId,
            };
            try
            {
                instanceInfo.CpuId = MachineInfo.GetProcessorId();
            }
            catch (Exception)
            {
                instanceInfo.CpuId = "Error"; //It is possible that the cpuId cannot be calculated.
            }

            return instanceInfo;
        }

        private bool AllServiceUrlsAttempted()
        {
            return m_currentLicenseSvcIndex >= m_licenseSvcUrls.Length;
        }

        private bool AllKeyUrlsAttempted()
        {
            return m_currentKeySvcIndex >= m_keySvcUrls.Length;
        }

        /// <summary>
        /// throw LicenceServerException if maximum number of attempts has reached
        /// </summary>
        /// <returns>the URL in Key URL array matching the current index being used</returns>
        string GetKeySvcUrl()
        {
            string url = m_keySvcUrls[m_currentKeySvcIndex % m_keySvcUrls.Length];
            if (url == "")
            {
                SetupNextKeySvcUrl(true);
                return GetKeySvcUrl();
            }

            return url;
        }

        /// <summary>
        /// Call this if there is an exception connecting or if KeyClient returns a result with an Exception.
        /// It increases the index of current Key Svc URL and sets key client to null. Next call to GetKeyClient() will use the next url
        /// </summary>
        void SetupNextKeySvcUrl(bool a_loop)
        {
            if (!a_loop && m_currentKeySvcIndex + 1 >= m_keySvcUrls.Length * 2)
            {
                m_currentKeySvcIndex = 0;
                throw new LicenseServerException("No other key servers are available to try.");
            }

            m_currentKeySvcIndex++;
            m_keyClient = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>the URL in Key URL array matching the current index being used</returns>
        string GetLicenseSvcUrl()
        {
            string url = m_licenseSvcUrls[m_currentLicenseSvcIndex % m_licenseSvcUrls.Length];
            if (url == "")
            {
                SetupNextLicenseSvcUrl(true);
                return GetLicenseSvcUrl();
            }

            return url;
        }

        /// <summary>
        /// Call this if there is an exception connecting or if KeyClient returns a result with an Exception.
        /// It increases the index of current Key Svc URL and sets key client to null. Next call to GetKeyClient() will use the next url
        /// </summary>
        void SetupNextLicenseSvcUrl(bool a_loop)
        {
            if (!a_loop && m_currentLicenseSvcIndex + 1 >= m_licenseSvcUrls.Length * 2)
            {
                m_currentLicenseSvcIndex = 0;
                throw new LicenseServerException("No other License Servers are available to try.");
            }

            m_currentLicenseSvcIndex++;

            if (GetLicenseSvcUrl() == "")
            {
                SetupNextLicenseSvcUrl(a_loop);
            }

            m_licenseClient = null;
        }

        /// <summary>
        /// creates a binding to Key Service.
        /// </summary>
        /// <returns>Key Service Client object.</returns>
        KeyServiceClient GetKeyClient()
        {
            if (m_keyClient == null)
            {
                TimeSpan timeout = new TimeSpan(0, 5, 0);
                try
                {
                    BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                    binding.TransferMode = TransferMode.Streamed;
                    binding.OpenTimeout = binding.SendTimeout = binding.ReceiveTimeout = binding.CloseTimeout = timeout;
                    binding.MaxReceivedMessageSize = 2147483000;
                    m_keyClient = new KeyServiceClient(binding, new EndpointAddress(GetKeySvcUrl()));
                    m_keyClient.Open();
                    if (m_keyClient.State != CommunicationState.Opening && m_keyClient.State != CommunicationState.Opened)
                    {
                        SetupNextKeySvcUrl(false);
                        return GetKeyClient();
                    }
                }
                catch (LicenseServerException licEx)
                {
                    throw licEx;
                }
                catch (Exception)
                {
                    SetupNextKeySvcUrl(false);
                    return GetKeyClient();
                }
            }

            return m_keyClient;
        }

        /// <summary>
        /// creates a binding to license Service.
        /// </summary>
        /// <returns>Licese Service Client object.</returns>
        LicenseServiceClient GetLicenseClient()
        {
            if (m_licenseClient == null)
            {
                TimeSpan timeout = new TimeSpan(0, 5, 0);
                try
                {
                    BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                    binding.TransferMode = TransferMode.Streamed;
                    binding.OpenTimeout = binding.SendTimeout = binding.ReceiveTimeout = binding.CloseTimeout = timeout;
                    binding.MaxReceivedMessageSize = 2147483000;
                    m_licenseClient = new LicenseServiceClient(binding, new EndpointAddress((GetLicenseSvcUrl())));
                    m_licenseClient.Open();
                    if (m_licenseClient.State != CommunicationState.Opening && m_licenseClient.State != CommunicationState.Opened && AllServiceUrlsAttempted())
                    {
                        SetupNextLicenseSvcUrl(false);
                        return GetLicenseClient();
                    }
                }
                catch (LicenseServerException licEx)
                {
                    throw licEx;
                }
                catch
                {
                    SetupNextLicenseSvcUrl(false);
                    return GetLicenseClient();
                }
            }

            return m_licenseClient;
        }

        public bool UpdateKey()
        {
            string currentKeySerialCode = GetCurrentKeySerialCode(null);
            if (currentKeySerialCode == m_serialCode)
            {
                //If the serial code is different, then we don't need to check the date, we just get the key

                //Check the date
                GetLastKeyUpdateTimeResponse getLastKeyUpdateTimeResponse = GetLastKeyUpdateDateTime();
                DateTime latestKeyDate = getLastKeyUpdateTimeResponse.LastUpdatedDateTimeUTC;
                DateTime currentKeyDate = GetKeyModifiedDate(null); //TODO: Logging

                if (latestKeyDate <= currentKeyDate)
                {
                    return true;
                }
            }
            
            GetLatestKeyResponse key = GetLatestKey(new () { SerialCode = m_serialCode });
            if (key.ResultKey.Success)
            {
                UpdateKeyFile(key);
                return true;
            }

            ServerManagerException sme = GetKeySvcResponse(key.ResultKey, m_serialCode, GetInstanceInfoString());
            m_errorLogger.LogException(sme, new ScenarioExceptionInfo(), false, ELogClassification.Key);
            return false;
        }

        /// <summary>
        /// Backup any existing files (deleting old backups). Then creates key zip file and decompresses it.
        /// </summary>
        /// <param name="a_key"></param>
        internal void UpdateKeyFile(GetLatestKeyResponse a_key)
        {
            try
            {
                if (!Directory.Exists(m_keyFolder))
                {
                    throw new ServerManagerException("7005", new object[] { m_keyFolder }, false);
                }

                FileInfo[] files = new DirectoryInfo(m_keyFolder).GetFiles();
                // Delete old backup folder
                string keyBackupPath = Path.Combine(m_keyFolder, "backup");
                if (Directory.Exists(keyBackupPath) && files.Length > 0)
                {
                    DirectoryUtils.Delete(keyBackupPath);
                }

                // backup current key
                if (files.Length > 0)
                {
                    Directory.CreateDirectory(keyBackupPath);
                    foreach (FileInfo file in files)
                    {
                        file.CopyTo(Path.Combine(keyBackupPath, file.Name));
                        file.Delete();
                    }
                }

                ZipUtils.Extract(a_key.KeyStream, m_keyFolder);
            }
            catch (Exception ex)
            {
                throw new ServerManagerException("7006", ex, null, false);
            }
        }

        /// <summary>
        /// Gets the LastKeyFieldModification field. If key file doesn't exist, it returns DateTime.Min 
        /// to indicate key doesn't exist yet.
        /// </summary>
        /// <returns>last modify date on key or min DateTime if there's no key</returns>
        internal DateTime GetKeyModifiedDate(ILogger a_logger)
        {
            DateTime modifiedDate = DateTime.MinValue;
            LicenseKeyJson licenseKey = GetLicenseKey(a_logger);

            if (licenseKey != null)
            {
                modifiedDate = licenseKey.LastKeyFieldModificationUTC;
            }

            return modifiedDate;
        }

        internal string GetCurrentKeySerialCode(ILogger a_logger)
        {
            string serialCode = string.Empty;
            LicenseKeyJson licenseKey = GetLicenseKey(a_logger);

            if (licenseKey != null)
            {
                serialCode = licenseKey.SerialCode;
            }

            return serialCode;
        }

        /// <summary>
        /// returns XmlDocument for instance's Key File. If file doesn't exist, it logs the issue and returns empty XML file.
        /// </summary>
        /// <returns></returns>
        LicenseKeyJson GetLicenseKey(ILogger a_logger)
        {
            LicenseKeyJson licenseKeyJson = null;

            try
            {
                if (File.Exists(m_keyFile))
                {
                    licenseKeyJson = LicenseKeyJson.DeserializeToObject(m_keyFile);
                }
            }
            catch (Exception ex)
            {
                a_logger.LogError(new EventId(4105), ex, "Failed to get license key", m_keyFile);
            }

            return licenseKeyJson;
        }

        private string GetInstanceInfoString()
        {
            return string.Format("Instance Name '{0}' Version '{1}' SessionId '{2}'.",
                m_instanceInfo.InstanceName, m_instanceInfo.APSVersionNumber, m_instanceInfo.SessionId);
        }

        private GetLastKeyUpdateTimeResponse GetLastKeyUpdateDateTime()
        {
            GetLastKeyUpdateDateTimeRequest request = new GetLastKeyUpdateDateTimeRequest
            {
                ExtensionData = null,
                SerialCode = m_serialCode,
            };


            GetLastKeyUpdateTimeResponse resp;

            try
            {
                resp = GetKeyClient().GetLastKeyUpdateDateTime(request);
                if (resp.ResultKey.Exception || resp.ResultKey.FailedToUpdateDatabase)
                {
                    SetupNextKeySvcUrl(false);
                    resp = GetKeyClient().GetLastKeyUpdateDateTime(request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception)
            {
                SetupNextKeySvcUrl(false);
                resp = GetKeyClient().GetLastKeyUpdateDateTime(request);
            }

            return resp;
        }

        public GetLatestKeyResponse GetLatestKey(GetLatestKeyRequest a_request)
        {
            GetLatestKeyResponse resp;
            try
            {
                resp = GetKeyClient().GetLatestKey(a_request);
                if (resp.ResultKey.Exception || resp.ResultKey.FailedToUpdateDatabase)
                {
                    SetupNextKeySvcUrl(false);
                    resp = GetLatestKey(a_request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception)
            {
                SetupNextKeySvcUrl(false);
                resp = GetLatestKey(a_request);
            }

            return resp;
        }

        public ConvertKeyResponse ConvertToSystemIdKey(ConvertKeyRequest a_request)
        {
            ConvertKeyResponse resp;
            try
            {
                resp = GetKeyClient().ConvertToSystemIdKey(a_request);

                if (resp.ResultKey.Exception || resp.ResultKey.FailedToUpdateDatabase)
                {
                    SetupNextKeySvcUrl(false);
                    resp = ConvertToSystemIdKey(a_request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception)
            {
                SetupNextKeySvcUrl(false);
                resp = ConvertToSystemIdKey(a_request);
            }

            return resp;
        }

        public UpdateLicenseIdResponse UpdateLicenseId(UpdateLicenseIdRequest a_request)
        {
            UpdateLicenseIdResponse resp;

            try
            {
                resp = GetKeyClient().UpdateLicenseId(a_request);
                if (resp.ResultKey.Exception || resp.ResultKey.FailedToUpdateDatabase)
                {
                    SetupNextKeySvcUrl(false);
                    resp = UpdateLicenseId(a_request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception)
            {
                SetupNextKeySvcUrl(false);
                resp = UpdateLicenseId(a_request);
            }

            return resp;
        }

        public GetCompanyLicenseDataResponse GetCompanyLicenseData(GetCompanyLicenseDataRequest a_request)
        {
            GetCompanyLicenseDataResponse resp;

            try
            {
                resp = GetKeyClient().GetCompanyLicenseData(a_request);
                if (resp.ResultKey.Exception || resp.ResultKey.FailedToUpdateDatabase)
                {
                    SetupNextKeySvcUrl(false);
                    resp = GetCompanyLicenseData(a_request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception)
            {
                SetupNextKeySvcUrl(false);
                resp = GetCompanyLicenseData(a_request);
            }

            return resp;
        }

        //TODO: what was the purpose of this? doesnt even seem used in the old server manager
        public GetLicenseInfoResponse GetLicenseInfo(GetLicenseInfoRequest a_request)
        {
            GetLicenseInfoResponse resp;

            try
            {
                resp = GetLicenseClient().GetLicenseInfo(a_request);
                if (resp.ResultLicenseService.Exception || resp.ResultLicenseService.FailedToUpdateDatabase)
                {
                    SetupNextLicenseSvcUrl(false);
                    resp = GetLicenseInfo(a_request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception)
            {
                SetupNextLicenseSvcUrl(false);
                resp = GetLicenseInfo(a_request);
            }

            return resp;
        }

        public bool Ping()
        {
            PingRequest request = new PingRequest { SerialCode = m_serialCode, SessionId = m_instanceInfo.SessionId };

            PingResponse resp = null;

            try
            {
                resp = GetLicenseClient().Ping(request);
                if (resp.ResultLicenseService.Exception || resp.ResultLicenseService.FailedToUpdateDatabase)
                {
                    SetupNextLicenseSvcUrl(true);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception err)
            {
                m_errorLogger.LogException(err, null, false, ELogClassification.Key);
                SetupNextLicenseSvcUrl(true);
            }

            if (resp == null)
            {
                ServerManagerException smEx = new ServerManagerException("4101", new object[] { GetInstanceInfoString() }, false);
                m_errorLogger.LogException(smEx, null, false, ELogClassification.Key);
                return false;
            }
            
            if (!resp.ResultLicenseService.Success)
            {
                PTException smEx = GetExceptionToLog(resp.ResultLicenseService);
                m_errorLogger.LogException(smEx, null, false, ELogClassification.Key);
            }
                
            return resp.ResultLicenseService.Success;
        }

        public bool LockLicense()
        {
            LockLicenseRequest request = new LockLicenseRequest() { SerialCode = m_serialCode, InstanceInfo = m_instanceInfo };
            LockLicenseResponse response = Lock(request);
            if (!response.ResultLicenseService.Success)
            {
                m_errorLogger.LogException(GetExceptionToLog(response.ResultLicenseService), null, false, ELogClassification.Key);
                return false;
            }
            
            return true;
        }
        
        private LockLicenseResponse Lock(LockLicenseRequest a_request)
        {
            LockLicenseResponse resp;
            try
            {
                resp = GetLicenseClient().Lock(a_request);
                if (resp.ResultLicenseService.Exception || resp.ResultLicenseService.FailedToUpdateDatabase)
                {
                    SetupNextLicenseSvcUrl(false);
                    resp = Lock(a_request);
                }
            }
            catch (LicenseServerException licEx)
            {
                throw licEx;
            }
            catch (Exception lastFailureException)
            {
                try
                {
                    SetupNextLicenseSvcUrl(false);
                    resp = Lock(a_request);
                }
                catch (LicenseServerException e)
                {
                    //Instead of throwing error for no more license servers to try, report the last connection error instead.
                    throw lastFailureException;
                }
            }

            m_pingInterval = resp.ServerSpecifiedPingInterval.TotalMilliseconds;
            m_timeoutInterval = resp.ServerTimeoutTimeSpan.TotalMilliseconds;
            m_instanceInfo.SessionId = resp.SessionIdReturned;

            return resp;
        }

        public void Release()
        {
            //Store and return or required variables to use the request/response in this class.
            //Log stuff
            ReleaseLicenseRequest request = new ReleaseLicenseRequest
            {
                ExtensionData = null,
                SerialCode = m_serialCode,
                SessionId = m_instanceInfo.SessionId,
                ReleaseReason = new ReleaseReasonToSend
                {
                    ExtensionData = null,
                    None = false,
                    ClientReleased = true,
                    Administrator = false,
                    Support = false
                }
            };
            
            ReleaseLicenseResponse resp;

            try
            {
                resp = GetLicenseClient().Release(request);
                if (resp.ResultLicenseService.Exception || resp.ResultLicenseService.FailedToUpdateDatabase)
                {
                    SetupNextLicenseSvcUrl(false);
                    resp = GetLicenseClient().Release(request);
                }
            }
            catch (LicenseServerException ex)
            {
                throw ex;
            }
            catch
            {
                SetupNextLicenseSvcUrl(false);
                resp = GetLicenseClient().Release(request);
            }

            if (!resp.ResultLicenseService.Success)
            {
                PTException smEx = GetExceptionToLog(resp.ResultLicenseService);
                m_errorLogger.LogException(smEx, null, false, ELogClassification.Key);
            }
        }

        /// <summary>
        /// Handles the logging unsuccessful responses from Key Service.
        /// </summary>
        /// <param name="a_result">result base</param>
        /// <param name="a_serialCode"></param>
        /// <param name="a_instanceInfoString"></param>
        public ServerManagerException GetKeySvcResponse(global::ServerManagerWebLibrary.KeyService.KeyResultBase a_result, string a_serialCode, string a_instanceInfoString)
        {
            ServerManagerException smEx;

            if (a_result.Exception)
            {
                smEx = new ServerManagerException("4095", new object[] { a_serialCode, a_instanceInfoString }, false);
            }
            else if (a_result.FailedToUpdateDatabase)
            {
                smEx = new ServerManagerException("4096", new object[] { a_serialCode, a_instanceInfoString }, false);
            }
            else if (a_result.InvalidSerialCode)
            {
                smEx = new ServerManagerException("4092", new object[] { a_serialCode, a_instanceInfoString }, false);
            }
            else if (a_result.InvalidSystemIdFormat)
            {
                smEx = new ServerManagerException("4106", new object[] { a_instanceInfoString }, false);
            }
            else if (a_result.LicenseIsSystemIdType || a_result.LicenseIsWindowsProductIdType)
            {
                smEx = new ServerManagerException("4094", new object[] { a_serialCode, a_instanceInfoString }, false);
            }
            else
            {
                smEx = new ServerManagerException("4107", new object[] { a_serialCode, a_instanceInfoString }, false);
            }

            return smEx;
        }

        /// <summary>
        /// Handles the logging unsuccessful responses from license service Service.
        /// </summary>
        /// <param name="a_result">result base</param>
        private PTException GetExceptionToLog(LicenseServiceResultBase a_result)
        {
            PTException smEx;

            if (a_result.KeyExpired)
            {
                smEx = new PTException("4090", new object[] { m_instanceInfo.InstanceName, m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.MaintenanceExpired)
            {
                smEx = new PTException("4091", new object[] { m_instanceInfo.InstanceName, m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.InvalidSerialCode)
            {
                smEx = new PTException("4092", new object[] { m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.LicenseInUse)
            {
                smEx = new PTException("4093", new object[] { m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.LicenseIsSystemIdType
                     || a_result.LicenseIsWindowsProductIdType)
            {
                smEx = new PTException("4094", new object[] { m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.Exception)
            {
                smEx = new PTException("4095", new object[] { m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.FailedToUpdateDatabase)
            {
                smEx = new PTException("4096", new object[] { m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.InvalidReleaseReason)
            {
                smEx = new PTException("4097", new object[] { m_serialCode, GetInstanceInfoString() }, false);
            }
            else if (a_result.SessionDoesntExist)
            {
                smEx = new PTException("4098", new object[] { m_serialCode, m_instanceInfo.SessionId, GetInstanceInfoString() }, false);
            }
            else if (a_result.SessionTimedOut)
            {
                smEx = new PTException("4099", new object[] { m_serialCode, m_instanceInfo.SessionId, GetInstanceInfoString() }, false);
            }
            else if (a_result.InvalidSessionID)
            {
                smEx = new PTException("4100", new object[] { m_serialCode, m_instanceInfo.SessionId, GetInstanceInfoString() }, false);
            }
            else
            {
                smEx = new PTException("4101", new object[] { GetInstanceInfoString() }, false);
            }

            return smEx;
        }

        [DataContract(Name = "InstanceLicenseStatus")]
        public enum EInstanceLicenseStatus
        {
            /// <summary>
            /// Indicates no license status information due to instance being stopped.
            /// </summary>
            [EnumMember]
            Stopped = 0,

            /// <summary>
            /// Instance is in ReadOnly mode and License Service is being used.
            /// </summary>
            [EnumMember]
            LicenseServiceReadOnly = 1,

            /// <summary>
            /// Instance is in Readonly mode and License Serivce is not in used.
            /// </summary>
            [EnumMember]
            ReadOnly = 2,

            /// <summary>
            /// Instance is not in Read-only mode.
            /// </summary>
            [EnumMember]
            Ok = 3,

            /// <summary>
            /// Instance is old and status information is not available
            /// </summary>
            [EnumMember]
            NotAvaiable = 4
        }
    }
}
