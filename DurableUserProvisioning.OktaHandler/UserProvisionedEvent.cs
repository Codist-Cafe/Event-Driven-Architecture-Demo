namespace DurableUserProvisioning.OktaHandler;

public class UserProvisionedEvent
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}