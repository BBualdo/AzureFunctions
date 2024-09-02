using AzureFunctions.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace AzureFunctions.Services;

public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    
    public CosmosDbService(string connectionString, string databaseName, string containerName)
    {
        _cosmosClient = new CosmosClient(connectionString);
        _container = _cosmosClient.GetContainer(databaseName, containerName);
    }
    
    public async Task AddOrderAsync(Order order)
    {
        await _container.CreateItemAsync(order, new PartitionKey(order.Id));
    }
}