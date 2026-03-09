using Microsoft.AspNetCore.Authentication;

namespace Archiver.Services.Auth;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
}
