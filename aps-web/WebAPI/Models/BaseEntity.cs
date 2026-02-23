using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class BaseEntity
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public BaseEntity()
        {
            CreationDate = DateTime.UtcNow;
        }
        public override bool Equals(object obj)
        {
            if (obj is BaseEntity typedObj)
            {
                return Id == typedObj.Id && Name == typedObj.Name;
            }
            return base.Equals(obj);
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
