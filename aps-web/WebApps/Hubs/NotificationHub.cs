using Microsoft.AspNetCore.SignalR;


namespace ReportsWebApp.Hubs
{

    public class NotificationHub : Hub
    {
	    public async Task JoinGroup(string email)
	    {
            if(!string.IsNullOrEmpty(email))
		    await Groups.AddToGroupAsync(Context.ConnectionId, email);
	    }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}