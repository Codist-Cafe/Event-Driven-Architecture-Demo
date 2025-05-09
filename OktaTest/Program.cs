using Okta.Sdk.Abstractions;
using Okta.Sdk.Api;
using Okta.Sdk.Client;
using Okta.Sdk.Model;

Configuration config = new Configuration
{
    OktaDomain = "https://trial-5489974.okta.com",
    Token = "00jDCv8hg1HctbJ4HYNmb7_FfFxRm7FIDXdnzECE7V"
};

var userApi = new UserApi(config);

try
{
    var createUserRequest = new CreateUserRequest
    {
        Profile = new UserProfile
        {
            FirstName = "Test",
            LastName = "User6",
            Email = "test6@example.com",
            Login = "test6@example.com"
        },
        Credentials = new UserCredentials
        {
            Password = new PasswordCredential
            {
                Value = "D1sturB1ng!"
            }
        }
    };
    var createdUser = await userApi.CreateUserAsync(createUserRequest);
    Console.WriteLine($"Created: {createdUser.Id} - {createdUser.Profile.Email}");
}
catch (OktaApiException ex)
{
    throw;
}
catch (Exception ex)
{
    throw;
}

try
{
    // Get a list of users
    User[] users = await userApi.ListUsers().ToArrayAsync();
    foreach (User user in users)
    {
        Console.WriteLine($"{user.Id}: {user.Profile.Email}");
    }
}
catch (Exception e)
{
    Console.WriteLine("An exception occurred: " + e.Message);
}