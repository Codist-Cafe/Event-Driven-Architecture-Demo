using DurableUserProvisioning.Models;
using Microsoft.Azure.Functions.Worker;

namespace DurableUserProvisioning.Functions;

public static class CreateUserActivity
{
    [Function(nameof(CreateUserActivity))]
    public static Task<string> Run([ActivityTrigger] User input)
    {
        // Replace this with real provisioning logic
        return Task.FromResult($"User {input.Id} ({input.Email}) created.");
    }
}