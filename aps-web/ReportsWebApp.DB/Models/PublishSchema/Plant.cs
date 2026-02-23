using System;
using System.Collections.Generic;

namespace ReportsWebApp.DB.Models;

public partial class Plant
{
    public DateTime PublishDate { get; set; }

    public long PlantId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public double? BottleneckThreshold { get; set; }

    public double? HeavyLoadThreshold { get; set; }

    public string? ExternalId { get; set; }

    public string? AttributesSummary { get; set; }

    public int? DepartmentCount { get; set; }

    public double? StableDays { get; set; }

    public double? DailyOperatingExpense { get; set; }

    public double? InvestedCapital { get; set; }

    public double? AnnualPercentageRate { get; set; }

    public string? InstanceId { get; set; }
}
