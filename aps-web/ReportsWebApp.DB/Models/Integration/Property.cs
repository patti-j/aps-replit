using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public EPropertyDataType DataType { get; set; }
        public EPropertySourceOption SourceOption { get; set; }
        public string FixedValue { get; set; }
        [ForeignKey(nameof(IntegrationConfig))]
        public int IntegrationConfigId { get; set; }
        public virtual IntegrationConfig IntegrationConfig { get; set; }
    }

    public enum EPropertyDataType
    {
        Boolean,
        Byte,
        DateTime,
        Decimal,
        Double,
        Short,
        Int,
        Long,
        String,
        Unspecified = 999
    }

    public enum EPropertySourceOption
    {
        FromTable,
        FixedValue,
        KeepValue,
        ClearValue
    }
}
