using System;
using System.Text;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class TimerTrigger
{
    private readonly ILogger _logger;
    private readonly CosmosDbService _cosmosDbService;
    
    public TimerTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TimerTrigger>();
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ?? 
                               throw new ArgumentNullException();
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ??
                           throw new ArgumentNullException();
        const string containerName = "Orders";
        _cosmosDbService = new CosmosDbService(connectionString, databaseName, containerName);
    }

    [Function("TimerTrigger")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var orders = await _cosmosDbService.GetOrdersFromDateAsync(DateTime.UtcNow.Date);

        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("Id------ProductName------Quantity------Price------Status");

        foreach (var order in orders)
        {
            reportBuilder.AppendLine($"{order.Id}------{order.ProductName}------{order.Quantity}------{order.Price}$------{order.Status}");
        }

        var reportPath = Path.Combine(Environment.CurrentDirectory, $"SalesReport_{DateTime.UtcNow:yyyyMMdd}.csv");
        await File.WriteAllTextAsync(reportPath, reportBuilder.ToString());
        
        _logger.LogInformation($"Sales report generated and saved to: {reportPath}");
    }
}