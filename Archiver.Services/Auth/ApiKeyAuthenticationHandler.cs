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

    // Cache the expected API key and its UTF-8 representation
    private static string? _cachedApiKey;
    private static byte[]? _cachedExpectedKeyBytes;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (IsAnonymousEndpoint())
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!TryExtractApiKey(out var extractedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
        }

        var apiKey = configuration["Authentication:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key not configured"));
        }

        var cachedBytes = GetExpectedKeyBytes(apiKey);

        if (!IsValidApiKey(extractedApiKey.ToString(), cachedBytes))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        return Task.FromResult(CreateSuccessResult());
    }

    private bool IsAnonymousEndpoint()
    {
        var endpoint = Context.GetEndpoint();
        return endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;
    }

    private bool TryExtractApiKey(out Microsoft.Extensions.Primitives.StringValues extractedApiKey)
    {
        return Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out extractedApiKey);
    }

    private byte[] GetExpectedKeyBytes(string apiKey)
    {
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

        return cachedBytes;
    }

    private bool IsValidApiKey(string extractedStr, byte[] expectedBytes)
    {
        int maxBytes = Encoding.UTF8.GetMaxByteCount(extractedStr.Length);

        // Use stackalloc for typical small keys to avoid heap allocation
        if (maxBytes <= 256)
        {
            Span<byte> providedKeySpan = stackalloc byte[maxBytes];
            int length = Encoding.UTF8.GetBytes(extractedStr, providedKeySpan);
            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(providedKeySpan[..length], expectedBytes);
        }
        else
        {
            var providedKeyBytes = Encoding.UTF8.GetBytes(extractedStr);
            return CryptographicOperations.FixedTimeEquals(providedKeyBytes, expectedBytes);
        }
    }

    private AuthenticateResult CreateSuccessResult()
    {
        var identity = new ClaimsIdentity(CachedClaims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
