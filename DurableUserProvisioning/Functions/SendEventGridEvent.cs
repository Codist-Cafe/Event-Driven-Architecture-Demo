using Azure;
using Azure.Messaging.EventGrid;
using DurableUserProvisioning.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.Functions;

public static class SendEventGridEvent
{
    [Function(nameof(SendEventGridEvent))]
    public static async Task Run(
        [ActivityTrigger] User user,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(SendEventGridEvent));

        var endpoint = Environment.GetEnvironmentVariable("EventGridTopicEndpoint");
        var key = Environment.GetEnvironmentVariable("EventGridKey");

        var credentials = new AzureKeyCredential(key);
        var client = new EventGridPublisherClient(new Uri(endpoint), credentials);

        var eventGridEvent = new EventGridEvent(
            subject: $"user/{user.Id}",
            eventType: "User.Created",
            dataVersion: "1.0",
            data: user
        );

        await client.SendEventAsync(eventGridEvent);
        logger.LogInformation($"Event sent to Event Grid for user {user.Email}.");
    }
}