using Azure.Storage.Queues;

namespace ReportsWebApp.Controllers.Models;

public class QueueManager
{
    private readonly string _connectionString;
    private readonly string _queueName;

    public QueueManager(string connectionString, string queueName)
    {
        _connectionString = connectionString;
        _queueName = queueName;
    }

    public async Task SendMessageAsync(string message)
    {
        // Create a QueueClient to interact with the queue
        QueueClient queueClient = new QueueClient(_connectionString, _queueName);

        // Create the queue if it doesn't exist
        await queueClient.CreateIfNotExistsAsync();

        // Send a message to the queue
        await queueClient.SendMessageAsync(message);
    }
}