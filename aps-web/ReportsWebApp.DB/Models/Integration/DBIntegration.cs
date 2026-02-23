using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models;

public class DBIntegration
{
    [Key]
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Version { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }
    
    [Required]
    [ForeignKey("CreatedByUser")]
    public int CreatedBy { get; set; }
    
    public virtual User? CreatedByUser { get; set; }
    
    [ForeignKey("Company")] 
    public int? CompanyId { get; set; }
    public virtual Company? Company { get; set; }
    
    public string VersionNotes { get; set; }
    
    public List<DBIntegrationObject> IntegrationDBObjects { get; set; }
    
    public override bool Equals(object obj) //TODO: Fix BaseEntity to use User Id instead of string
    {
        if (obj is DBIntegration typedObj)
        {
            return Id == typedObj.Id;
        }
        return base.Equals(obj);
    }
    public override string ToString()
    {
        return Name;
    }
}