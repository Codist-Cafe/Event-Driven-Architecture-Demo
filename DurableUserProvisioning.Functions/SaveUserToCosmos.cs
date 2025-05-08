using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DurableUserProvisioning.Models;
using Newtonsoft.Json;

namespace DurableUserProvisioning.Functions;

public class SaveUserToCosmos
{
    private readonly Container _container;
    private readonly ILogger<SaveUserToCosmos> _logger;

    public SaveUserToCosmos(IConfiguration config, ILogger<SaveUserToCosmos> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var connectionString = config["CosmosDBConnection"]
                               ?? throw new ArgumentException("Missing CosmosDBConnection in configuration.");

        var cosmosClient = new CosmosClient(connectionString);
        _container = cosmosClient.GetContainer("UserDb", "Users");
    }

    [Function("SaveUserToCosmos")]
    public async Task Run([ActivityTrigger] Models.User user)
    {
        try
        {
            _logger.LogInformation("Saving user to Cosmos DB: {UserId}, {Email}, {UserName}", user.Id, user.Email, user.Name);

            _logger.LogInformation("Serialized User: {UserJson}", JsonConvert.SerializeObject(user));

            var response = await _container.UpsertItemAsync(user, new PartitionKey(user.Id));

            _logger.LogInformation("User saved. Status Code: {StatusCode}", response.StatusCode);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error saving user {UserId}. Status: {Status}", user.Id, ex.StatusCode);
            _logger.LogError("User data: {@User}", user);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving user {UserId}.", user.Id);
            throw;
        }
    }
}