using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Integration
{
    public class PropertyDTO
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public EPropertyDataType DataType { get; set; }
        public EPropertySourceOption SourceOption { get; set; }
        public string FixedValue { get; set; }

        public Property ToModel(int ConfigId)
        {
            return new Property
            {
                Id = Id,
                TableName = TableName,
                ColumnName = ColumnName,
                DataType = DataType,
                SourceOption = SourceOption,
                FixedValue = FixedValue,
                IntegrationConfigId = ConfigId
            };
        }
    }
}
