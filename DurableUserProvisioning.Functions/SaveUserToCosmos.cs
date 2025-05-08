using Microsoft.Azure.Cosmos;

using DurableUserProvisioning.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.Functions;

public class SaveUserToCosmos
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger _logger;

    public SaveUserToCosmos(IConfiguration config, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SaveUserToCosmos>();

        var connectionString = config["CosmosDBConnection"];
        var databaseName = "UserDb";
        var containerName = "Users";

        _cosmosClient = new CosmosClient(connectionString);
        _container = _cosmosClient.GetContainer(databaseName, containerName);
    }

    [Function("SaveUserToCosmos")]
    public async Task Run([ActivityTrigger] Models.User user)
    {
        try
        {
            _logger.LogInformation($"Saving user to Cosmos DB: {user.Id} - {user.Email}");

            await _container.UpsertItemAsync(user, new PartitionKey(user.Id));

            _logger.LogInformation("User saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving user {user.Id} to Cosmos DB.");
            throw;
        }
    }
}