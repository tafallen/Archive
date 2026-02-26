using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Archiver.ApiGateway.Tests;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_RandomPath_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/random");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsOkOrServiceUnavailable()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        // If the route matches, YARP will forward it. If backend is unreachable, it returns 503.
        // If the route doesn't match, it returns 404.
        // We want to ensure it DOES match (so NOT 404).
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
