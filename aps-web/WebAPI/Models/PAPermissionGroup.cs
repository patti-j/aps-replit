namespace WebAPI.Models
{
    public class PAPermissionGroup : BaseEntity
    {
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public List<PAUserPermission> Permissions { get; set; } = new();
    }
}
