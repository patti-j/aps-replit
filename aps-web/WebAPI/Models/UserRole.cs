namespace WebAPI.Models
{
    public class UserRole
    {
        public int UsersId { get; set; }
        public User User { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
