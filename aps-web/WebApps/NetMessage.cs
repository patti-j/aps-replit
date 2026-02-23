using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

using ReportsWebApp.Common;

namespace ReportsWebApp;

//using dependency injection here might be a bit overkill but if we heavily depend on it in the future it could help us quite a bit if we ever want to switch messaging protocols (ZMQ/RabbitMQ)
public class NetMessage
{
    public delegate void HandleMessageCB(string messageTypeKey, string messageDataJson);
    
    private INetMessageSocketProvider m_socketProvider;
    private HandleMessageCB m_handleMessageCB;
    public NetMessage(INetMessageSocketProvider socketProvider, HandleMessageCB handler)
    {
        m_socketProvider = socketProvider;
        m_handleMessageCB = handler;
        m_socketProvider.SetHandleMessageCB(m_handleMessageCB);
    }

    public void SendMessage<T>(string messageTypeKey, T messageData)
    {
        m_socketProvider.SendMessage(messageTypeKey, messageData);
    }
}

public interface INetMessageSocketProvider : IDisposable
{
    public void SetHandleMessageCB(NetMessage.HandleMessageCB handler);
    public void SendMessage<T>(string messageTypeKey, T messageData);
}

public class NMessage
{
    public string MessageKey { get; set; }
    public string MessageData { get; set; }
}

public class UpdatePaApiKeyEvent : IEvent
{
    public UpdatePaApiKeyEvent(string a_apiKey, string a_paKey)
    {
        ApiKey = a_apiKey;
        PAKey = a_paKey;
    }
    
    public string PAKey { get; set; }
    public string ApiKey { get; set; }
}

public class SignalRClientMessageProvider : INetMessageSocketProvider
{
    private NetMessage.HandleMessageCB? m_handleMessageCB; 
    private HubConnection m_connection; 
    
    public SignalRClientMessageProvider(HubConnection hubConnection)
    {
        m_connection = hubConnection;
        m_connection.On<NMessage>("ReceiveMessage", (msg) =>
        {
            if (m_handleMessageCB != null)
            {
                m_handleMessageCB(msg.MessageKey, msg.MessageData);
            }
        });
    }
    
    public void SetHandleMessageCB(NetMessage.HandleMessageCB handler)
    {
        m_handleMessageCB = handler;
    }
    public void SendMessage<T>(string messageTypeKey, T messageData)
    {
        m_connection.InvokeAsync("SendMessage", new NMessage() { MessageKey = messageTypeKey, MessageData = JsonConvert.SerializeObject(messageData) });
    }

    public void Dispose()
    {
        m_connection.DisposeAsync();
    }
}