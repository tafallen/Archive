using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Archiver.ApiGateway.Tests;

public class RouteSecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RouteSecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        // We expect either OK (200) if backend is mocked/reachable, or ServiceUnavailable (503) / BadGateway (502) if backend is down.
        // The key is that it is NOT NotFound (404).
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable ||
            response.StatusCode == HttpStatusCode.BadGateway,
            $"Expected OK, 503, or 502, but got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Get_RandomPath_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/random/path/that/does/not/exist");

        // Assert
        // This confirms that the "catch-all" route is NOT present.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WeatherForecast_ReturnsMethodNotAllowed_Or_NotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/weatherforecast", null);

        // Assert
        // Since we restricted Methods to ["GET"], a POST request should not match the route.
        // If no route matches, YARP returns 404 (NotFound).
        // If YARP was configured to match path but filter method, it might return 405, but usually it's 404 if the Match block includes Methods.
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected NotFound or MethodNotAllowed, but got {response.StatusCode}");
    }

    [Fact]
    public async Task Get_Root_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        // The root path should not be exposed.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
