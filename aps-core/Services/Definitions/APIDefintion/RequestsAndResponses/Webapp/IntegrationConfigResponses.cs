using System.ComponentModel.DataAnnotations;

namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

public class IntegrationConfigOptionsResponse()
{
    public List<IntegrationConfigOptionsDTO> IntegrationConfigs { get; set; } = new();
}
public class IntegrationConfigOptionsDTO
{
    public int IntegrationConfigId { get; set; }
    public string IntegrationName { get; set; }
    public string VersionNumber { get; set; }
    public int? UpgradedFromConfigId { get; set; }

    public override string ToString() // Override so combobox can take whole object and use this as display
    {
        return IntegrationName;
    }
}

public class FeatureDTO
{
    [Key]
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string Name { get; set; }
    public bool? Distinct { get; set; }
    public bool? AutoDelete { get; set; }

}

public class PropertyDTO
{
    [Key]
    public int Id { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public EPropertyDataType DataType { get; set; }
    public EPropertySourceOption SourceOption { get; set; }
    public string FixedValue { get; set; }
    
    /// <summary>
    /// Enables common comparison key across PropertyDTO and underlying model class.
    /// </summary>
    public class PropertyLookupKey : IEquatable<PropertyLookupKey>
    {
        public string TableName { get; }
        public string ColumnName { get; }

        public PropertyLookupKey(string a_tableName, string a_columnName)
        {
            TableName = a_tableName;
            ColumnName = a_columnName;
        }

        public override bool Equals(object a_comparator)
        {
            return a_comparator is PropertyLookupKey other && Equals(other);
        }

        public bool Equals(PropertyLookupKey a_other)
        {
            return a_other != null &&
                   TableName == a_other.TableName &&
                   ColumnName == a_other.ColumnName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName, ColumnName);
        }
    }
}