using Microsoft.Azure.Functions.Worker;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.Functions;

public class PublishProvisioningEvent
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _eventGridEndpoint;
    private readonly string _eventGridKey;

    public PublishProvisioningEvent(IConfiguration config, ILoggerFactory loggerFactory)
    {
        _httpClient = new HttpClient();
        _logger = loggerFactory.CreateLogger<PublishProvisioningEvent>();

        _eventGridEndpoint = config["EventGridTopicEndpoint"];
        _eventGridKey = config["EventGridKey"];
    }

    [Function("PublishProvisioningEvent")]
    public async Task Run([ActivityTrigger] Models.User user)
    {
        try
        {
            var events = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    eventType = "User.Provisioned",
                    subject = $"user/{user.Id}",
                    eventTime = DateTime.UtcNow,
                    data = user,
                    dataVersion = "1.0"
                }
            };

            var json = JsonSerializer.Serialize(events);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("aeg-sas-key", _eventGridKey);

            var response = await _httpClient.PostAsync(_eventGridEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to publish event. Status: {response.StatusCode}");
                throw new Exception($"Event Grid publish failed: {response.StatusCode}");
            }

            _logger.LogInformation("Provisioning event published to Event Grid.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing provisioning event.");
            throw;
        }
    }
}
