using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Archiver.Services.Auth;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    // Cache the claims array as it's static and doesn't change
    private static readonly Claim[] CachedClaims = { new(ClaimTypes.Name, "ApiKeyUser") };

    // Cache the AuthenticateResult per scheme name to support multiple schemes correctly
    private static readonly ConcurrentDictionary<string, AuthenticateResult> CachedResults = new();

    // Cache the expected API key and its UTF-8 representation
    private static string? _cachedApiKey;
    private static byte[]? _cachedExpectedKeyBytes;

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

        var apiKey = configuration["Authentication:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return AuthenticateResult.Fail("API Key not configured");
        }

        // Cache the expected key bytes to avoid allocating a byte array per request.
        // Copy to locals to avoid thread-safety issues when caching logic updates.
        var cachedKey = _cachedApiKey;
        var cachedBytes = _cachedExpectedKeyBytes;

        if (cachedKey != apiKey || cachedBytes == null)
        {
            cachedKey = apiKey;
            cachedBytes = Encoding.UTF8.GetBytes(apiKey);
            _cachedExpectedKeyBytes = cachedBytes; // Order matters: set bytes first
            _cachedApiKey = cachedKey;
        }

        var extractedStr = extractedApiKey.ToString();
        int maxBytes = Encoding.UTF8.GetMaxByteCount(extractedStr.Length);
        bool isValid = false;

        // Use stackalloc for typical small keys to avoid heap allocation
        if (maxBytes <= 256)
        {
            Span<byte> providedKeySpan = stackalloc byte[maxBytes];
            int length = Encoding.UTF8.GetBytes(extractedStr, providedKeySpan);
            // Constant-time comparison to prevent timing attacks
            isValid = CryptographicOperations.FixedTimeEquals(providedKeySpan[..length], cachedBytes);
        }
        else
        {
            var providedKeyBytes = Encoding.UTF8.GetBytes(extractedStr);
            isValid = CryptographicOperations.FixedTimeEquals(providedKeyBytes, cachedBytes);
        }

        if (!isValid)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        // Prevent redundant allocation of Claims array, AuthenticationTicket, and AuthenticateResult per request.
        // Fast path: use TryGetValue to completely avoid the delegate allocation overhead of GetOrAdd.
        if (!CachedResults.TryGetValue(Scheme.Name, out var result))
        {
            // Identity and Principal are technically mutable, but in this specific API key
            // usage context, they are treated as static representations of the service user.
            var identity = new ClaimsIdentity(CachedClaims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            result = AuthenticateResult.Success(ticket);
            CachedResults.TryAdd(Scheme.Name, result);
        }

        return result;
    }
}
