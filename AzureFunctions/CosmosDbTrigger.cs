using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class CosmosDbTrigger
{
    private readonly ILogger _logger;

    public CosmosDbTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<CosmosDbTrigger>();
    }

    [Function("CosmosDbTrigger")]
    public void Run([CosmosDBTrigger(
            databaseName: "databaseName",
            containerName: "containerName",
            Connection = "",
            LeaseContainerName = "leases")]
        IReadOnlyList<MyDocument> input, FunctionContext context)
    {
        if (input != null && input.Count > 0)
        {
            _logger.LogInformation("Documents modified: " + input.Count);
            _logger.LogInformation("First document Id: " + input[0].Id);
        }

        
    }
}

public class MyDocument
{
    public string Id { get; set; }

    public string Text { get; set; }

    public int Number { get; set; }

    public bool Boolean { get; set; }
}