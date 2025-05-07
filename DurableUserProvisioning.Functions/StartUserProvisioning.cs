using DurableUserProvisioning.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using System.Net;
using Microsoft.Extensions.Logging;

namespace DurableUserProvisioning.Functions;

public static class StartUserProvisioning
{
    private const string YOUR_FUNCTION_KEY = "";
    [Function("StartUserProvisioning")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/provision")] HttpRequestData req,
        FunctionContext executionContext,
        [DurableClient] DurableTaskClient client)
    {
        var logger = executionContext.GetLogger("StartUserProvisioning");

        var requestBody = await req.ReadFromJsonAsync<User>();
        if (requestBody == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid request payload.");
            return badResponse;
        }

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync("ProvisionUserOrchestrator", requestBody);

        logger.LogInformation("Started orchestration with ID = {instanceId}", instanceId);

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        var baseUrl = $"{req.Url.Scheme}://{req.Url.Host}";
        var statusQueryUri = $"{baseUrl}/runtime/webhooks/durabletask/instances/{instanceId}?taskHub=DurableTaskHub&connection=Storage&code={YOUR_FUNCTION_KEY}";

        await response.WriteAsJsonAsync(new
        {
            instanceId,
            statusQueryGetUri = statusQueryUri
        });

        return response;
    }
}