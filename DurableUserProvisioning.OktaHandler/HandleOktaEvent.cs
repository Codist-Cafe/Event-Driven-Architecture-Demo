using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DurableUserProvisioning.OktaHandler;
public class HandleOktaEvent
{
    private readonly ILogger _logger;

    public HandleOktaEvent(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HandleOktaEvent>();
    }

    [Function("HandleOktaEvent")]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Received event: {data}", eventGridEvent.Data.ToString());

        // Parse event and act on it
        var userEvent = JsonSerializer.Deserialize<UserProvisionedEvent>(eventGridEvent.Data.ToString());
        _logger.LogInformation($"Provisioned user {userEvent?.Id} for Okta.");

        // You can now call Okta APIs or log/audit
        await Task.CompletedTask;
    }
}