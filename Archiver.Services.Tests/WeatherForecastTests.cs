using Archiver.Shared;
using Xunit;

namespace Archiver.Services.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]       // Freezing point
    [InlineData(100, 212)]    // Boiling point
    [InlineData(-40, -40)]    // Parity point
    [InlineData(25, 77)]      // Standard warm day
    [InlineData(1, 33)]       // Just above freezing: 32 + 1.8 -> 33
    [InlineData(-1, 31)]      // Just below freezing: 32 - 1.8 -> 31
    [InlineData(37, 98)]      // Body temperature: 32 + 66.6 -> 98 (truncation)
    [InlineData(-10, 14)]     // Cold day: 32 - 18 -> 14
    [InlineData(26, 78)]      // Truncation check: 26 * 1.8 = 46.8 -> 32 + 46 = 78
    public void TemperatureF_CalculatesCorrectly(int tempC, int expectedTempF)
    {
        // Arrange
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), tempC, "Test");

        // Act
        var actualTempF = forecast.TemperatureF;

        // Assert
        Assert.Equal(expectedTempF, actualTempF);
    }
}
