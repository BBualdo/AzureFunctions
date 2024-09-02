using System.Net;
using AzureFunctions.Models;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions;

public class HttpTrigger
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly ILogger _logger;

    public HttpTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HttpTrigger>();
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ?? 
                               throw new ArgumentNullException();
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ??
                           throw new ArgumentNullException();
        const string containerName = "Orders";
        _cosmosDbService = new CosmosDbService(connectionString, databaseName, containerName);
    }

    [Function("HttpTrigger")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("C# HTTP Trigger function processed a request.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var order = JsonConvert.DeserializeObject<Order>(requestBody);

        if (order is null)
        {
            _logger.LogError("Order processing failed. Order has insufficient information.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
        
        _logger.LogInformation("Saving order to CosmosDB.");
        await _cosmosDbService.AddOrderAsync(order);

        // TODO: Add Message to Azure Queue

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.WriteString("Order received and is being processed.");

        return response;
    }
}