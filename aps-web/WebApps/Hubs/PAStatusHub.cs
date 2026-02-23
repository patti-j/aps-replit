using Microsoft.AspNetCore.SignalR;

using ReportsWebApp.DB.Models;

namespace ReportsWebApp.Hubs
{

    public class PAStatusHub : Hub
    {
	    public async Task JoinGroupsForPAs(List<string> paKeys)
	    {
            foreach (var key in paKeys)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, key);
            }
        }

        public async Task LeaveGroupsForPAs(List<string> pas)
        {
            foreach (var pa in pas)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, pa);
            }
        }
    }
}