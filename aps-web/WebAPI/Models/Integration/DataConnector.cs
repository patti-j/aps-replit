using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models.Integration;

public class DataConnector
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";
    
    [Required]
    [ForeignKey(nameof(Company))]
    public int CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    [Required]
    public string ConnectionString { get; set; } = "";
}