using Azure;
using DurableUserProvisioning.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.Functions;

public static class SaveUserToCosmos
{
    [Function(nameof(SaveUserToCosmos))]
    [CosmosDBOutput("UserDb", "Users", Connection = "CosmosDBConnection")]
    public static User Run(
        [ActivityTrigger] User user,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(SaveUserToCosmos));
        logger.LogInformation($"Saving user {user.Email} to Cosmos DB...");

        return user;
    }
}
