using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Archiver.WebApp.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Note: 'unsafe-inline' is currently required for Blazor Server.
        // Future improvement: Implement nonces to remove 'unsafe-inline'.
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:; frame-ancestors 'self'");

        context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        // Ensure no caching for sensitive data by default, can be overridden by specific endpoints if needed
        if (!context.Response.Headers.ContainsKey("Cache-Control"))
        {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, max-age=0, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
        }

        await _next(context);
    }
}
