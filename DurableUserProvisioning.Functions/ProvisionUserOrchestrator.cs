using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DurableUserProvisioning.Models;

namespace DurableUserProvisioning.Functions;
public static class ProvisionUserOrchestrator
{
    [Function(nameof(ProvisionUserOrchestrator))]
    public static async Task Run(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var user = context.GetInput<User>();

        // Save user to Cosmos DB
        await context.CallActivityAsync(nameof(SaveUserToCosmos), user);

        await context.CallActivityAsync(nameof(PublishProvisioningEvent), user);
    }
}