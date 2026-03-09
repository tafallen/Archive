using System.IO;
using Xunit;

namespace Archiver.Services.Tests;

public class SecurityScans
{
    [Fact]
    public void AppSettings_DoesNotContainPlaceholderSecret()
    {
        // Try multiple possible locations for appsettings.json to handle different test runners
        var pathsToTry = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Archiver.Services", "appsettings.json")
        };

        string? jsonPath = null;
        foreach (var path in pathsToTry)
        {
            if (File.Exists(path))
            {
                jsonPath = path;
                break;
            }
        }

        if (jsonPath == null)
        {
            throw new FileNotFoundException($"Could not find appsettings.json. Checked: {string.Join(", ", pathsToTry)}");
        }

        var json = File.ReadAllText(jsonPath);

        // Explicit check for the reported placeholder secret
        Assert.DoesNotContain("REPLACE_ME_WITH_REAL_SECRET", json);

        // Also ensure Authentication node doesn't have an ApiKey hardcoded if it exists
        Assert.DoesNotContain("\"ApiKey\":", json);
    }
}
