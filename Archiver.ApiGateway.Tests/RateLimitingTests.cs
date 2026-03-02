using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Archiver.ApiGateway.Tests;

public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RateLimitingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ManyRequests_ReturnsTooManyRequests()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act & Assert
        HttpStatusCode lastStatusCode = HttpStatusCode.OK;
        for (int i = 0; i < 110; i++)
        {
            var response = await client.GetAsync("/weatherforecast");
            lastStatusCode = response.StatusCode;
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastStatusCode);
    }
}
