using System.IO;
using Xunit;

namespace Archiver.ApiGateway.Tests;

public class SecurityScans
{
    [Fact]
    public void AppSettings_DoesNotContainVulnerableUrl()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        if (!File.Exists(jsonPath))
        {
             // Fallback for when running tests where output path might be different
             // trying to find it relative to the solution root if possible, or just fail
             throw new FileNotFoundException($"Could not find appsettings.json at {jsonPath}");
        }

        var json = File.ReadAllText(jsonPath);

        // Explicit check for the reported vulnerability
        Assert.DoesNotContain("https://localhost:7001", json);
    }
}
