namespace WebAPI.Models.CTP
{
    public class CtpUpdateRequest
    {
        public string CompanyId { get; set; }
        public int PAId { get; set; }
        public int RequestID { get; set; }
        public DateTime SchedulePStart { get; set; }
        public DateTime ScheduleFinish { get; set; }
        public string Status { get; set; }
    }

    public class CtpUpdateResponse
    {
        public string UpdateStatus { get; set; }
        public string Detail { get; set; }
    }
}
