using System;
using System.Collections.Generic;

namespace ReportsWebApp.DB.Models;

public partial class Department
{
    public Plant Plant { get; set; }

    public DateTime PublishDate { get; set; }

    public long PlantId { get; set; }

    public long DepartmentId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public string? ExternalId { get; set; }

    public string? AttributesSummary { get; set; }

    public string? PlantName { get; set; }

    public int? ResourceCount { get; set; }

    public double? DepartmentFrozenSpanDays { get; set; }

    public string? InstanceId { get; set; }
}
