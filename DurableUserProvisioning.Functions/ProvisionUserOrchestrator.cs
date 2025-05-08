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
    public static async Task<string> Run(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var userInfo = context.GetInput<User>();

        string result = await context.CallActivityAsync<string>(
            nameof(CreateUserActivity), userInfo);

        return $"Provisioning complete: {result}";
    }
}