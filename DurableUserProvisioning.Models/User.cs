using System.Text.Json.Serialization;

namespace DurableUserProvisioning.Models;

public class User
{
    [JsonPropertyName("id")] public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
