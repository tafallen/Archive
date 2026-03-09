using System.Net;
using System.Net.Http.Json;
using Archiver.Services.Auth;
using Archiver.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Archiver.Services.Tests;

public class WeatherForecastIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestApiKey = "integration-test-key-123";

    public WeatherForecastIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:ApiKey"] = TestApiKey
                });
            });
        });
    }

    [Fact]
    public async Task Get_WeatherForecast_NoApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_ValidApiKey_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AuthConstants.ApiKeyHeaderName, TestApiKey);

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        Assert.NotNull(forecasts);
        Assert.Equal(WeatherForecast.ForecastDays, forecasts.Length);
    }

    [Fact]
    public async Task Get_WeatherForecast_InvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AuthConstants.ApiKeyHeaderName, "WRONG_KEY");

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
