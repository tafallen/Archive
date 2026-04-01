using Archiver.WebApp.Middleware;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Xunit;

namespace Archiver.WebApp.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AddsExpectedSecurityHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(next: (innerContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("SAMEORIGIN", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()", context.Response.Headers["Permissions-Policy"]);
        Assert.Equal("none", context.Response.Headers["X-Permitted-Cross-Domain-Policies"]);
    }

    [Fact]
    public async Task InvokeAsync_GeneratesAndAddsCspNonce()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(next: (innerContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Items.ContainsKey("csp-nonce"));
        var nonce = context.Items["csp-nonce"]?.ToString();
        Assert.False(string.IsNullOrEmpty(nonce));

        var cspHeader = context.Response.Headers["Content-Security-Policy"].ToString();
        Assert.Contains($"'nonce-{nonce}'", cspHeader);
    }

    [Fact]
    public async Task InvokeAsync_AddsCacheControlHeaders_WhenNotPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(next: (innerContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("no-store, no-cache, max-age=0, must-revalidate", context.Response.Headers["Cache-Control"]);
        Assert.Equal("no-cache", context.Response.Headers["Pragma"]);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotOverrideExistingCacheControlHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedCacheControl = "public, max-age=3600";
        context.Response.Headers.Append("Cache-Control", expectedCacheControl);

        var middleware = new SecurityHeadersMiddleware(next: (innerContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(expectedCacheControl, context.Response.Headers["Cache-Control"]);
        Assert.False(context.Response.Headers.ContainsKey("Pragma"));
    }
}
