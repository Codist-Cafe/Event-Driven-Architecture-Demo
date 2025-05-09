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

        _logger.LogInformation($"Domain: {oktaDomain}. Token: {oktaToken}.");
        var configuration = new Configuration
        {
            OktaDomain = oktaDomain,
            Token = oktaToken
        };

        _logger.LogInformation("userApi creating.");
        _userApi = new UserApi(configuration);
        _logger.LogInformation("userApi created.");
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

        _logger.LogInformation("Creating user {FirstName} {LastName} ({Email}) in Okta.", user.FirstName, user.LastName, user.Email);

        try
        {
            var createUserRequest = new CreateUserRequest
            {
                Profile = new UserProfile
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Login = user.Email
                },
                Credentials = new UserCredentials
                {
                    Password = new PasswordCredential
                    {
                        Value = user.Password
                    }
                }
            };
            var createdUser = await _userApi.CreateUserAsync(createUserRequest);
            _logger.LogInformation("User created in Okta: {Id}", createdUser.Id);
        }
        catch (OktaApiException ex)
        {
            _logger.LogError(ex,
                "Failed to create user in Okta. Error: {ErrorSummary}, Code: {ErrorCode}, Details: {Details}",
                ex.ErrorSummary, ex.ErrorCode, ex.InnerException?.Message ?? "None");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create user in Okta. {ex.Message}");
            throw;
        }
    }
}