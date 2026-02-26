using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Archiver.ApiGateway.Tests;

public class ConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void AppSettings_DoesNotContainHardcodedAddress()
    {
        // Verify that the base appsettings.json does not contain hardcoded URLs
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        if (!File.Exists(jsonPath))
        {
             throw new FileNotFoundException($"Could not find appsettings.json at {jsonPath}");
        }

        var json = File.ReadAllText(jsonPath);

        // Security checks
        Assert.DoesNotContain("localhost:7001", json);
        Assert.DoesNotContain("REPLACE_ME", json);

        // Structural check
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Navigate: ReverseProxy -> Clusters -> serviceCluster -> Destinations
        if (root.TryGetProperty("ReverseProxy", out var reverseProxy) &&
            reverseProxy.TryGetProperty("Clusters", out var clusters))
        {
             // Check if Clusters is Object (Dictionary) or Array
             if (clusters.ValueKind == JsonValueKind.Object)
             {
                 if (clusters.TryGetProperty("serviceCluster", out var serviceCluster) &&
                     serviceCluster.TryGetProperty("Destinations", out var destinations))
                 {
                     // Destinations should be empty or default should not have Address
                     if (destinations.ValueKind == JsonValueKind.Object)
                     {
                         if (destinations.TryGetProperty("default", out var def))
                         {
                             // If default exists, Address should be empty
                             if (def.TryGetProperty("Address", out var address))
                             {
                                 Assert.True(string.IsNullOrEmpty(address.GetString()), "Address must be empty if present");
                             }
                         }
                         else
                         {
                             // No default destination - GOOD
                             Assert.True(true);
                         }
                     }
                 }
             }
             else if (clusters.ValueKind == JsonValueKind.Array)
             {
                 // Handle Array case if reverted, but we expect Object now
                 Assert.Fail("Clusters should be an Object (Dictionary) for consistent merging.");
             }
        }
    }

    [Fact]
    public async Task ApplicationStarts_Successfully()
    {
        // This ensures that the empty Address in appsettings.json doesn't crash the app
        // when combined with appsettings.Development.json (which WebApplicationFactory uses by default).
        var client = _factory.CreateClient();

        // We just want to ensure startup didn't throw.
        // Making a request ensures the pipeline is built.
        var response = await client.GetAsync("/unknown-route");

        // Should be 404 (or handled by YARP), but definitely not 500 Startup Error.
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == System.Net.HttpStatusCode.BadGateway);
    }
}
