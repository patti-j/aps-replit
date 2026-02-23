namespace WebAPI.Models.CTP
{
    public class CtpResponse
    {
        public int RequestId { get; set; }
        public string ItemExternalId { get; set; }
        public string WarehouseExternalId { get; set; }
        public decimal RequiredQty { get; set; }
        public DateTime NeedDate { get; set; }
        public string RequiredPathId { get; set; }
        public int Priority { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledFinish { get; set; }
        public string Status { get; set; }
        public bool IsHot { get; set; }
        public string HotReason { get; set; }
    }
}
