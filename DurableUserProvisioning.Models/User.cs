namespace DurableUserProvisioning.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
