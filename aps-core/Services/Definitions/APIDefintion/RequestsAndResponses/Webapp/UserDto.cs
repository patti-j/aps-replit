using System.ComponentModel.DataAnnotations.Schema;
namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

public class UserDto
{
    public string Email { get; set; }
    public string ExternalId { get; set; } = Guid.NewGuid().ToString();
    public int WebAppId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public string DisplayLanguage { get; set; } = "English";
    public ECompressionType CompressionType { get; set; } = ECompressionType.Normal;
    
    public List<string> PermissionSet { get; set; } = new List<string>();
}

public enum ECompressionType
{
    /// <summary>
    /// Indicates no compression
    /// </summary>
    None = 0,

    /// <summary>
    /// Normal Compression (currently LZ4)
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Fast Compression (currently LZ4)
    /// </summary>
    Fast = 2,

    /// <summary>
    /// High Compression (currently LMZA)
    /// </summary>
    High = 3
}