using System.Text.Encodings.Web;
using Archiver.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Archiver.Services.Tests.Auth;

public class ApiKeyAuthenticationHandlerTests
{
    [Fact]
    public async Task HandleAuthenticateAsync_MissingApiKey_ReturnsFail()
    {
        // Arrange
        var options = new ApiKeyAuthenticationOptions();
        var optionsMonitor = new TestOptionsMonitor<ApiKeyAuthenticationOptions>(options);
        var loggerFactory = NullLoggerFactory.Instance;
        var encoder = UrlEncoder.Default;

        var configuration = new ConfigurationBuilder().Build();

        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, configuration);

        var context = new DefaultHttpContext();
        var scheme = new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler));

        await handler.InitializeAsync(scheme, context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Missing API Key", result.Failure?.Message);
    }

    private class TestOptionsMonitor<T>(T currentValue) : IOptionsMonitor<T> where T : class, new()
    {
        private readonly T _currentValue = currentValue;
        public T Get(string? name) => _currentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
        public T CurrentValue => _currentValue;
    }
}
