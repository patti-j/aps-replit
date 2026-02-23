namespace WebAPI.Models
{
	public class BasicLoginRequest
	{
		public string UserName { get; set; }
		public byte[] PasswordHash { get; set; }
	}

	public class GetScenarioRequest : BaseRequest
	{
		public string ScenarioType { get; set; }
		public double TimeoutMinutes { get; set; }
		public bool GetBlackBoxScenario { get; set; }
	}

	public class ImportRequest : BaseScenarioRequest
	{
		public string ScenarioName { get; set; }
		public bool CreateScenarioIfNew { get; set; }
	}

	public class BaseScenarioRequest : BaseRequest
	{
		public long ScenarioId { get; set; }
	}

    public class CreateWebAppUsersRequest
    {
       public List<CreateWebAppUserRequest> Users { get; set; }
    }

    public class CreateWebAppUserRequest
    {
        public string UserType { get; set; } // "Web" or "Service"
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> PermissionGroupNamesList { get; set; }
        public string? DisplayLanguage { get; set; }
        public string? CompressionType { get; set; }
    }

    public class BaseRequest
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public TimeSpan TimeoutDuration { get; set; } = TimeSpan.FromSeconds(30);
	}
}
