namespace WebAPI.Models   
{
    public class PAPermissionGroupPAUserPermission
    {
        public int PAPermissionGroupId { get; set; }
        public PAPermissionGroup PAPermissionGroup { get; set; }
        public int PAUserPermissionId { get; set; }
        public PAUserPermission PAUserPermission { get; set; }
    }
}
