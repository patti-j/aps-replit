namespace ReportsWebApp.DB.Models;

public class UserTeam
{
    public int UsersId { get; set; }
    public User User { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; }
}