using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using DurableUserProvisioning.Models;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.Functions;
public static class ProvisionUserOrchestrator
{
    [Function(nameof(ProvisionUserOrchestrator))]
    public static async Task Run(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ProvisionUserOrchestrator));
        var user = context.GetInput<User>();

        logger.LogInformation("Orchestration started for user: {UserId} - {Email} - {FirstName} - {LastName}", user!.Id, user.Email, user.FirstName, user.LastName);

        // Save user to Cosmos DB
        logger.LogInformation("Calling SaveUserToCosmos activity.");
        await context.CallActivityAsync(nameof(SaveUserToCosmos), user);

        // Publish event to Event Grid
        logger.LogInformation("Calling PublishProvisioningEvent activity.");
        await context.CallActivityAsync(nameof(PublishProvisioningEvent), user);

        logger.LogInformation("Orchestration completed for user: {UserId}", user.Id);
    }
}