using System;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class License
    {
        public int id { get; set; }
        public string SerialCode { get; set; }
        public string LicenseId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime MaintenanceExpirationDate { get;  set; }
        public long MaxNbrMasterSchedulers { get;  set; }
        public long MaxNbrViewOnlyUsers { get;  set; }
        public long MaxNbrShopViewUsers { get;  set; }
        public long MaxNbrPlants { get;  set; }
        public string SystemId { get; set; }
        public bool ExportToDatabase { get; set; }
        public bool Import { get; set; }
        public bool MultipleOperations { get; set; }
        public bool SalesAndOperationsPlanning { get; set; }
        public bool IncludeFiniteOption { get; set; }
        public bool IncludeAdvancedPlanning { get; set; }
        public bool IncludeOptimization { get; }
        public bool WhatIfScenarios { get; set; }
        public bool ProcessManufacturing { get; set; }
        public bool MultipleResourceRequirements { get; set; }
        public bool MaterialRequirements { get; set; }
        public bool Optimizer { get; set; }
        public bool AllowJobEdits { get; set; }
        public bool Unlimited { get; set; }
    }
}
