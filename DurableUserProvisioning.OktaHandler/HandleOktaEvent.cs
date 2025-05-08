using System.Text.Json;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Sdk.Abstractions;
using Okta.Sdk.Model;
using Okta.Sdk.Api;
using Okta.Sdk.Client;


namespace DurableUserProvisioning.OktaHandler;


public class HandleOktaEvent
{
    private readonly ILogger<HandleOktaEvent> _logger;
    private readonly IUserApi _userApi;

    public HandleOktaEvent(IConfiguration config, ILogger<HandleOktaEvent> logger)
    {
        _logger = logger;

        var oktaDomain = config["OktaDomain"];
        var oktaToken = config["OktaToken"];

        var clientConfiguration = new Configuration
        {
            OktaDomain = oktaDomain,
            Token = oktaToken
        };

        _userApi = new UserApi(clientConfiguration);
    }

    [Function("HandleOktaEvent")]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Okta event received: {EventType}", eventGridEvent.EventType);
        _logger.LogInformation("Okta event received: {EventData}", eventGridEvent.Data.ToString());


        var user = JsonSerializer.Deserialize<Models.User>(eventGridEvent.Data.ToString());
        if (user == null)
        {
            _logger.LogError("Invalid user payload.");
            return;
        }

        _logger.LogInformation("Creating user {Username} ({Email}) in Okta.", user.Name, user.Email);

        try
        {
            var createdUser =
                await _userApi.CreateUserAsync(new CreateUserRequest
                {
                    Profile = new UserProfile
                    {
                        FirstName = user.Name,
                        LastName = user.Name,
                        Email = user.Email,
                        Login = user.Email
                    }
                });
            _logger.LogInformation("User created in Okta: {Id}", createdUser.Id);
        }
        catch (OktaApiException ex)
        {
            _logger.LogError(ex, "Failed to create user in Okta. Error: {ErrorSummary}, Code: {ErrorCode}, Details: {Details}",
                ex.ErrorSummary, ex.ErrorCode, ex.InnerException?.Message ?? "None");
            throw;
        }

    }
}