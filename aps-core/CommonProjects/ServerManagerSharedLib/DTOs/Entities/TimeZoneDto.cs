namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// Holds select TimeZoneInfo data
    /// </summary>
    public class TimeZoneDto
    {
        public string DisplayName { get; set; }
        public string TimeZoneId { get; set; }

        public TimeZoneDto() {}

        public TimeZoneDto(string a_displayName, string a_timeZoneId)
        {
            DisplayName = a_displayName;
            TimeZoneId = a_timeZoneId;
        }
    }
}
