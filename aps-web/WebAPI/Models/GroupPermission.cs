namespace WebAPI.Models
{
    public class GroupPermission
    {
        public int GroupId { get; set; }
        public Role Role { get; set; }
        public int PermissionId { get; set; }
        public PermissionKey Permission { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is GroupPermission gp)
            {
                return gp.GroupId == GroupId && gp.PermissionId == PermissionId;
            }
            return base.Equals(obj);
        }
    }
}
