using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using DurableUserProvisioning.Models;

namespace DurableUserProvisioning.Functions;

public static class StartUserProvisioning
{
    [Function("StartUserProvisioning")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/provision")] HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var logger = context.GetLogger("StartUserProvisioning");
        logger.LogInformation("Received user provisioning request.");

        // Deserialize incoming JSON body
        User? user;
        try
        {
            user = await req.ReadFromJsonAsync<User>();
            if (user == null)
            {
                logger.LogWarning("Request body is null or invalid JSON.");
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Invalid or empty request body.");
                return badReq;
            }

            logger.LogInformation("Provisioning user: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize request body.");
            var errorResp = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResp.WriteStringAsync("Malformed JSON: " + ex.Message);
            return errorResp;
        }

        // Start Durable Orchestration
        try
        {
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ProvisionUserOrchestrator), user);
            
            logger.LogInformation("Started orchestration with ID = {InstanceId}", instanceId);

            var response = req.CreateResponse(HttpStatusCode.Accepted);

            var functionKey = Environment.GetEnvironmentVariable("FUNCTION_KEY_PROVISION");
            var baseUrl = $"{req.Url.Scheme}://{req.Url.Host}";

            var statusUrl = $"{baseUrl}/runtime/webhooks/durabletask/instances/{instanceId}?taskHub=DurableTaskHub&connection=Storage&code={functionKey}";

            await response.WriteAsJsonAsync(new
            {
                instanceId,
                statusQueryGetUri = statusUrl
            });

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start orchestration.");
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync("Internal server error: " + ex.Message);
            return error;
        }
    }
}
