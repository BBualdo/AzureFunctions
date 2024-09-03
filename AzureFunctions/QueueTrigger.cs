using AzureFunctions.Models;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions;

public class QueueTrigger
{
    private readonly ILogger<QueueTrigger> _logger;
    private readonly CosmosDbService _cosmosDbService;

    public QueueTrigger(ILogger<QueueTrigger> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ?? 
                               throw new ArgumentNullException();
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ??
                           throw new ArgumentNullException();
        const string containerName = "Orders";
        _cosmosDbService = new CosmosDbService(connectionString, databaseName, containerName);
    }

    [Function(nameof(QueueTrigger))]
    public async Task Run([QueueTrigger("orders-queue", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogInformation("C# Queue Trigger function processed a queue message.");
        var order = JsonConvert.DeserializeObject<Order>(message);

        if (order is not null)
        {
            _logger.LogInformation($"OrderID: {order.Id}, Status: {order.Status}");
            order.Status = StatusOptions.PaymentComplete;

            _logger.LogInformation("Updating order status in CosmosDB.");
            await _cosmosDbService.UpdateOrderAsync(order);
            
            _logger.LogInformation($"OrderID: {order.Id}, Status: {order.Status}");
        }
        else
        {
            _logger.LogError("Failed to process the queue message.");
        }
    }
}