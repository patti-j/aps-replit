using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppImportService.Models
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
}
