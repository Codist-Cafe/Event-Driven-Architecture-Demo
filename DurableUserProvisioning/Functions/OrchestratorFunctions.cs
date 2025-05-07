using Azure;
using DurableUserProvisioning.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableUserProvisioning.Functions;

public static class OrchestratorFunction
{
    [Function(nameof(OrchestratorFunction))]
    public static async Task<string> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger(nameof(OrchestratorFunction));
        var user = context.GetInput<User>();
        logger.LogInformation($"Starting orchestration for {user.Email}");

        // Step 1 & 2: Save user to Cosmos DB
        await context.CallActivityAsync(nameof(SaveUserToCosmos), user);

        // Step 3: Send event to Event Grid
        await context.CallActivityAsync(nameof(SendEventGridEvent), user);

        return $"Orchestration complete for {user.Email}";
    }
}
