using System.Collections.Concurrent;
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

    // Cache the claims array as it's static and doesn't change
    private static readonly Claim[] CachedClaims = { new(ClaimTypes.Name, "ApiKeyUser") };

    // Cache the AuthenticationTicket per scheme name to support multiple schemes correctly
    private static readonly ConcurrentDictionary<string, AuthenticationTicket> CachedTickets = new();

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

        // Get or create the cached ticket for this scheme
        var ticket = CachedTickets.GetOrAdd(Scheme.Name, schemeName =>
        {
            // Identity and Principal are technically mutable, but in this specific API key
            // usage context, they are treated as static representations of the service user.
            var identity = new ClaimsIdentity(CachedClaims, schemeName);
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, schemeName);
        });

        return AuthenticateResult.Success(ticket);
    }
}
