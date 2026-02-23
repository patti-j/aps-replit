using WebAPI.Models.Integration;

using static WebAPI.Models.ServiceStatus;

namespace WebAPI.Models
{
	public class UserLoginResponse
	{
		public long UserId { get; set; }
		public string SessionToken { get; set; }
	}

	/// <summary>
	/// For calling GetScenario
	/// </summary>
	public class GetScenarioResponse : BaseResponse
	{
		public string ErrorMessage { get; set; }
		public List<ScenarioConfirmation> Confirmations { get; set; }
	}

	public class ScenarioConfirmation
	{
		public long ScenarioId { get; set; }
		public string ScenarioName { get; set; }
		public string ScenarioType { get; set; }
	}


	public class ImportResponse : BaseResponse
	{
	}

	public class BaseResponse
	{
		public EBaseResponseCode ResponseCode { get; set; }
		public bool Exception { get; set; } = false;
		public string FullExceptionText { get; set; }
	}

	public enum EBaseResponseCode
	{
		Success = 0,
		SuccessWithoutValidation = 1,
		InvalidDateFormat = 2,
		InvalidDatePast = 3,
		InvalidDateFuture = 4,
		InvalidUserCredentials = 5,
		InvalidUserPermissions = 6,
		NoServiceListening = 7,
		NoInstanceWithThatName = 8,
		ValidationTimeout = 9,
		NoServerManager = 10,
		InvalidScenarioName = 11,
		FailedToBroadcast = 12,
		ProcessingTimeout = 13,
		Failure = 14,
		InvalidScenarioId = 15,
		InvalidScenarioIdAndName = 16,
		InvalidLiveScenario = 17,
		NoIdObjects = 18,
		FailedInvalidResource = 19,
		FailedInvalidJob = 20,
		FailedInvalidMO = 21,
		FailedInvalidOp = 22,
		FailedInvalidActivity = 23,
		FailedUndoNoUserFound = 24,
		FailedUndoInvalidPermissions = 25,
		FailedUndoNoActions = 26,
		FailedUndoInvalidTransmissionNbr = 27,
		InvalidLicense = 28,
		FailedRequiredAdminAccess = 29,
	}

	/// <summary>
	/// A request from the WebApp to the Server Manager to perform an action.
	/// </summary>
    public class WebApiAction
    {
        public string TransactionId { get; set; }
        public string Action { get; set; }
        public string Parameters { get; set; }
    }

	/// <summary>
	/// A response from the Server Manager based on an action taken.
	/// </summary>
    public class WebApiActionUpdate
    {
        public string Parameters { get; set; }
        public string Status { get; set; }
    }

	/// <summary>
	/// A <see cref="WebApiActionUpdate"/> that originated from the WebApp, and has been returned from the Server Manager.
	/// </summary>
    public class WebApiActionFollowup : WebApiActionUpdate
    {
		public Guid TransactionId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class WebApiAgentStatusUpdate
    {
        public string ServerAuthToken { get; set; }
        public Version Version { get; set; }
		public List<ServerCertificateDTO> Certificates { get; set; }
		public string SystemId { get; set; }
    }

    /// <summary>
    /// A <see cref="WebApiActionUpdate"/> that originated from the Server Manager, and requires final steps to be performed in the WebApp.
    /// </summary>
    public class WebApiActionFromServer : WebApiActionUpdate
    {
		public string ActionType { get; set; }
    }

    public class WebApiStatusUpdate
    {
        public string ServerAuthToken { get; set; }
        public int Port { get; set; }
        public List<string>? AvailableVersions { get; set; }
        public Dictionary<string, ServiceStatus>? Statuses { get; set; }
    }

    public record PlanningAreaStatusList(List<PlanningAreaStatus> StatusList, int ServerId);
    public record PlanningAreaStatus(string PlanningAreaKey, EServiceState state);

    public class ServiceStatus
    {
        #region Status

        public int? UsersOnline { get; set; }
        public DateTime? LastLogon { get; set; }
        public DateTime? StartTime { get; set; } 
        public DateTime? LastActionTime { get; set; }

        public EServiceState State { get; set; }
        public ELicenseStatus  LicenseStatus { get; set; }

        public enum EServiceState
        {
            Stopped,
            Stopping,
            Idle,
            Started,
            Active,
            Starting,
            Unknown,
			NotFound
        }

        #endregion

        #region Identifiers
        // These are returned to verify the response comes from the expected instance
        // (ie, not an older version/ different instance using the an old instance's port).
        public Version? ProductVersion { get; set; }
        public string? InstanceName { get; set; }

        #endregion
    }
}
