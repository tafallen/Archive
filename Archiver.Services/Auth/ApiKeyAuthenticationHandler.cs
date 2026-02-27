using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Archiver.Services.Auth;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Allow unauthenticated requests for metadata endpoints like OpenAPI
        var endpoint = Context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
        {
            return AuthenticateResult.NoResult();
        }

        if (!Request.Headers.TryGetValue("X-Internal-Key", out var extractedApiKey))
        {
            return AuthenticateResult.Fail("Missing API Key");
        }

        var apiKey = _configuration["Authentication:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return AuthenticateResult.Fail("API Key not configured");
        }

        // Constant-time comparison to prevent timing attacks
        var providedKeyBytes = Encoding.UTF8.GetBytes(extractedApiKey.ToString());
        var expectedKeyBytes = Encoding.UTF8.GetBytes(apiKey);

        if (!CryptographicOperations.FixedTimeEquals(providedKeyBytes, expectedKeyBytes))
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
