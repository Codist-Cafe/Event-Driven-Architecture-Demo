using System.Text.Json;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.OktaHandler;

public static class HandleOktaEvent
{
    [Function(nameof(HandleOktaEvent))]
    public static void Run(
        [EventGridTrigger] EventGridEvent eventGridEvent,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(HandleOktaEvent));

        logger.LogInformation("Okta event received.");
        logger.LogInformation("Event Type: {EventType}", eventGridEvent.EventType);
        logger.LogInformation("Subject: {Subject}", eventGridEvent.Subject);
        logger.LogInformation("Data: {Data}", eventGridEvent.Data?.ToString());

        try
        {
            // Deserialize event data as needed
            var json = eventGridEvent.Data!.ToString();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Example: log a specific field if it exists
            if (root.TryGetProperty("id", out var idProp))
            {
                logger.LogInformation("User ID in event: {UserId}", idProp.GetString());
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing Okta event data.");
            throw;
        }
    }
}