﻿namespace DurableUserProvisioning.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}