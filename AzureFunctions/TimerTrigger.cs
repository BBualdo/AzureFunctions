using System.Text;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class TimerTrigger
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly ILogger _logger;
    private readonly SalesReportService _salesReportService;

    public TimerTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TimerTrigger>();
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ??
                               throw new ArgumentNullException();
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ??
                           throw new ArgumentNullException();
        const string containerName = "Orders";
        _cosmosDbService = new CosmosDbService(connectionString, databaseName, containerName);

        var blobConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ??
                                   throw new ArgumentNullException();
        _salesReportService = new SalesReportService(blobConnectionString);
    }

    [Function("TimerTrigger")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var date = DateTime.UtcNow.Date.AddDays(-1);
        var orders = await _cosmosDbService.GetOrdersFromDateAsync(date);

        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine($"Sales Report for {date:yyyy-MM-dd}");
        reportBuilder.AppendLine("OrderId\tProductName\tQuantity\tPrice");

        foreach (var order in orders)
            reportBuilder.AppendLine($"{order.Id}\t{order.ProductName}\t{order.Quantity}\t{order.Price:C}");

        var fileName = $"sales-report-{date:yyyy-MM-dd}.txt";
        await _salesReportService.SaveReportAsync("sales-report", fileName, reportBuilder.ToString());

        _logger.LogInformation($"Sales report generated and saved to Blob Storage: {fileName}");
    }
}