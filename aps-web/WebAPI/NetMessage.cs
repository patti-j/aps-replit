using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using WebAPI.Common;

namespace WebAPI;

public class NMessage
{
    public string MessageKey { get; set; }
    public string MessageData { get; set; }
}

public class UpdatePaApiKeyEvent
{
    public string PAKey { get; set; }
    public string ApiKey { get; set; }
}

public class RealTimeHub : Hub
{
    public async Task SendMessage(NMessage message)
    {
        switch (message.MessageKey)
        {
            case "UpdatePaApiKey":
            {
                var ev = JsonConvert.DeserializeObject<UpdatePaApiKeyEvent>(message.MessageData);
                if (ev == null)
                {
                    return;
                }
                CommonMethods.UpdatePaApiKey(ev);
                break;
            }
        }
    }
}

public interface IMessagePublisher
{
    public void Publish(string messageKey, object message);
}

public class SignalRMessagePublisher : IMessagePublisher
{
    private readonly IHubContext<RealTimeHub> _hubContext;

    public SignalRMessagePublisher(IHubContext<RealTimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public void Publish(string messageKey, object message)
    {
        _hubContext.Clients.All.SendAsync("ReceiveMessage", new NMessage() { MessageKey = messageKey, MessageData = JsonConvert.SerializeObject(message) });
    }

    internal static event Action<string, object>? SendToWebAppEv;

    public static void SendToWebApp<T>(string messageKey, T messageData)
    {
        if (messageData == null)
        {
            throw new ArgumentNullException(nameof(messageData));
        }
        SendToWebAppEv?.Invoke(messageKey, messageData);
    }
}