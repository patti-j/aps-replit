namespace PT.PackageDefinitionsUI.ElementHelperBaseClasses;

public class BaseKpi
{
    public string Name { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// A unique ID for this KPI. No two KPIs loaded in the system can have the same ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// For comparisons, whether lower values are better
    /// </summary>
    public bool LowerIsBetter { get; set; }

    /// <summary>
    /// The KPI category. Different UI elements will retrieve KPIs by category.
    /// </summary>
    public string PackageObjectId => "kpiElement_" + Name;
}