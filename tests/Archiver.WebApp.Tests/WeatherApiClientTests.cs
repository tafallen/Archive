using Archiver.Shared;
using Archiver.WebApp.Clients;
using RichardSzalay.MockHttp;
using System.Net.Http.Json;

namespace Archiver.WebApp.Tests;

public class WeatherApiClientTests
{
    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsForecasts_WhenApiReturnsData()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedForecasts = new[]
        {
            new WeatherForecast(new DateOnly(2023, 10, 1), 20, "Mild"),
            new WeatherForecast(new DateOnly(2023, 10, 2), 25, "Warm")
        };

        mockHttp.When("/weatherforecast")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(expectedForecasts));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7005");
        var client = new WeatherApiClient(httpClient);

        // Act
        var result = await client.GetWeatherForecastAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal(expectedForecasts[0], result[0]);
        Assert.Equal(expectedForecasts[1], result[1]);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsEmptyArray_WhenApiReturnsNull()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        mockHttp.When("/weatherforecast")
                .Respond("application/json", "null");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7005");
        var client = new WeatherApiClient(httpClient);

        // Act
        var result = await client.GetWeatherForecastAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
