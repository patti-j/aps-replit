namespace WebAPI.Models.CTP
{
    public class CtpRequest
    {
        public string CompanyId { get; set; }
        public int PAId { get; set; }
    }
    public class Ctp : BaseEntity
    {
        public int CompanyId { get; set; }
        public int JobTemplateId { get; set; }
        public int PADetailsId { get; set; }
        private int _requestId;
        public int RequestId { get; set; }
        public string ItemExternalId { get; set; }
        public string WarehouseExternalId { get; set; }
        public decimal RequiredQty { get; set; }
        public DateTime NeedDate { get; set; }
        public string RequiredPathId { get; set; }
        public int Priority { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledFinish { get; set; }
        public string ScheduledPath { get; set; }
        public bool InventoryInquiry { get; set; }
        public bool HotOff { get; set; }
        public DateTime ReserveCapacityAndMaterialsUntil { get; set; }
        public string SchedulingType { get; set; }
        public decimal Revenue { get; set; }
        public decimal Throughput { get; set; }
        public bool IsHot { get; set; }
        public string HotReason { get; set; }
        public string Status { get; set; }
    }
}
